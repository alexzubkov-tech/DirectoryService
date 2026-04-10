using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    public DepartmentLocationId Id { get; private set; }

    public LocationId LocationId { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        Id = new DepartmentLocationId(Guid.NewGuid());
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}