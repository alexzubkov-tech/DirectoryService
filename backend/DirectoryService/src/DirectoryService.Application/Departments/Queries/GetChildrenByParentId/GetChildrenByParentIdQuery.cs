using Core.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParentId;

public record GetChildrenByParentIdQuery(Guid ParentId,  GetChildrenByParentIdRequest Request) : IQuery;