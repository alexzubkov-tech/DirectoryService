using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.Restore;

public class RestoreLocationHandler : ICommandHandler<RestoreLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RestoreLocationHandler> _logger;
    private readonly HybridCache _cache;

    public RestoreLocationHandler(
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<RestoreLocationHandler> logger,
        HybridCache cache)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UnitResult<Errors>> Handle(RestoreLocationCommand command, CancellationToken cancellationToken)
    {
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

        if (location.IsActive)
        {
            transactionScope.Rollback();
            return LocationApplicationErrors.Active(location.Id.Value).ToErrors();
        }

        location.Activate();

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

        _logger.LogInformation("Location restored with id {LocationId}", location.Id.Value);

        return UnitResult.Success<Errors>();
    }
}