using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(
        Department department,
        CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(
        Guid id,
        CancellationToken ct);

    Task<Department?> GetByIdentifierAsync(
        DepartmentIdentifier identifier,
        CancellationToken cancellationToken);

    Task<List<Department>> GetListByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteLocationsByDepartmentIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<bool> IsDescendantAsync(
        string candidateParentPath,
        string departmentPath,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> LockDescendantsAsync(
        string rootPath,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateDescendantsPathAsync(
        string oldPath,
        string newPath,
        CancellationToken cancellationToken);
}