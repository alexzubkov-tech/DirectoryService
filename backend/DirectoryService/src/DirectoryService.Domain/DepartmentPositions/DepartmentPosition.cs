using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;

namespace DirectoryService.Domain.DepartmentPositions;

public sealed class DepartmentPosition
{
    public DepartmentPositionId Id { get; private set; }

    public PositionId PositionId { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public DepartmentPosition(PositionId positionId, DepartmentId departmentId)
    {
        Id = new DepartmentPositionId(Guid.NewGuid());
        PositionId = positionId;
        DepartmentId = departmentId;
    }
}