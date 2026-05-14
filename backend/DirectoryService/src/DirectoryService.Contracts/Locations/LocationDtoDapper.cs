using DirectoryService.Contracts.Departments;

namespace DirectoryService.Contracts.Locations;

public record LocationDtoDapper
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string TimeZone { get; init; } = null!;

    public string Country { get; init; } = null!;

    public string City { get; init; } = null!;

    public string Street { get; init; } = null!;

    public string BuildingNumber { get; init; } = null!;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public List<DepartmentInfoDto> Departments { get;  init; } = [];
}
