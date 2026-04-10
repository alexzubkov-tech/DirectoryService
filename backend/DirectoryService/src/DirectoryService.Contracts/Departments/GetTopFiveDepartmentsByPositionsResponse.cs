namespace DirectoryService.Contracts.Departments;

public record GetTopFiveDepartmentsByPositionsResponse
{
    public string Name { get; init; } = null!;

    public string Identificator { get; init; } = null!;

    public string DepartmentPath { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public int PositionCount { get; init; }
}