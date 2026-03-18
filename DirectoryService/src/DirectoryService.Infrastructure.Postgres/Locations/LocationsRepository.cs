using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure.Locations.Errors;
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
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName?.Contains("ix_locations_name", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return LocationApplicationErrors.AlreadyExistsByName(location.LocationName.Value);
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

    public async Task<Location?> GetByAddressAsync(LocationAddress address, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .FirstOrDefaultAsync(
                l => l.LocationAddress.Country == address.Country &&
                     l.LocationAddress.City == address.City &&
                     l.LocationAddress.Street == address.Street &&
                     l.LocationAddress.BuildingNumber == address.BuildingNumber,
                cancellationToken);
    }

    public async Task<Location?> GetByNameAsync(LocationName name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .FirstOrDefaultAsync(l => l.LocationName.Value == name.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .IgnoreQueryFilters()
            .AsNoTracking().ToListAsync(cancellationToken);
    }

    // игнорирую фильтр, чтобы конкретизировать ошибку - локация не найдена либо не активна
    public async Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var id = new LocationId(locationId);
        return await _dbContext.Locations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
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
}