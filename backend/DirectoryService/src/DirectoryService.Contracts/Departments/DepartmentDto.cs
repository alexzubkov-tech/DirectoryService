using System.Text.Json.Serialization;

namespace DirectoryService.Contracts.Departments;

public record DepartmentDto()
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Identificator { get; set; } = null!;

    public string Path { get; set; } = null!;

    public string Depth { get; init; } = null!;

    public bool IsActive { get; init; }

    public Guid? ParentId { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdtedAt { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DepartmentDto>? Children { get; set; }

    public bool HasMoreChildren { get; set; }
}