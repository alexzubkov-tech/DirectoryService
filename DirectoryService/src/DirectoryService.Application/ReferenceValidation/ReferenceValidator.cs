using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Fails;
using Shared;

namespace DirectoryService.Application.ReferenceValidation;

public class ReferenceValidator: IReferenceValidator
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;

    public ReferenceValidator(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<IReadOnlyList<Guid>, Errors>> ExistAndActiveDepartmentsAsync(IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken)
    {
        var existingDepartments =
            await _departmentsRepository.GetListByIdsAsync(departmentIds, cancellationToken);

        var departmentsDict = existingDepartments.ToDictionary(d => d.Id.Value);

        var errors = new List<Error>();
        var validIds = new List<Guid>();

        foreach (var id in departmentIds)
        {
            if (!departmentsDict.TryGetValue(id, out var department))
            {
                errors.Add(DepartmentApplicationErrors.NotFound(id));
                continue;
            }

            if (!department.IsActive)
            {
                errors.Add(DepartmentApplicationErrors.Inactive(id));
                continue;
            }

            validIds.Add(id);
        }

        if (errors.Any())
            return new Errors(errors);

        return validIds;
    }

    public async Task<Result<IReadOnlyList<Guid>, Errors>> ExistAndActiveLocationsAsync(IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        var existingLocations =
            await _locationsRepository.GetListByIdsAsync(locationIds, cancellationToken);

        var locationsDict = existingLocations.ToDictionary(l => l.Id.Value);

        var errors = new List<Error>();
        var validIds = new List<Guid>();

        foreach (var id in locationIds)
        {
            if (!locationsDict.TryGetValue(id, out var location))
            {
                errors.Add(LocationApplicationErrors.NotFound(id));
                continue;
            }

            if (!location.IsActive)
            {
                errors.Add(LocationApplicationErrors.Inactive(id));
                continue;
            }

            validIds.Add(id);
        }

        if (errors.Any())
            return new Errors(errors);

        return validIds;
    }
}