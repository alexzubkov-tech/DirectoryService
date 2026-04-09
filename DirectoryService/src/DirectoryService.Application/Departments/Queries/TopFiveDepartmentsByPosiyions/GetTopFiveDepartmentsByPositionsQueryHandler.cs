using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace DirectoryService.Application.Departments.Queries.TopFiveDepartmentsByPosiyions;

public class GetTopFiveDepartmentsByPositionsQueryHandler :
    IQueryHandler<List<GetTopFiveDepartmentsByPositionsResponse>, GetTopFiveDepartmentsByPositionsQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly HybridCache _cache;

    public GetTopFiveDepartmentsByPositionsQueryHandler(
        IDbConnectionFactory connectionFactory,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<Result<List<GetTopFiveDepartmentsByPositionsResponse>, Errors>> Handle(
        GetTopFiveDepartmentsByPositionsQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _cache.GetOrCreateAsync(
            DepartmentCacheKeys.GetTopFiveDepartmentsByPositions(),
            async token =>
            {
                var connection = await _connectionFactory.CreateConnectionAsync(token);

                var result = await connection.QueryAsync<GetTopFiveDepartmentsByPositionsResponse>(
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
                    """);

                return result.ToList();
            },
            tags: [CacheTags.DEPARTMENTS_LIST],
            cancellationToken: cancellationToken);

        return response;
    }

    private static class DepartmentCacheKeys
    {
        public static string GetTopFiveDepartmentsByPositions()
            => "departments:top-five-by-positions";
    }
}