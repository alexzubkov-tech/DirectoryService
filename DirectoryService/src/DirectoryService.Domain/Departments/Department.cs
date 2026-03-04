using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.Errors;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
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
        DepartmentId? parentId,
        DepartmentName departmentName,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        short depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id;
        ParentId = parentId;
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
        IEnumerable<LocationId> locationIds,
        DepartmentId? departmentId = null)
    {
        var locationIdsList = locationIds.ToList();
        if (locationIdsList.Count == 0)
            return DepartmentDomainErrors.List.Empty();

        if (locationIdsList.Count != locationIdsList.Distinct().Count())
            return DepartmentDomainErrors.List.Duplicates();

        var id = new DepartmentId(Guid.NewGuid());

        var path = DepartmentPath.CreateParent(identifier);

        // добавление связи
        var departmentLocations = locationIdsList
            .Select(locId => new DepartmentLocation(id, locId))
            .ToList();

        return new Department(id, null, name, identifier, path, 0, departmentLocations);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        DepartmentIdentifier identifier,
        Department parent,
        IEnumerable<LocationId> locationIds,
        DepartmentId? departmentId = null)
    {
        var locationIdsList = locationIds.ToList();
        if (locationIdsList.Count == 0)
            return DepartmentDomainErrors.List.Empty();

        if (locationIdsList.Count != locationIdsList.Distinct().Count())
            return DepartmentDomainErrors.List.Duplicates();

        var id = new DepartmentId(Guid.NewGuid());

        var path = parent.DepartmentPath.CreateChild(identifier);

        var departmentLocations = locationIdsList
            .Select(locId => new DepartmentLocation(id, locId))
            .ToList();

        return new Department(id, parent.Id, name, identifier, path, (short)(parent.Depth + 1), departmentLocations);
    }


    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}


