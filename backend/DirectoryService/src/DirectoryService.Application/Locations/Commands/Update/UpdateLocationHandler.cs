using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.Update;

public class UpdateLocationHandler : ICommandHandler<Guid, UpdateLocationCommand>
{
    private readonly IValidator<UpdateLocationCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateLocationHandler> _logger;
    private readonly HybridCache _cache;

    public UpdateLocationHandler(
        IValidator<UpdateLocationCommand> validator,
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<UpdateLocationHandler> logger,
        HybridCache cache)
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToListError();
        }

        var locationId = new LocationId(command.LocationId);

        var transactionScopedResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopedResult.IsFailure)
        {
            return transactionScopedResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopedResult.Value;

        var locationResult = await _locationsRepository.GetByIdWithLock(
            locationId,
            cancellationToken);

        if (locationResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationResult.Error.ToErrors();
        }

        var location = locationResult.Value;

        var nameResult = LocationName.Create(command.Request.Name);
        if (nameResult.IsFailure)
        {
            transactionScope.Rollback();
            return nameResult.Error.ToErrors();
        }

        var addressResult = LocationAddress.Create(
            command.Request.AddressDto.Country,
            command.Request.AddressDto.City,
            command.Request.AddressDto.Street,
            command.Request.AddressDto.BuildingNumber);
        if (addressResult.IsFailure)
        {
            transactionScope.Rollback();
            return addressResult.Error.ToErrors();
        }

        var timezoneResult = LocationTimeZone.Create(command.Request.Timezone);
        if (timezoneResult.IsFailure)
        {
            transactionScope.Rollback();
            return timezoneResult.Error.ToErrors();
        }

        var name = nameResult.Value;
        var address = addressResult.Value;
        var timezone = timezoneResult.Value;

        if (location.LocationName.Value != name.Value)
        {
            var existingByName = await _locationsRepository.GetBy(
                l => l.LocationName.Value == name.Value,
                includeInactive: true,
                cancellationToken);

            if (existingByName.IsSuccess && existingByName.Value.Id != location.Id)
            {
                transactionScope.Rollback();
                return LocationApplicationErrors.AlreadyExistsByName(name.Value).ToErrors();
            }
        }

        if (location.LocationAddress.Country != address.Country ||
            location.LocationAddress.City != address.City ||
            location.LocationAddress.Street != address.Street ||
            location.LocationAddress.BuildingNumber != address.BuildingNumber)
        {
            var existingByAddress = await _locationsRepository.GetBy(
                l => l.LocationAddress.Country == address.Country &&
                     l.LocationAddress.City == address.City &&
                     l.LocationAddress.Street == address.Street &&
                     l.LocationAddress.BuildingNumber == address.BuildingNumber,
                includeInactive: true,
                cancellationToken);

            if (existingByAddress.IsSuccess && existingByAddress.Value.Id != location.Id)
            {
                transactionScope.Rollback();
                return LocationApplicationErrors.AlreadyExistsByAddress(address.ToString()).ToErrors();
            }
        }

        location.UpdateName(name);
        location.UpdateAddress(address);
        location.UpdateTimezone(timezone);

        if (command.Request.IsActive && !location.IsActive)
        {
            location.Activate();
        }
        else if (!command.Request.IsActive && location.IsActive)
        {
            location.Deactivate();
        }

        var updateResult = await _locationsRepository.UpdateAsync(location, cancellationToken);
        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateResult.Error.ToErrors();
        }

        var committedResult = transactionScope.Commit();
        if (committedResult.IsFailure)
        {
            return committedResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheTags.LOCATIONS_LIST, cancellationToken);

        _logger.LogInformation("Location updated with id {LocationId}", location.Id.Value);

        return location.Id.Value;
    }
}
