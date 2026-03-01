using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.Errors;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

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
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                && pgEx.ConstraintName.Contains("ix_locations_name", StringComparison.InvariantCultureIgnoreCase))
            {
                return LocationErrors.NameConflict(location.LocationName.Value);
            }

            _logger.LogError(ex, "Database update error while creating location with name {locationName}",
                location.LocationName.Value);

            return LocationErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was canceled while creating location with name {locationName}",
                location.LocationName.Value);
            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating location with name {locationName}",
                location.LocationName.Value);

            return LocationErrors.DatabaseError();

        }
    }

    public async Task<Location?> GetByAddressAsync(LocationAddress address, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .FirstOrDefaultAsync(
                l =>
                    l.LocationAddress.Country == address.Country &&
                    l.LocationAddress.City == address.City &&
                    l.LocationAddress.Street == address.Street &&
                    l.LocationAddress.BuildingNumber == address.BuildingNumber,
                cancellationToken);
    }

    public async Task<Location?> GetByNameAsync(LocationName name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .FirstOrDefaultAsync(l => l.LocationName == name, cancellationToken);
    }
}