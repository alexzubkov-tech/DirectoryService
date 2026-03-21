using DirectoryService.Contracts.Departments;

namespace DirectoryService.Contracts.Locations;

public record LocationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public AddressDto Address { get; init; } = null!;

    public string TimeZone { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<DepartmentInfoDto> Departments { get; init; } = new List<DepartmentInfoDto>();
}