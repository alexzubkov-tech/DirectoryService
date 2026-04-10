using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;

public class UpdateDepartmentsLocationsValidator: AbstractValidator<UpdateDepartmentsLocationsCommand>
{
    public UpdateDepartmentsLocationsValidator()
    {
        RuleFor(d => d.Request.LocationIds)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithError(DepartmentDomainErrors.List.Empty())
            .NotEmpty().WithError(DepartmentDomainErrors.List.Empty())
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(DepartmentDomainErrors.List.Duplicates());
    }

}