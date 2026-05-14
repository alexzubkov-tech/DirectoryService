using Core.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Locations.Commands.Update;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(l => l.Request.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(l => l.Request.AddressDto)
            .MustBeValueObject(dto =>
                LocationAddress.Create(
                    dto.Country,
                    dto.City,
                    dto.Street,
                    dto.BuildingNumber));

        RuleFor(l => l.Request.Timezone)
            .MustBeValueObject(LocationTimeZone.Create);
    }
}