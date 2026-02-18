using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator: AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        // пока только название
        RuleFor(x => x.CreateLocationDto.Name)
            .NotEmpty().WithMessage("Name is required").WithErrorCode("Required")
            .MaximumLength(120).WithMessage("Name must not exceed 120").WithErrorCode("MaximumLength");
    }
}