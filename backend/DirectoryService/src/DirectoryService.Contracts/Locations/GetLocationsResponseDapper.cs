namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponseDapper
(
    IReadOnlyList<LocationDtoDapper> Items,
    long TotalCount,
    int Page,
    int PageSize,
    int TotalPages);