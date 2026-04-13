namespace DirectoryService.Domain.Departments.ValueObjects;

public sealed record DepartmentPath
{
    private const char SEPARATOR = '.';
    private const string DELETED_MARKER = "deleted";
    private const char LTREE_SAFE_SEPARATOR = '_';

    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static DepartmentPath Create(string value)
        => new DepartmentPath(ToLtreePath(value));

    public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
        => new DepartmentPath(ToLtreeLabel(identifier.Value));

    public DepartmentPath CreateChild(DepartmentIdentifier childIdentifier)
        => new DepartmentPath(Value + SEPARATOR + ToLtreeLabel(childIdentifier.Value));

    public DepartmentPath CreateDeletedPath(DepartmentIdentifier identifier)
    {
        string[] components = Value.Split(SEPARATOR);

        if (components.Length > 0)
        {
            components[components.Length - 1] = $"{DELETED_MARKER}{LTREE_SAFE_SEPARATOR}{ToLtreeLabel(identifier.Value)}";
        }

        return new DepartmentPath(string.Join(SEPARATOR.ToString(), components));
    }

    private static string ToLtreePath(string value)
        => string.Join(SEPARATOR, value.Split(SEPARATOR).Select(ToLtreeLabel));

    private static string ToLtreeLabel(string value)
        => value.Replace('-', LTREE_SAFE_SEPARATOR);
}
