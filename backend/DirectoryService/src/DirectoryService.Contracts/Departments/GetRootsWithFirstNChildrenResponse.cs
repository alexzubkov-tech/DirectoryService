namespace DirectoryService.Contracts.Departments;

public record GetRootsWithFirstNChildrenResponse
{
    public List<DepartmentDto> Items { get; set; } = [];

    public long TotalCount { get; set; }
}