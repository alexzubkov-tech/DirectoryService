using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.ValueObjects;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    // для ef core
    private Department()
    {
    }

    private readonly List<Department> _childrenDepartments = [];
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];

    private Department(
        DepartmentId id,
        DepartmentName departmentName,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        short depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id;
        DepartmentName = departmentName;
        DepartmentIdentifier = departmentIdentifier;
        DepartmentPath = departmentPath;
        Depth = depth;
        IsActive = true;
        _departmentLocations = departmentLocations.ToList();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public DepartmentId Id { get; private set; } = null!;

    public DepartmentName DepartmentName { get; private set; } = null!;

    public DepartmentIdentifier DepartmentIdentifier { get; private set; } = null!;

    public DepartmentId? ParentId { get; private set; }

    public DepartmentPath DepartmentPath { get; private set; } = null!;

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department, Error> CreateParent(
      DepartmentName name,
      DepartmentIdentifier identifier,
      IEnumerable<DepartmentLocation> departmentLocations,
      DepartmentId? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        var path = DepartmentPath.CreateParent(identifier);
        return new Department(departmentId ?? new DepartmentId(Guid.NewGuid()), name, identifier, path, 0,
            departmentLocationsList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        DepartmentIdentifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        var path = parent.DepartmentPath.CreateChild(identifier);
        return new Department(departmentId ?? new DepartmentId(Guid.NewGuid()), name, identifier, path,  (short)(parent.Depth + 1), departmentLocationsList);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}


