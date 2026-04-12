using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(
        Department department,
        CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetBy(
        Expression<Func<Department, bool>> predicate,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

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

    Task<UnitResult<Error>> DeactivateUnusedLocationsAndPositionsAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken);
}