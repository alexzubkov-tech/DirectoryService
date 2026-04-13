using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Locations.ValueObjects;
using Shared.SharedKernel;

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
        Id = new LocationId(Guid.NewGuid());
        LocationName = locationName;
        LocationAddress = locationAddress;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public LocationId Id { get; } = null!;

    public LocationName LocationName { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public LocationTimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Result<Location, Error> Create(LocationName name, LocationAddress locationAddress, LocationTimeZone timezone)
        => new Location(name, locationAddress, timezone);

    public void Deactivate()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}