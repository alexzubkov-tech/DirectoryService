using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions.Errors;
using DirectoryService.Domain.Positions.ValueObjects;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionValidator: AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(p => p.Request.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(p => p.Request.Description)
            .MustBeValueObject(PositionDescription.Create);

        RuleFor(p => p.Request.DepartmentIds)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithError(PositionDomainErrors.List.Empty())
            .NotEmpty().WithError(PositionDomainErrors.List.Empty())
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(PositionDomainErrors.List.Duplicates());
    }
}