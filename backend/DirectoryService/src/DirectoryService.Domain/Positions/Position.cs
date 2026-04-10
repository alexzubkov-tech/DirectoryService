using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.Errors;
using DirectoryService.Domain.Positions.ValueObjects;
using Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
     private Position()
    {
    }

     private readonly List<DepartmentPosition> _departmentPositions = new();

     private Position(
        PositionId id,
        PositionName positionName,
        PositionDescription positionDescription,
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = id;
        PositionName = positionName;
        PositionDescription = positionDescription;
        IsActive = true;
        _departmentPositions = departmentPositions.ToList();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

     public PositionId Id { get; }

     public PositionName PositionName { get; private set; }

     public PositionDescription PositionDescription { get; private set; }

     public bool IsActive { get; private set; }

     public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

     public DateTime CreatedAt { get; private set; }

     public DateTime UpdatedAt { get; private set; }

     public DateTime? DeletedAt { get; private set; }

     public static Result<Position, Error> Create(
        PositionName name,
        PositionDescription description,
        IEnumerable<DepartmentId> departmentIds)
    {
        var deptList = departmentIds.ToList();
        if (deptList.Count == 0)
            return PositionDomainErrors.List.Empty();

        if (deptList.Count != deptList.Distinct().Count())
            return PositionDomainErrors.List.Duplicates();

        var id = new PositionId(Guid.NewGuid());

        // добавление связи
        var departmentPositions = deptList
            .Select(deptId => new DepartmentPosition(id, deptId))
            .ToList();

        return new Position(id, name, description, departmentPositions);
    }

     public void Deactivate()
     {
         IsActive = false;
         DeletedAt = DateTime.UtcNow;
         UpdatedAt = DateTime.UtcNow;
    }
}