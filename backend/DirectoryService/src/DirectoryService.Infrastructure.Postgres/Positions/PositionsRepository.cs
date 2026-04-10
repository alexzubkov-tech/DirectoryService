using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Positions.Fails;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.Errors;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure.Positions.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Positions;

public class PositionsRepository: IPositionsRepository
{
     private readonly ILogger<PositionsRepository> _logger;
     private readonly DirectoryServiceDbContext _dbContext;

     public PositionsRepository(DirectoryServiceDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

     public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        await _dbContext.Positions.AddAsync(position, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Error>(position.Id.Value);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName?.Contains("ix_positions_name_active", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return PositionApplicationErrors.NameAlreadyExists(position.PositionName.Value);
            }

            _logger.LogError(ex, "Database update error while creating position with name {positionName}",
                position.PositionName.Value);
            return PositionInfrastructureErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was canceled while creating position with name {positionName}",
                position.PositionName.Value);
            return PositionInfrastructureErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating position with name {positionName}",
                position.PositionName.Value);
            return PositionInfrastructureErrors.DatabaseError();
        }
    }

     public async Task<Result<Position, Error>> GetBy(
        Expression<Func<Position, bool>> predicate,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Positions.AsQueryable();

        if (includeInactive)
        {
            query = query.IgnoreQueryFilters();
        }

        var position = await query.FirstOrDefaultAsync(predicate, cancellationToken);

        if (position is null)
        {
            return PositionApplicationErrors.NotFound();
        }

        return position;
    }
}