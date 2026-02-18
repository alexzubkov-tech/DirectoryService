using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared;

namespace DirectoryService.Domain.Locations;

public class Location
{
    // для ef core
    private Location()
    {
    }

    private readonly List<DepartmentLocation> _departmentLocations = new();

    private Location(LocationName locationName, LocationAddress locationAddress, LocationTimeZone timezone)
    {
        Id = Guid.NewGuid();
        LocationName = locationName;
        LocationAddress = locationAddress;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; }

    public LocationName LocationName { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public LocationTimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location, Error> Create(LocationName name, LocationAddress locationAddress, LocationTimeZone timezone)
        => new Location(name, locationAddress, timezone);

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}