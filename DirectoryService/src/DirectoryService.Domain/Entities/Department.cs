using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Entities;

public class Department
{
    private List<Department> _children = new ();
    private List<DepartmentLocation> _departmentLocations = new ();
    private List<DepartmentPosition> _departmentPositions = new ();

    private Department(
        DepartmentName name,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        short depth,
        Department? parent)
    {
        Id = Guid.NewGuid();
        Name = name;
        DepartmentIdentifier = departmentIdentifier;
        DepartmentPath = departmentPath;
        Depth = depth;
        Parent = parent;
        ParentId = parent?.Id;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; }

    public DepartmentName Name { get; private set; }

    public DepartmentIdentifier DepartmentIdentifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Department? Parent { get; private set; }

    public DepartmentPath DepartmentPath { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department> Create(DepartmentName name, DepartmentIdentifier departmentIdentifier, Department? parent = null)
    {
        var pathResult = DepartmentPath.Create(departmentIdentifier, parent?.DepartmentPath);
        if (pathResult.IsFailure)
            return Result.Failure<Department>(pathResult.Error);

        var depth = parent is null ? (short)1 : (short)(parent.Depth + 1);

        var department = new Department(name, departmentIdentifier, pathResult.Value, depth, parent);

        return Result.Success(department);
    }

    public Result Rename(DepartmentName newName)
    {
        Name = newName;
        return Result.Success();
    }

    public void AddChild(Department child)
    {
        _children.Add(child);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLocation(Guid locationId)
    {
        // добавляем локацию
    }

    public void RemoveLocation(Guid locationId)
    {
        // удаляем локацию
    }

    public void AddPosition(Guid positionId)
    {
        // добавляем должность
    }

    public void RemovePosition(Guid positionId)
    {
        // удаляем должность
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}


