namespace DirectoryService.Domain.Entities;

public class DepartmentPosition
{
    public DepartmentPosition(Guid departmentId, Guid positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid PositionId { get; private set; }
}