using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Application.ReferenceValidation;

public interface IReferenceValidator
{
    Task<Result<IReadOnlyList<Guid>, Errors>> ExistAndActiveDepartmentsAsync(
        IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<Guid>, Errors>> ExistAndActiveLocationsAsync(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken);
}