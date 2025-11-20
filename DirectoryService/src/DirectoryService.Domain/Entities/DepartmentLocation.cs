namespace DirectoryService.Domain.Entities;

public class DepartmentLocation
{
    public DepartmentLocation(Guid departmentId, Guid locationId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }
}