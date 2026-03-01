using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);

    Task<Location?> GetByAddressAsync(LocationAddress address, CancellationToken cancellationToken = default);
    
    Task<Location?> GetByNameAsync(LocationName name, CancellationToken cancellationToken = default);

}