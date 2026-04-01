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
        var candidates = (await connection.QueryAsync<(Guid Id, Guid? ParentId, string Path)>(
            """
            SELECT
                department_id         AS Id,
                parent_id             AS ParentId,
                department_path::text AS Path
            FROM departments
            WHERE is_active = false
              AND deleted_at < timezone('utc', now()) - make_interval(days => @ThresholdDays)
            ORDER BY depth DESC
            """,
            new { ThresholdDays = thresholdDays },
            transaction)).ToList();

        if (candidates.Count == 0)
        {
            transaction.Commit();
            _logger.LogInformation("Cleanup: nothing to delete.");
            return;
        }

        var candidateIds = candidates.Select(c => c.Id).ToArray();

        _logger.LogInformation("Found {Count} candidates for deletion.", candidates.Count);

        await connection.ExecuteAsync(
            """
            WITH candidates AS (
                SELECT
                    department_id,
                    department_path                                                              AS deleted_path,
                    NULLIF(subpath(department_path, 0, nlevel(department_path) - 1)::text, '')  AS parent_path
                FROM departments
                WHERE department_id = ANY(@CandidateIds)
            ),
            nearest_candidate AS (
                SELECT DISTINCT ON (d.department_id)
                    d.department_id AS child_id,
                    c.deleted_path,
                    c.parent_path
                FROM departments d
                JOIN candidates c
                    ON d.department_path <@ c.deleted_path::ltree
                    AND d.department_path != c.deleted_path::ltree
                WHERE d.is_active = true
                ORDER BY d.department_id, nlevel(c.deleted_path) DESC
            )
            UPDATE departments AS child
            SET
                department_path = CASE
                    WHEN nc.parent_path IS NOT NULL
                        THEN nc.parent_path::ltree
                             || subpath(child.department_path, nlevel(nc.deleted_path::ltree))
                    ELSE subpath(child.department_path, nlevel(nc.deleted_path::ltree))
                END,
                depth = nlevel(CASE
                    WHEN nc.parent_path IS NOT NULL
                        THEN nc.parent_path::ltree
                             || subpath(child.department_path, nlevel(nc.deleted_path::ltree))
                    ELSE subpath(child.department_path, nlevel(nc.deleted_path::ltree))
                END) - 1,
                updated_at = timezone('utc', now())
            FROM nearest_candidate nc
            WHERE child.department_id = nc.child_id
            """,
            new { CandidateIds = candidateIds },
            transaction);

        await connection.ExecuteAsync(
            """
            WITH candidates AS (
                SELECT department_id, parent_id
                FROM departments
                WHERE department_id = ANY(@CandidateIds)
            )
            UPDATE departments
            SET
                parent_id  = c.parent_id,
                updated_at = timezone('utc', now())
            FROM candidates c
            WHERE departments.parent_id = c.department_id
              AND departments.is_active = true
            """,
            new { CandidateIds = candidateIds },
            transaction);

        await connection.ExecuteAsync(
            """
            WITH
            del_locations AS (
                DELETE FROM department_locations
                WHERE department_id = ANY(@CandidateIds)
            ),
            del_positions AS (
                DELETE FROM department_positions
                WHERE department_id = ANY(@CandidateIds)
            ),
            del_departments AS (
                DELETE FROM departments
                WHERE department_id = ANY(@CandidateIds)
            )
            SELECT 1
            """,
            new { CandidateIds = candidateIds },
            transaction);

        transaction.Commit();

        _logger.LogInformation(
            "Cleanup completed. Deleted {Count} departments.",
            candidates.Count);
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