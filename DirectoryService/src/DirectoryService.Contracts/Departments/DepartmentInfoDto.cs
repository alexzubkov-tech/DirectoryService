namespace DirectoryService.Contracts.Departments;

public record DepartmentInfoDto
{
    public Guid Id { get; init; }

    public string Identificator { get; init; } = null!;

}