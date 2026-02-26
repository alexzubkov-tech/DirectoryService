using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public sealed record DepartmentPath
{
    private const char SEPARATOR = '/';

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
        => new DepartmentPath(identifier.Value);

    public DepartmentPath CreateChild(DepartmentIdentifier childIdentifier)
        => new DepartmentPath(Value + SEPARATOR + childIdentifier.Value);
}

