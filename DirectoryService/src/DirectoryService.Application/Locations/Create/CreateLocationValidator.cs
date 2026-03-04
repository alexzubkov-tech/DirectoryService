using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationValidator: AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
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

        // не пустой список и без дубликатов!!!
    }
}