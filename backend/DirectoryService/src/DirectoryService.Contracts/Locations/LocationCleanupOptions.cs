namespace DirectoryService.Contracts.Locations;

public record LocationCleanupOptions
{
    public const string SECTION_NAME = "LocationCleanup";

    public double IntervalHours { get; init; } = 24;

    public int InactiveDaysThreshold { get; init; } = 30;
}