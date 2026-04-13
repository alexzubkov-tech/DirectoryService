using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Contracts.Departments;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParentId;

public class GetChildrenByParentIdQueryHandler :
    IQueryHandler<GetChildrenByParentIdResponse, GetChildrenByParentIdQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IValidator<GetChildrenByParentIdQuery> _validator;
    private readonly HybridCache _cache;

    public GetChildrenByParentIdQueryHandler(
        IDbConnectionFactory connectionFactory,
        IValidator<GetChildrenByParentIdQuery> validator,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
        _cache = cache;
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

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var checkParameters = new
        {
            parent_id = parentId,
        };

        const string checkParentSql = """
                                      SELECT EXISTS(
                                          SELECT 1 
                                          FROM departments 
                                          WHERE department_id = @parent_id
                                      )
                                      """;

        bool parentExists = await connection.ExecuteScalarAsync<bool>(checkParentSql, checkParameters);
        if (!parentExists)
        {
            return DepartmentApplicationErrors.ParentNotFound(parentId).ToErrors();
        }

        string cacheKey = DepartmentCacheKeys.GetChildrenByParentId(
            parentId,
            request.Page,
            request.PageSize);

        var response = await _cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                int offset = (request.Page - 1) * request.PageSize;

                var parameters = new
                {
                    parent_id = parentId,
                    offset,
                    limit = request.PageSize,
                };

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

                return new GetChildrenByParentIdResponse
                {
                    Items = childrenList,
                    TotalCount = totalCount,
                };
            },
            tags: [CacheTags.DEPARTMENTS_LIST],
            cancellationToken: cancellationToken);

        return response;
    }

    private static class DepartmentCacheKeys
    {
        public static string GetChildrenByParentId(Guid parentId, int page, int pageSize)
            => $"departments:children:parentId={parentId}:page={page}:pageSize={pageSize}";
    }
}