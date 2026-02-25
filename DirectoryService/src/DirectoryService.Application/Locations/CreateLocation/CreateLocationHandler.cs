using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Extensions;
using DirectoryService.Application.Locations.Fails.Exceptions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using ValidationException = DirectoryService.Application.Exceptions.ValidationException;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler: ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(IValidator<CreateLocationCommand> validator, ILocationsRepository locationsRepository,  ILogger<CreateLocationHandler> logger)
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация входных данных
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        // бизнес валидация

        // создание сущности Location

        // Создание Value Objects
        var nameResult = LocationName.Create(command.CreateLocationDto.Name);
        if (nameResult.IsFailure)
            return nameResult.Error.ToErrors();

        var addressResult = LocationAddress.Create(
            command.CreateLocationDto.AddressDto.Country,
            command.CreateLocationDto.AddressDto.City,
            command.CreateLocationDto.AddressDto.Street,
            command.CreateLocationDto.AddressDto.BuildingNumber);
        if (addressResult.IsFailure)
            return addressResult.Error.ToErrors();

        var timezoneResult = LocationTimeZone.Create(command.CreateLocationDto.Timezone);
        if (timezoneResult.IsFailure)
            return timezoneResult.Error.ToErrors();

        // Создание сущности Location
        var locationResult = Location.Create(nameResult.Value, addressResult.Value, timezoneResult.Value);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;

        // Сохранение в базу данных
        await _locationsRepository.AddAsync(location, cancellationToken);

        // Логирование
        _logger.LogInformation("Location created with id {LocationId}", location.Id);

        return location.Id;
    }
}