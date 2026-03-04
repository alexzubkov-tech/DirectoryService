using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Create;

public class CreateDepartmentHandler: ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Валидация входных данных
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToListError();

        // 2. Создание Value Objects
        var name = DepartmentName.Create(command.Request.Name).Value;
        var identifier = DepartmentIdentifier.Create(command.Request.Identifier).Value;

        // 3. Проверка уникальности идентификатора
        bool exists = await _departmentsRepository
            .ExistsByIdentifierAsync(identifier, cancellationToken);

        if (exists)
        {
            return DepartmentApplicationErrors
                .IdentifierAlreadyExists(identifier.Value)
                .ToErrors();
        }

        // 4. Проверка локаций: существуют и активны
        var validLocationIdsResult = await ExistAndActiveLocationAsync(command.Request.LocationIds, cancellationToken);
        if (validLocationIdsResult.IsFailure)
            return validLocationIdsResult.Error;

        var validLocationIds = validLocationIdsResult.Value
            .Select(id => new LocationId(id))
            .ToList();

        // 5. Создание Department (Parent или Child)
        Result<Department, Error> departmentResult;
        if (command.Request.ParentId is null)
        {
            departmentResult = Department.CreateParent(name, identifier, validLocationIds);
        }
        else
        {
            var parent = await _departmentsRepository.GetByIdAsync(command.Request.ParentId.Value, cancellationToken);
            if (parent is null)
                return DepartmentApplicationErrors.ParentNotFound(command.Request.ParentId.Value).ToErrors();

            if (!parent.IsActive)
                return DepartmentApplicationErrors.ParentInactive(command.Request.ParentId.Value).ToErrors();

            departmentResult = Department.CreateChild(name, identifier, parent, validLocationIds);
        }

        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        var department = departmentResult.Value;

        // 6. Сохранение в репозитории
        await _departmentsRepository.AddAsync(department, cancellationToken);

        // 7. Логирование
        _logger.LogInformation(
            "Department {DepartmentId} created with path {Path}",
            department.Id.Value,
            department.DepartmentPath.Value);

        return department.Id.Value;
    }

    private async Task<Result<IReadOnlyList<Guid>, Errors>> ExistAndActiveLocationAsync(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();
        var validIds = new List<Guid>();

        foreach (var id in locationIds)
        {
            var location = await _locationsRepository.GetByIdAsync(id, cancellationToken);
            if (location is null)
            {
                errors.Add(LocationApplicationErrors.NotFound(id));
                continue;
            }

            if (!location.IsActive)
            {
                errors.Add(LocationApplicationErrors.Inactive(id));
                continue;
            }

            validIds.Add(id);
        }

        if (errors.Any())
            return new Errors(errors);

        return validIds;
    }
}