using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParentId;

public class GetChildrenByParentIdQueryHandler :
    IQueryHandler<GetChildrenByParentIdResponse, GetChildrenByParentIdQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IValidator<GetChildrenByParentIdQuery> _validator;

    public GetChildrenByParentIdQueryHandler(
        IDbConnectionFactory connectionFactory,
        IValidator<GetChildrenByParentIdQuery> validator)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
    }

    public async Task<Result<GetChildrenByParentIdResponse, Errors>> Handle(
        GetChildrenByParentIdQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToListError();
        }

        var parentId = query.ParentId;
        var request = query.Request;
        int offset = (request.Page - 1) * request.PageSize;

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new
        {
            parent_id = parentId,
            offset = offset,
            limit = request.PageSize,
        };

        const string checkParentSql = """
                                      SELECT EXISTS(
                                          SELECT 1 
                                          FROM departments 
                                          WHERE department_id = @parent_id
                                      )
                                      """;

        bool parentExists = await connection.ExecuteScalarAsync<bool>(checkParentSql, parameters);
        if (!parentExists)
        {
            return DepartmentApplicationErrors.ParentNotFound(parentId).ToErrors();
        }

        const string countSql = """
                                SELECT COUNT(*)
                                FROM departments
                                WHERE parent_id = @parent_id
                                  AND is_active = TRUE
                                """;

        long totalCount = await connection.ExecuteScalarAsync<long>(countSql, parameters);

        const string dataSql = """
                               SELECT
                                   d.department_id AS Id,
                                   d.parent_id AS ParentId,
                                   d.department_name AS Name,
                                   d.department_identifier AS Identificator,
                                   d.department_path AS Path,
                                   CAST(d.depth AS VARCHAR) AS Depth,
                                   d.is_active AS IsActive,
                                   d.created_at AS CreatedAt,
                                   d.updated_at AS UpdatedAt,
                                   EXISTS(
                                       SELECT 1 
                                       FROM departments 
                                       WHERE parent_id = d.department_id
                                         AND is_active = TRUE
                                       LIMIT 1
                                   ) AS HasMoreChildren
                               FROM departments d
                               WHERE d.parent_id = @parent_id
                                 AND d.is_active = TRUE
                               ORDER BY d.created_at, d.department_id
                               OFFSET @offset
                               LIMIT @limit
                               """;

        var rawDepartments = await connection.QueryAsync<DepartmentDto>(dataSql, parameters);
        var childrenList = rawDepartments.ToList();

        foreach (var child in childrenList)
        {
            child.Children = null;
        }

        var response = new GetChildrenByParentIdResponse
        {
            Items = childrenList,
            TotalCount = totalCount,
        };

        return response;
    }
}