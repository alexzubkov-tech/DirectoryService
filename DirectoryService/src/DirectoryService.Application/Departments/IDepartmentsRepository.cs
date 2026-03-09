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
}