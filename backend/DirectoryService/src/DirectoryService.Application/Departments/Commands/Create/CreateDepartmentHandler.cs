using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.ReferenceValidation;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Commands.Create;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly IReferenceValidator _referenceValidator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly HybridCache _cache;

    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IReferenceValidator referenceValidator,
        IDepartmentsRepository departmentsRepository,
        ILogger<CreateDepartmentHandler> logger,
        HybridCache cache)
    {
        _validator = validator;
        _referenceValidator = referenceValidator;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToListError();

        var name = DepartmentName.Create(command.Request.Name).Value;
        var identifier = DepartmentIdentifier.Create(command.Request.Identifier).Value;

        var existingDepartment = await _departmentsRepository.GetBy(
            d => d.DepartmentIdentifier.Value == identifier.Value,
            includeInactive: true,
            cancellationToken);

        if (existingDepartment.IsSuccess)
        {
            return DepartmentApplicationErrors
                .IdentifierAlreadyExists(identifier.Value)
                .ToErrors();
        }

        var validLocationIdsResult =
            await _referenceValidator.ExistAndActiveLocationsAsync(
                command.Request.LocationIds,
                cancellationToken);

        if (validLocationIdsResult.IsFailure)
            return validLocationIdsResult.Error;

        var validLocationIds = validLocationIdsResult.Value
            .Select(id => new LocationId(id))
            .ToList();

        Result<Department, Error> departmentResult;
        if (command.Request.ParentId is null)
        {
            departmentResult = Department.CreateParent(name, identifier, validLocationIds);
        }
        else
        {
            var parentResult = await _departmentsRepository.GetBy(
                d => d.Id == new DepartmentId(command.Request.ParentId.Value),
                includeInactive: true,
                cancellationToken);

            if (parentResult.IsFailure)
                return parentResult.Error.ToErrors();

            var parent = parentResult.Value;

            if (!parent.IsActive)
            {
                return DepartmentApplicationErrors
                    .ParentInactive(command.Request.ParentId.Value)
                    .ToErrors();
            }

            departmentResult = Department.CreateChild(
                name,
                identifier,
                parent,
                validLocationIds);
        }

        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        var department = departmentResult.Value;

        var addResult = await _departmentsRepository.AddAsync(department, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        await _cache.RemoveByTagAsync(CacheTags.DEPARTMENTS_LIST, cancellationToken);

        _logger.LogInformation(
            "Department {DepartmentId} created with path {Path}",
            department.Id.Value,
            department.DepartmentPath.Value);

        _logger.LogInformation(
            "Cache invalidated by tag {CacheTag} after department creation. DepartmentId: {DepartmentId}",
            CacheTags.DEPARTMENTS_LIST,
            department.Id.Value);

        return department.Id.Value;
    }
}