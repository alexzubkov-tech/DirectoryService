using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure.Locations.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.SharedKernel;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository: ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<ILocationsRepository> _logger;

    public LocationsRepository(DirectoryServiceDbContext dbContext, ILogger<ILocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Error>(location.Id.Value);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName?.Contains("ix_locations_name", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return LocationApplicationErrors.AlreadyExistsByName(location.LocationName.Value);
            }

            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName?.Contains("ix_locations_address", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return LocationApplicationErrors.AlreadyExistsByAddress(location.LocationAddress.ToString());
            }

            _logger.LogError(ex, "Database update error while creating location with name {locationName}",
                location.LocationName.Value);
            return LocationInfrastructureErrors.DatabaseError();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating location with name {locationName}",
                location.LocationName.Value);
            return LocationInfrastructureErrors.DatabaseError();
        }
    }

    public async Task<List<Location>> GetListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var locationIds = ids
            .Select(id => new LocationId(id))
            .ToList();

        return await _dbContext.Locations
            .IgnoreQueryFilters()
            .Where(l => locationIds.Contains(l.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<Location, Error>> GetBy(
        Expression<Func<Location, bool>> predicate,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Locations.AsQueryable();

        if (includeInactive)
        {
            query = query.IgnoreQueryFilters();
        }

        var location = await query.FirstOrDefaultAsync(predicate, cancellationToken);

        if (location is null)
        {
            return LocationApplicationErrors.NotFound();
        }

        return location;
    }

    public async Task<Result<Location, Error>> GetByIdWithLock(
        LocationId locationId,
        CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations
            .FromSql($"SELECT * FROM locations WHERE location_id = {locationId.Value} FOR UPDATE")
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            return LocationApplicationErrors.NotFound(locationId.Value);
        }

        return location;
    }

    public async Task<UnitResult<Error>> UpdateAsync(
        Location location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName?.Contains("ix_locations_name", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return LocationApplicationErrors.AlreadyExistsByName(location.LocationName.Value);
            }

            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName?.Contains("ix_locations_address", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return LocationApplicationErrors.AlreadyExistsByAddress(location.LocationAddress.ToString());
            }

            _logger.LogError(ex, "Database update error while updating location with id {LocationId}",
                location.Id.Value);
            return LocationInfrastructureErrors.DatabaseError();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating location with id {LocationId}",
                location.Id.Value);
            return LocationInfrastructureErrors.DatabaseError();
        }
    }
}
