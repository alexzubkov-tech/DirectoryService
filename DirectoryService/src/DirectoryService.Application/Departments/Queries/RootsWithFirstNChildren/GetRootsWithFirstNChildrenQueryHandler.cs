using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;

public class GetRootsWithFirstNChildrenQueryHandler :
    IQueryHandler<GetRootsWithFirstNChildrenResponse, GetRootsWithFirstNChildrenQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IValidator<GetRootsWithFirstNChildrenQuery> _validator;

    public GetRootsWithFirstNChildrenQueryHandler(
        IDbConnectionFactory connectionFactory,
        IValidator<GetRootsWithFirstNChildrenQuery> validator)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
    }

    public async Task<Result<GetRootsWithFirstNChildrenResponse, Errors>> Handle(
        GetRootsWithFirstNChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToListError();
        }

        var request = query.Request;
        int offset = (request.Page - 1) * request.PageSize;

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new
        {
            offset,
            root_limit = request.PageSize,
            child_limit = request.Prefetch,
        };

        const string countSql = """
                                SELECT COUNT(*)
                                FROM departments
                                WHERE parent_id IS NULL
                                  AND is_active = TRUE
                                """;
        long totalCount = await connection.ExecuteScalarAsync<long>(countSql);

        const string sql = """
                           WITH roots AS (
                               SELECT
                                   d.department_id,
                                   d.parent_id,
                                   d.department_name,
                                   d.department_identifier,
                                   d.department_path,
                                   d.depth,
                                   d.is_active,
                                   d.created_at,
                                   d.updated_at
                               FROM departments d
                               WHERE d.parent_id IS NULL
                               ORDER BY d.created_at
                               OFFSET @offset
                               LIMIT @root_limit
                           )
                           
                           SELECT
                               r.department_id AS Id,
                               r.parent_id AS ParentId,
                               r.department_name AS Name,
                               r.department_identifier AS Identificator,
                               r.department_path AS Path,
                               CAST(r.depth AS VARCHAR) AS Depth,
                               r.is_active AS IsActive,
                               r.created_at AS CreatedAt,
                               r.updated_at AS UpdatedAt,
                               EXISTS(
                                   SELECT 1 
                                   FROM departments 
                                   WHERE parent_id = r.department_id 
                                   OFFSET @child_limit 
                                   LIMIT 1
                               ) AS HasMoreChildren
                           FROM roots r
                           
                           UNION ALL
                           
                           SELECT
                               c.department_id AS Id,
                               c.parent_id AS ParentId,
                               c.department_name AS Name,
                               c.department_identifier AS Identificator,
                               c.department_path AS Path,
                               CAST(c.depth AS VARCHAR) AS Depth,
                               c.is_active AS IsActive,
                               c.created_at AS CreatedAt,
                               c.updated_at AS UpdatedAt,
                               EXISTS(
                                   SELECT 1 
                                   FROM departments 
                                   WHERE parent_id = c.department_id
                                   LIMIT 1
                               ) AS HasMoreChildren
                           FROM roots r 
                           CROSS JOIN LATERAL (
                               SELECT
                                   d.department_id,
                                   d.parent_id,
                                   d.department_name,
                                   d.department_identifier,
                                   d.department_path,
                                   d.depth,
                                   d.is_active,
                                   d.created_at,
                                   d.updated_at
                               FROM departments d
                               WHERE d.parent_id = r.department_id
                                   AND d.is_active = true
                               ORDER BY d.created_at
                               LIMIT @child_limit
                           ) c
                           """;

        var rawDepartments = await connection.QueryAsync<DepartmentDto>(sql, parameters);
        var departmentList = rawDepartments.ToList();

        var departmentsDict = departmentList.ToDictionary(x => x.Id);
        var roots = new List<DepartmentDto>();

        foreach (var department in departmentList)
        {
            if (!department.ParentId.HasValue && department.Children == null)
            {
                department.Children = new List<DepartmentDto>();
            }

            if (department.ParentId.HasValue &&
                departmentsDict.TryGetValue(department.ParentId.Value, out var parent))
            {
                parent.Children ??= new List<DepartmentDto>();

                parent.Children.Add(department);
            }
            else
            {
                roots.Add(department);
            }
        }

        var response = new GetRootsWithFirstNChildrenResponse
        {
            Items = roots,
            TotalCount = totalCount,
        };

        return response;
    }
}