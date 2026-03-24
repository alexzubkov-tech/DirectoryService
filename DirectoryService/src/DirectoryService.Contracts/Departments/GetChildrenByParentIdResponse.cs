namespace DirectoryService.Contracts.Departments;

public record GetChildrenByParentIdResponse
{
    public List<DepartmentDto> Items { get; set; } = [];

    public long TotalCount { get; set; }
}