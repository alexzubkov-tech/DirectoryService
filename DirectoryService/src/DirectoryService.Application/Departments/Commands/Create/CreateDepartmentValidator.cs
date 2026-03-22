using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.Errors;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.Create;

public class CreateDepartmentValidator: AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(d => d.Request.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(d => d.Request.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);

        RuleFor(d => d.Request.LocationIds)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithError(DepartmentDomainErrors.List.Empty())
            .NotEmpty().WithError(DepartmentDomainErrors.List.Empty())
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(DepartmentDomainErrors.List.Duplicates());
    }
}