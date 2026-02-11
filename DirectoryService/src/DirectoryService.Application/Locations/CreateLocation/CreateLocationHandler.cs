using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler: ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationsRepository locationsRepository,  ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация входных данных

        // бизнес валидация

        // создание сущности Location

        // Создание Value Objects
        var nameResult = LocationName.Create(command.CreateLocationDto.Name);
        if (nameResult.IsFailure)
            return Result.Failure<Guid, Error>(nameResult.Error);

        var addressResult = LocationAddress.Create(
            command.CreateLocationDto.AddressDto.Country,
            command.CreateLocationDto.AddressDto.City,
            command.CreateLocationDto.AddressDto.Street,
            command.CreateLocationDto.AddressDto.BuildingNumber);
        if (addressResult.IsFailure)
            return Result.Failure<Guid, Error>(addressResult.Error);

        var timezoneResult = LocationTimeZone.Create(command.CreateLocationDto.Timezone);
        if (timezoneResult.IsFailure)
            return Result.Failure<Guid, Error>(timezoneResult.Error);

        // Создание сущности Location
        var locationResult = Location.Create(nameResult.Value, addressResult.Value, timezoneResult.Value);
        if (locationResult.IsFailure)
            return Result.Failure<Guid, Error>(locationResult.Error);

        var location = locationResult.Value;

        // Сохранение в базу данных
        await _locationsRepository.AddAsync(location, cancellationToken);

        // Логирование
        _logger.LogInformation("Location created with id {LocationId}", location.Id);

        return Result.Success<Guid, Error>(location.Id);
    }
}