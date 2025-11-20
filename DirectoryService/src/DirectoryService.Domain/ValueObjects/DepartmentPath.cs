using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentPath
{
    private DepartmentPath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentPath> Create(DepartmentIdentifier departmentIdentifier, DepartmentPath? parentPath = null)
    {
        var path = parentPath is null ? departmentIdentifier.Value : $"{parentPath.Value}.{departmentIdentifier.Value}";
        return Result.Success(new DepartmentPath(path));
    }

    public override string ToString() => Value;
}

