namespace DirectoryService.Contracts.Departments;

public record DepartmentCleanupOptions
{
    public const string SECTION_NAME = "DepartmentCleanup";

    public double IntervalHours { get; init; } = 24;

    public int InactiveDaysThreshold { get; init; } = 30;
}