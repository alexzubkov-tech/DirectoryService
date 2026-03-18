using System.Text.Json;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Locations.Queries.GetDapper;

public class GetLocationsDapperHandler : IQueryHandler<GetLocationsResponseDapper, GetLocationsDapperQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IValidator<GetLocationsDapperQuery> _validator;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GetLocationsDapperHandler(
        IDbConnectionFactory connectionFactory,
        IValidator<GetLocationsDapperQuery> validator)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
    }

    public async Task<Result<GetLocationsResponseDapper, Errors>> Handle(
        GetLocationsDapperQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToListError();

        var request = query.Request;
        int page = request.Pagination?.Page ?? 1;
        int pageSize = request.Pagination?.PageSize ?? 20;
        int offset = (page - 1) * pageSize;

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new
        {
            DepartmentIds = request.DepartmentIds,
            Search = request.Search,
            IsActive = request.IsActive,
            PageSize = pageSize,
            Offset = offset,
        };

        const string countSql = """
            SELECT COUNT(*)
            FROM locations l
            WHERE
                (
                    COALESCE(@DepartmentIds, ARRAY[]::uuid[]) = ARRAY[]::uuid[]
                    OR EXISTS (
                        SELECT 1
                        FROM department_locations dl
                        WHERE dl.location_id = l.location_id
                          AND dl.department_id = ANY(COALESCE(@DepartmentIds, ARRAY[]::uuid[]))
                    )
                )
                AND (@Search IS NULL OR l.location_name ILIKE '%' || @Search || '%')
                AND (@IsActive IS NULL OR l.is_active = @IsActive)
            """;

        long totalCount = await connection.ExecuteScalarAsync<long>(countSql, parameters);

        string dataSql = """
            SELECT jsonb_build_object(
                'Id', l.location_id,
                'Name', l.location_name,
                'TimeZone', l.timezone,
                'Country', COALESCE(l.address->>'Country', ''),
                'City', COALESCE(l.address->>'City', ''),
                'Street', COALESCE(l.address->>'Street', ''),
                'BuildingNumber', COALESCE(l.address->>'BuildingNumber', ''),
                'CreatedAt', l.created_at,
                'Departments', COALESCE(
                    (SELECT jsonb_agg(jsonb_build_object('Id', d.department_id, 'Identificator', d.department_identifier))
                     FROM department_locations dl
                     JOIN departments d ON d.department_id = dl.department_id
                     WHERE dl.location_id = l.location_id), '[]'::jsonb
                )
            )::text AS LocationJson
            FROM (
                SELECT location_id
                FROM locations l
                WHERE
                    (
                        COALESCE(@DepartmentIds, ARRAY[]::uuid[]) = ARRAY[]::uuid[]
                        OR EXISTS (
                            SELECT 1
                            FROM department_locations dl
                            WHERE dl.location_id = l.location_id
                              AND dl.department_id = ANY(COALESCE(@DepartmentIds, ARRAY[]::uuid[]))
                        )
                    )
                    AND (@Search IS NULL OR l.location_name ILIKE '%' || @Search || '%')
                    AND (@IsActive IS NULL OR l.is_active = @IsActive)
                ORDER BY l.created_at, l.location_name
                LIMIT @PageSize OFFSET @Offset
            ) AS paginated
            JOIN locations l ON l.location_id = paginated.location_id
            ORDER BY l.created_at, l.location_name
            """;

        var jsonRows = await connection.QueryAsync<string>(dataSql, parameters);

        var items = new List<LocationDtoDapper>();
        foreach (string json in jsonRows)
        {
            var dto = JsonSerializer.Deserialize<LocationDtoDapper>(json, _jsonOptions);
            if (dto is not null)
                items.Add(dto);
        }

        var response = new GetLocationsResponseDapper
        {
            Items = items,
            TotalCount = totalCount,
        };

        return Result.Success<GetLocationsResponseDapper, Errors>(response);
    }
}