namespace DirectoryService.Contracts.Departments;

public class LocationsPositionsToDeactivateDto
{
    public Guid[]? LocationIds { get; init; }

    public Guid[]? PositionIds { get; init; }
}