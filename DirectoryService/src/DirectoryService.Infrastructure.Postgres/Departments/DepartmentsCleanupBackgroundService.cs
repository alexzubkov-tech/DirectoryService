using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Departments;

public sealed class DepartmentsCleanupBackgroundService : BackgroundService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IOptionsMonitor<CleanupOptions> _optionsMonitor;
    private readonly ILogger<DepartmentsCleanupBackgroundService> _logger;

    public DepartmentsCleanupBackgroundService(
        IDbConnectionFactory connectionFactory,
        IOptionsMonitor<CleanupOptions> optionsMonitor,
        ILogger<DepartmentsCleanupBackgroundService> logger)
    {
        _connectionFactory = connectionFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Department cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;
            _logger.LogInformation("Cleanup cycle: ThresholdDays={ThresholdDays}, IntervalHours={IntervalHours}",
                options.InactiveDaysThreshold, options.IntervalHours);

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
            const string candidatesSql = """
                SELECT
                    d.department_id         AS Id,
                    d.parent_id             AS ParentId,
                    d.department_path::text AS Path
                FROM departments d
                WHERE d.is_active = false
                  AND d.deleted_at < timezone('utc', now()) - make_interval(days => @ThresholdDays)
                ORDER BY d.depth DESC
                """;

            var candidates = (await connection.QueryAsync<(Guid Id, Guid? ParentId, string Path)>(
                candidatesSql,
                new { ThresholdDays = thresholdDays },
                transaction)).ToList();

            if (candidates.Count == 0)
            {
                transaction.Commit();
                _logger.LogInformation("Cleanup: nothing to delete (ThresholdDays={ThresholdDays}).", thresholdDays);
                return;
            }

            _logger.LogInformation("Found {Count} candidates for deletion.", candidates.Count);

            foreach (var cand in candidates)
            {
                const string updatePathsSql = """
                    WITH parent_path_cte AS (
                        SELECT NULLIF(
                            subpath(@deletedPath::ltree, 0, nlevel(@deletedPath::ltree) - 1)::text,
                            ''
                        ) AS parent_path
                    )
                    UPDATE departments AS child
                    SET
                        department_path = CASE
                            WHEN (SELECT parent_path FROM parent_path_cte) IS NOT NULL
                                THEN (
                                    (SELECT parent_path FROM parent_path_cte)::ltree
                                    || subpath(child.department_path, nlevel(@deletedPath::ltree))
                                )
                            ELSE subpath(child.department_path, nlevel(@deletedPath::ltree))
                        END,
                        depth = nlevel(
                            CASE
                                WHEN (SELECT parent_path FROM parent_path_cte) IS NOT NULL
                                    THEN (
                                        (SELECT parent_path FROM parent_path_cte)::ltree
                                        || subpath(child.department_path, nlevel(@deletedPath::ltree))
                                    )
                                ELSE subpath(child.department_path, nlevel(@deletedPath::ltree))
                            END
                        ) - 1,
                        updated_at = timezone('utc', now())
                    WHERE child.department_path <@ @deletedPath::ltree
                      AND child.department_path != @deletedPath::ltree
                      AND child.is_active = true
                    """;

                await connection.ExecuteAsync(updatePathsSql, new { deletedPath = cand.Path }, transaction);

                if (cand.ParentId.HasValue)
                {
                    const string updateParentSql = """
                        UPDATE departments
                        SET parent_id  = @newParentId,
                            updated_at = timezone('utc', now())
                        WHERE parent_id = @deletedId
                          AND is_active = true
                        """;
                    await connection.ExecuteAsync(updateParentSql,
                        new { newParentId = cand.ParentId.Value, deletedId = cand.Id },
                        transaction);
                }
                else
                {
                    const string updateParentNullSql = """
                        UPDATE departments
                        SET parent_id  = NULL,
                            updated_at = timezone('utc', now())
                        WHERE parent_id = @deletedId
                          AND is_active = true
                        """;
                    await connection.ExecuteAsync(updateParentNullSql,
                        new { deletedId = cand.Id },
                        transaction);
                }

                const string deleteSql = """
                    WITH
                    del_locations AS (
                        DELETE FROM department_locations WHERE department_id = @deletedId
                    ),
                    del_positions AS (
                        DELETE FROM department_positions WHERE department_id = @deletedId
                    ),
                    del_dept AS (
                        DELETE FROM departments WHERE department_id = @deletedId
                    )
                    SELECT 1
                    """;
                await connection.ExecuteAsync(deleteSql, new { deletedId = cand.Id }, transaction);
            }

            transaction.Commit();
            _logger.LogInformation("Cleanup completed. Deleted {Count} departments.", candidates.Count);
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