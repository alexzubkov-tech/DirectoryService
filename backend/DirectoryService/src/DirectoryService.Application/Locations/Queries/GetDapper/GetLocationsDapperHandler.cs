using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Queries.GetDapper;

public class GetLocationsDapperHandler
    : IQueryHandler<GetLocationsResponseDapper, GetLocationsDapperQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IValidator<GetLocationsDapperQuery> _validator;

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

        int page = request.Page;
        int pageSize = request.PageSize;
        int offset = (page - 1) * pageSize;

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new
        {
            DepartmentIds = request.DepartmentIds ?? Array.Empty<Guid>(),
            Search = request.Search,
            IsActive = request.IsActive,
            PageSize = pageSize,
            Offset = offset,
        };

        const string countSql = """
                                SELECT COUNT(DISTINCT l.location_id)
                                FROM locations l
                                LEFT JOIN department_locations dl ON dl.location_id = l.location_id
                                WHERE
                                    (
                                        COALESCE(@DepartmentIds::uuid[], ARRAY[]::uuid[]) = ARRAY[]::uuid[]
                                        OR dl.department_id = ANY(@DepartmentIds::uuid[])
                                    )
                                    AND (@Search IS NULL OR l.location_name ILIKE '%' || @Search || '%')
                                    AND (@IsActive IS NULL OR l.is_active = @IsActive)
                                """;

        long totalCount = await connection.ExecuteScalarAsync<long>(countSql, parameters);

        int totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        const string dataSql = """
                               WITH filtered_locations AS (
                                   SELECT DISTINCT l.location_id,
                                          l.location_name,
                                          l.timezone,
                                          l.address,
                                          l.is_active,
                                          l.created_at
                                   FROM locations l
                                   LEFT JOIN department_locations dl ON dl.location_id = l.location_id
                                   WHERE
                                       (
                                           COALESCE(@DepartmentIds::uuid[], ARRAY[]::uuid[]) = ARRAY[]::uuid[]
                                           OR dl.department_id = ANY(@DepartmentIds::uuid[])
                                       )
                                       AND (@Search IS NULL OR l.location_name ILIKE '%' || @Search || '%')
                                       AND (@IsActive IS NULL OR l.is_active = @IsActive)
                               ),
                               paginated_ids AS (
                                   SELECT location_id
                                   FROM filtered_locations
                                   ORDER BY created_at, location_name
                                   LIMIT @PageSize OFFSET @Offset
                               )
                               SELECT 
                                   l.location_id AS Id,
                                   l.location_name AS Name,
                                   l.timezone AS TimeZone,
                                   l.address->>'Country' AS Country,
                                   l.address->>'City' AS City,
                                   l.address->>'Street' AS Street,
                                   l.address->>'BuildingNumber' AS BuildingNumber,
                                   l.is_active AS IsActive,
                                   l.created_at AS CreatedAt,
                                   d.department_id AS Id,
                                   d.department_identifier AS Identificator
                               FROM paginated_ids pi
                               LEFT JOIN locations l ON l.location_id = pi.location_id
                               LEFT JOIN department_locations dl ON dl.location_id = l.location_id
                               LEFT JOIN departments d ON dl.department_id = d.department_id
                               ORDER BY l.created_at, l.location_name
                               """;

        var locationsDict = new Dictionary<Guid, LocationDtoDapper>();

        await connection.QueryAsync<LocationDtoDapper, DepartmentInfoDto, LocationDtoDapper>(
            dataSql,
            param: parameters,
            splitOn: "Id",
            map: (location, department) =>
            {
                if (!locationsDict.TryGetValue(location.Id, out var existingLocation))
                {
                    existingLocation = new LocationDtoDapper
                    {
                        Id = location.Id,
                        Name = location.Name,
                        TimeZone = location.TimeZone,
                        Country = location.Country,
                        City = location.City,
                        Street = location.Street,
                        BuildingNumber = location.BuildingNumber,
                        CreatedAt = location.CreatedAt,
                        Departments = new List<DepartmentInfoDto>(),
                    };

                    locationsDict[existingLocation.Id] = existingLocation;
                }

                if (department != null && department.Id != Guid.Empty)
                {
                    existingLocation.Departments.Add(department);
                }

                return existingLocation;
            });

        var response = new GetLocationsResponseDapper(
            Items: locationsDict.Values.ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );

        return response;
    }
}
