using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Entities;

public class Position
{
    // для ef core
    private Position()
    {
    }

    private readonly List<DepartmentPosition> _departmentPositions = new();

    private Position(PositionName positionName, PositionDescription positionDescription)
    {
        Id = Guid.NewGuid();
        PositionName = positionName;
        PositionDescription = positionDescription;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; }

    public PositionName PositionName { get; private set; }

    public PositionDescription PositionDescription { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get;  private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Position> Create(PositionName name, PositionDescription description)
    {
        return Result.Success(new Position(name, description));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}