using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Entities;

public class Position
{
    private readonly List<DepartmentPosition> _departmentPositions = new();

    private Position(PositionName name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; }

    public PositionName Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get;  private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Position> Create(PositionName name, string? description)
    {
        if (description != null && description.Length > 1000)
            return Result.Failure<Position>("Description must not exceed 1000 characters");

        return Result.Success(new Position(name, description));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

}