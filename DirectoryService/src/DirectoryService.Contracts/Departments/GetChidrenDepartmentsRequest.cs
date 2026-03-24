namespace DirectoryService.Contracts.Departments;

public record GetChidrenDepartmentsRequest()
{
    public Guid ParentId { get; init; }

    public int Page { get; init; } = 1;

    public int Size { get; init; } = 20;
}