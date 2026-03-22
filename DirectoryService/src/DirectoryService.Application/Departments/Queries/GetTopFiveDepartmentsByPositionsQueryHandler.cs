using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Shared;

namespace DirectoryService.Application.Departments.Queries;

public class GetTopFiveDepartmentsByPositionsQueryHandler(IDbConnectionFactory connectionFactory) :
    IQueryHandler<List<GetTopFiveDepartmentsByPositionsResponse>, GetTopFiveDepartmentsByPositionsQuery>
{
    public async Task<Result<List<GetTopFiveDepartmentsByPositionsResponse>, Errors>> Handle(
        GetTopFiveDepartmentsByPositionsQuery query,
        CancellationToken cancellationToken)
    {
        return Result.Success<List<GetTopFiveDepartmentsByPositionsResponse>, Errors>(
            (await (await connectionFactory.CreateConnectionAsync(cancellationToken))
                .QueryAsync<GetTopFiveDepartmentsByPositionsResponse>(
                    """
                    SELECT 
                        d.department_name AS Name,
                        d.department_identifier AS Identificator,
                        d.department_path AS DepartmentPath,
                        d.created_at AS CreatedAt,
                        COUNT(dp.position_id) AS PositionCount
                    FROM departments d
                    INNER JOIN department_positions dp ON dp.department_id = d.department_id
                    WHERE d.is_active = true
                    GROUP BY d.department_id, d.department_name, d.department_identifier, d.department_path, d.created_at
                    ORDER BY PositionCount DESC, d.department_name ASC
                    LIMIT 5;
                    """,
                    cancellationToken))
            .ToList());
    }
}