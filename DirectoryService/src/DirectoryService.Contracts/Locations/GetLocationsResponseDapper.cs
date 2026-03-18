namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponseDapper
{
    public IReadOnlyList<LocationDtoDapper> Items { get; init; } = null!;

    public long TotalCount { get; init; }
}