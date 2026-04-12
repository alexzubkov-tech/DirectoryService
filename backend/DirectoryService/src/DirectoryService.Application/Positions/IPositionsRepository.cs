using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);


    Task<Result<Position, Error>> GetBy(
        Expression<Func<Position, bool>> predicate,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}