namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponse
{
    public IReadOnlyList<LocationDto> Items { get; init; } = null!;

    public long TotalCount { get; init; }
}