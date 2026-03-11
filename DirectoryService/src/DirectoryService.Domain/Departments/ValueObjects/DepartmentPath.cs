using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public sealed record DepartmentPath
{
    private const char SEPARATOR = '.';

    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static DepartmentPath Create(string value)
        => new DepartmentPath(value);

    public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
        => new DepartmentPath(identifier.Value);

    public DepartmentPath CreateChild(DepartmentIdentifier childIdentifier)
        => new DepartmentPath(Value + SEPARATOR + childIdentifier.Value);
}

