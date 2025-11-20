using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Entities;

public class Location
{
    private readonly List<DepartmentLocation> _departmentLocations = new();

    private Location(LocationName name, LocationAddress locationAddress, LocationTimeZone timezone)
    {
        Id = Guid.NewGuid();
        Name = name;
        LocationAddress = locationAddress;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; }

    public LocationName Name { get; private set; }

    public LocationAddress LocationAddress { get; private set; }

    public LocationTimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(LocationName name, LocationAddress locationAddress, LocationTimeZone timezone)
        => Result.Success(new Location(name, locationAddress, timezone));

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}