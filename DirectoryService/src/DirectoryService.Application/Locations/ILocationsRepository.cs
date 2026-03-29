using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default);

    Task<List<Location>> GetListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<Result<Location, Error>> GetBy(
        Expression<Func<Location, bool>> predicate,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

}