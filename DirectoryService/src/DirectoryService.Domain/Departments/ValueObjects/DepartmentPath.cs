using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObjects;

public sealed record DepartmentPath
{
    private const char SEPARATOR = '.';
    private const string DELETED_MARKER = "deleted";

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

    public DepartmentPath CreateDeletedPath(DepartmentIdentifier identifier)
    {
        string[] components = Value.Split(SEPARATOR);

        if (components.Length > 0)
        {
            components[components.Length - 1] = $"{DELETED_MARKER}-{identifier.Value}";
        }

        return new DepartmentPath(string.Join(SEPARATOR.ToString(), components));
    }
}

