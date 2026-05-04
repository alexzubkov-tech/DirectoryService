namespace DirectoryService.Contracts.Locations;

public record GetLocationsResponse
(
    IReadOnlyList<LocationDto> Items,
    long TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);