namespace DirectoryService.Contracts.Departments;

public record GetRootsWithFirstNChildrenRequest()
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public int Prefetch { get; init; } = 3;
}