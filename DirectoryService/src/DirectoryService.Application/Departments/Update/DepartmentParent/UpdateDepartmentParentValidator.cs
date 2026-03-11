using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Update.DepartmentParent;

public class UpdateDepartmentParentValidator: AbstractValidator<UpdateDepartmentParentCommand>
{
    public UpdateDepartmentParentValidator()
    {
        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty().WithError(DepartmentDomainErrors.Id.Empty())
            .NotNull().WithError(DepartmentDomainErrors.Id.Null());

        RuleFor(x => x.Request.ParentId)
            .Must(id => id != Guid.Empty)
            .When(x => x.Request.ParentId.HasValue)
            .WithError(DepartmentDomainErrors.Id.ParentIdEmpty());
    }
}