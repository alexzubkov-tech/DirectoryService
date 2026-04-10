using Dapper;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Departments;

public sealed class DepartmentsCleanupBackgroundService : BackgroundService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IOptionsMonitor<CleanupOptions> _optionsMonitor;
    private readonly ILogger<DepartmentsCleanupBackgroundService> _logger;
    private readonly HybridCache _cache;

    public DepartmentsCleanupBackgroundService(
        IDbConnectionFactory connectionFactory,
        IOptionsMonitor<CleanupOptions> optionsMonitor,
        ILogger<DepartmentsCleanupBackgroundService> logger,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Department cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;

            _logger.LogInformation(
                "Cleanup cycle: ThresholdDays={ThresholdDays}, IntervalHours={IntervalHours}",
                options.InactiveDaysThreshold,
                options.IntervalHours);

            try
            {
                await CleanupAsync(options.InactiveDaysThreshold, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during department cleanup.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(options.IntervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Department cleanup service stopped.");
    }

    private async Task CleanupAsync(int thresholdDays, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            var deletedCount = await connection.ExecuteScalarAsync<int>(
                """
                WITH candidates AS (
                    SELECT
                        d.department_id,
                        d.parent_id,
                        d.department_path AS deleted_path,
                        NULLIF(subpath(d.department_path, 0, nlevel(d.department_path) - 1)::text, '') AS parent_path
                    FROM departments d
                    WHERE d.is_active = false
                      AND d.deleted_at < timezone('utc', now()) - make_interval(days => @ThresholdDays)
                ),

                nearest_candidate AS (
                    SELECT DISTINCT ON (child.department_id)
                        child.department_id AS child_id,
                        c.department_id     AS deleted_id,
                        c.parent_id         AS new_parent_id,
                        c.deleted_path,
                        c.parent_path
                    FROM departments child
                    JOIN candidates c
                        ON child.department_path <@ c.deleted_path
                       AND child.department_path != c.deleted_path
                    WHERE child.is_active = true
                    ORDER BY child.department_id, nlevel(c.deleted_path) DESC
                ),

                updated_children AS (
                    UPDATE departments AS child
                    SET
                        parent_id = CASE
                            WHEN child.parent_id = nc.deleted_id THEN nc.new_parent_id
                            ELSE child.parent_id
                        END,

                        department_path = CASE
                            WHEN nc.parent_path IS NULL
                                THEN subpath(child.department_path, nlevel(nc.deleted_path))
                            ELSE nc.parent_path::ltree || subpath(child.department_path, nlevel(nc.deleted_path))
                        END,

                        depth = nlevel(
                            CASE
                                WHEN nc.parent_path IS NULL
                                    THEN subpath(child.department_path, nlevel(nc.deleted_path))
                                ELSE nc.parent_path::ltree || subpath(child.department_path, nlevel(nc.deleted_path))
                            END
                        ) - 1,

                        updated_at = timezone('utc', now())
                    FROM nearest_candidate nc
                    WHERE child.department_id = nc.child_id
                    RETURNING child.department_id
                ),

                deleted_locations AS (
                    DELETE FROM department_locations dl
                    USING candidates c
                    WHERE dl.department_id = c.department_id
                    RETURNING dl.department_id
                ),

                deleted_positions AS (
                    DELETE FROM department_positions dp
                    USING candidates c
                    WHERE dp.department_id = c.department_id
                    RETURNING dp.department_id
                ),

                deleted_departments AS (
                    DELETE FROM departments d
                    USING candidates c
                    WHERE d.department_id = c.department_id
                    RETURNING d.department_id
                )

                SELECT COUNT(*) FROM deleted_departments;
                """,
                new { ThresholdDays = thresholdDays },
                transaction);

            transaction.Commit();

            if (deletedCount == 0)
            {
                _logger.LogInformation("Cleanup: nothing to delete.");
                return;
            }

            await _cache.RemoveByTagAsync(CacheTags.DEPARTMENTS_LIST, cancellationToken);

            _logger.LogInformation(
                "Cache invalidated by tag {CacheTag} after departments cleanup.",
                CacheTags.DEPARTMENTS_LIST);

            _logger.LogInformation(
                "Cleanup completed. Deleted {Count} departments. Cache invalidated by tag {CacheTag}.",
                deletedCount,
                CacheTags.DEPARTMENTS_LIST);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Cleanup failed, transaction rolled back.");
            throw;
        }
    }

    public async Task RunCleanupForTestsAsync(int thresholdDays, CancellationToken cancellationToken)
        => await CleanupAsync(thresholdDays, cancellationToken);
}