using Dapper;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Locations;

public sealed class LocationsCleanupBackgroundService : BackgroundService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IOptionsMonitor<LocationCleanupOptions> _optionsMonitor;
    private readonly ILogger<LocationsCleanupBackgroundService> _logger;
    private readonly HybridCache _cache;

    public LocationsCleanupBackgroundService(
        IDbConnectionFactory connectionFactory,
        IOptionsMonitor<LocationCleanupOptions> optionsMonitor,
        ILogger<LocationsCleanupBackgroundService> logger,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Location cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;

            _logger.LogInformation(
                "Location cleanup cycle: ThresholdDays={ThresholdDays}, IntervalHours={IntervalHours}",
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
                _logger.LogError(ex, "Error during location cleanup.");
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

        _logger.LogInformation("Location cleanup service stopped.");
    }

    private async Task CleanupAsync(int thresholdDays, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        int deletedCount;

        try
        {
            deletedCount = await connection.ExecuteScalarAsync<int>(
                """
                WITH candidates AS (
                    SELECT location_id
                    FROM locations
                    WHERE is_active = false
                      AND deleted_at < timezone('utc', now()) - make_interval(days => @ThresholdDays)
                    FOR UPDATE
                ),
                deleted_links AS (
                    DELETE FROM department_locations dl
                    USING candidates c
                    WHERE dl.location_id = c.location_id
                    RETURNING dl.location_id
                ),
                deleted_locations AS (
                    DELETE FROM locations l
                    USING candidates c
                    WHERE l.location_id = c.location_id
                      AND l.is_active = false
                      AND l.deleted_at < timezone('utc', now()) - make_interval(days => @ThresholdDays)
                    RETURNING l.location_id
                )
                SELECT COUNT(*) FROM deleted_locations;
                """,
                new { ThresholdDays = thresholdDays },
                transaction);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Location cleanup failed, transaction rolled back.");
            throw;
        }

        if (deletedCount == 0)
        {
            _logger.LogInformation("Location cleanup: nothing to delete.");
            return;
        }

        try
        {
            await _cache.RemoveByTagAsync(CacheTags.LOCATIONS_LIST, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Location cleanup deleted {Count} locations, but cache invalidation failed.",
                deletedCount);
            return;
        }

        _logger.LogInformation(
            "Location cleanup completed. Deleted {Count} locations. Cache invalidated.",
            deletedCount);
    }

    public async Task RunCleanupForTestsAsync(int thresholdDays, CancellationToken cancellationToken)
        => await CleanupAsync(thresholdDays, cancellationToken);
}
