using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions.ValueObjects;
using Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    // для ef core
    private Position()
    {
    }

    private readonly List<DepartmentPosition> _departmentPositions = new();

    private Position(PositionName positionName, PositionDescription positionDescription)
    {
        Id = new PositionId(Guid.NewGuid());
        PositionName = positionName;
        PositionDescription = positionDescription;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public PositionId Id { get; }

    public PositionName PositionName { get; private set; }

    public PositionDescription PositionDescription { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get;  private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Position, Error> Create(PositionName name, PositionDescription description)
    {
        return new Position(name, description);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}