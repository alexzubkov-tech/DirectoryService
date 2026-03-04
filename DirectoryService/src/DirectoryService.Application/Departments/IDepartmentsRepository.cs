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

    Task<bool> ExistsAsync(
        Guid locationId,
        CancellationToken cancellationToken);

    Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken ct);

    Task<bool> ExistsByIdentifierAsync(
        DepartmentIdentifier identifier,
        CancellationToken cancellationToken);

}