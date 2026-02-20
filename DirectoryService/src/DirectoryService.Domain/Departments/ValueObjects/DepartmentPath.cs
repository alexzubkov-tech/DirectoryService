using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentPath
{
    private DepartmentPath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DepartmentPath FromString(string path) => new DepartmentPath(path);

    public static Result<DepartmentPath> Create(DepartmentIdentifier departmentIdentifier, DepartmentPath? parentPath = null)
    {
        var path = parentPath is null ? departmentIdentifier.Value : $"{parentPath.Value}.{departmentIdentifier.Value}";
        return Result.Success(new DepartmentPath(path));
    }
}

