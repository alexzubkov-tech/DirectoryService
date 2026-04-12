using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.Create;

public class CreateLocationHandler: ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        IValidator<CreateLocationCommand> validator,
        ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandler> logger)
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
            return validationResult.ToListError();
        }

        // Создание Value Objects
        var name = LocationName.Create(command.Request.Name).Value;

        var address = LocationAddress.Create(
            command.Request.AddressDto.Country,
            command.Request.AddressDto.City,
            command.Request.AddressDto.Street,
            command.Request.AddressDto.BuildingNumber).Value;

        var timezone = LocationTimeZone.Create(command.Request.Timezone).Value;

        // бизнес валидация:

        // Нельзя создавать локацию с одним и тем же названием (проверяем даже неактивные)
        var existingByName = await _locationsRepository.GetBy(
            l => l.LocationName.Value == name.Value,
            includeInactive: true,
            cancellationToken);

        if (existingByName.IsSuccess)
        {
            return LocationApplicationErrors.AlreadyExistsByName(name.Value).ToErrors();
        }

        // Нельзя создавать локацию на адресе, если такой уже занят (проверяем даже неактивные)
        var existingByAddress = await _locationsRepository.GetBy(
            l => l.LocationAddress.Country == address.Country &&
                 l.LocationAddress.City == address.City &&
                 l.LocationAddress.Street == address.Street &&
                 l.LocationAddress.BuildingNumber == address.BuildingNumber,
            includeInactive: true,
            cancellationToken);

        if (existingByAddress.IsSuccess)
        {
            return LocationApplicationErrors.AlreadyExistsByAddress(address.ToString()).ToErrors();
        }

        // создание сущности Location
        var locationResult = Location.Create(name, address, timezone);

        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;

        // Сохранение в базу данных
        var addResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        // Логирование
        _logger.LogInformation("Location created with id {LocationId}", location.Id);

        return location.Id.Value;
    }
}