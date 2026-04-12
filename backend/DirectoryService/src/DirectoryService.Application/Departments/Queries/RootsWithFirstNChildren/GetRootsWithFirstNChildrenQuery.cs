using Core.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;

public record GetRootsWithFirstNChildrenQuery(GetRootsWithFirstNChildrenRequest Request): IQuery;