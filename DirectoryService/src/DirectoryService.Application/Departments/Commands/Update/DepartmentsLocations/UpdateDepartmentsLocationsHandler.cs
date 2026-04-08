using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.ReferenceValidation;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;

public class UpdateDepartmentsLocationsHandler : ICommandHandler<UpdateDepartmentsLocationsCommand>
{
    private readonly IValidator<UpdateDepartmentsLocationsCommand> _validator;
    private readonly IReferenceValidator _referenceValidator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<UpdateDepartmentsLocationsHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _cache;

    public UpdateDepartmentsLocationsHandler(
        IValidator<UpdateDepartmentsLocationsCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ILogger<UpdateDepartmentsLocationsHandler> logger,
        ITransactionManager transactionManager,
        IReferenceValidator referenceValidator,
        HybridCache cache)
    {
        _validator = validator;
        _referenceValidator = referenceValidator;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _transactionManager = transactionManager;
        _cache = cache;
    }

    public async Task<UnitResult<Errors>> Handle(
        UpdateDepartmentsLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToListError();

        var transactionScopedResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopedResult.IsFailure)
        {
            return transactionScopedResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopedResult.Value;

        var departmentResult = await _departmentsRepository.GetBy(
            d => d.Id == new DepartmentId(command.Request.DepartmentId),
            includeInactive: true,
            cancellationToken);

        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        if (!department.IsActive)
        {
            transactionScope.Rollback();
            return DepartmentApplicationErrors.Inactive(department.Id.Value).ToErrors();
        }

        var validLocationIdsResult =
            await _referenceValidator.ExistAndActiveLocationsAsync(
                command.Request.LocationIds,
                cancellationToken);

        if (validLocationIdsResult.IsFailure)
        {
            transactionScope.Rollback();
            return validLocationIdsResult.Error;
        }

        var validLocationIds = validLocationIdsResult.Value
            .Select(id => new LocationId(id))
            .ToList();

        await _departmentsRepository.DeleteLocationsByDepartmentIdAsync(
            new DepartmentId(command.Request.DepartmentId),
            cancellationToken);

        department.UpdateLocations(validLocationIds);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitedResult = transactionScope.Commit();

        if (commitedResult.IsFailure)
        {
            return commitedResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(["departments:list"], cancellationToken);

        _logger.LogInformation("Локации отдела успешно обновлены");

        return UnitResult.Success<Errors>();
    }
}