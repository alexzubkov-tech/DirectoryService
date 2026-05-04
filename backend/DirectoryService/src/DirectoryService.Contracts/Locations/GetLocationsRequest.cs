namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest
{
    public Guid[]? DepartmentIds { get; init; }

    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}