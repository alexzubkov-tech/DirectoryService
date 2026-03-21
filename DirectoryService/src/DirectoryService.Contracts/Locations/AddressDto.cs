namespace DirectoryService.Contracts.Locations;

public record AddressDto
{
    public string Country { get; init; } = null!;

    public string City { get; init; } = null!;

    public string Street { get; init; } = null!;

    public string BuildingNumber { get; init; } = null!;
}