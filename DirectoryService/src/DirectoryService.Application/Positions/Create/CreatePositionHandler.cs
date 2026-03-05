using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.Positions.Fails;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionHandler: ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;

    public CreatePositionHandler(
        ILogger<CreatePositionHandler> logger,
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository)
    {
        _logger = logger;
        _validator = validator;
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        // 1. Валидация входных данных
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToListError();

        // 2. Создание Value Objects
        var name = PositionName.Create(command.Request.Name).Value;
        var description = PositionDescription.Create(command.Request.Description).Value;

        // 3. Бизнес-валидация: уникальность имени среди всех активных позиций
        var existingPosition = await _positionsRepository.GetByNameAsync(name, cancellationToken);
        if (existingPosition != null && existingPosition.IsActive)
            return PositionApplicationErrors.NameAlreadyExists(name.Value).ToErrors();

        // 4. Проверка departments: существуют и активны
        var validDepartmentIdsResult = await ExistAndActiveDepartmentsAsync(command.Request.DepartmentIds, cancellationToken);
        if (validDepartmentIdsResult.IsFailure)
            return validDepartmentIdsResult.Error;

        var validDepartmentIds = validDepartmentIdsResult.Value
            .Select(id => new DepartmentId(id))
            .ToList();

        // 5. Создание сущности Position (сразу с отделами)
        var positionResult = Position.Create(name, description, validDepartmentIds);
        if (positionResult.IsFailure)
            return positionResult.Error.ToErrors();

        var position = positionResult.Value;

        // 6. Сохранение в репозитории
        var addResult = await _positionsRepository.AddAsync(position, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        // 7. Логирование
        _logger.LogInformation("Position {PositionId} created with name {Name}", position.Id.Value, name.Value);

        return position.Id.Value;
    }

    private async Task<Result<IReadOnlyList<Guid>, Errors>> ExistAndActiveDepartmentsAsync(
        IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken)
    {
        var existingDepartments = await _departmentsRepository.GetListByIdsAsync(departmentIds, cancellationToken);

        var departmentsDict = existingDepartments.ToDictionary(d => d.Id.Value);

        var errors = new List<Error>();
        var validIds = new List<Guid>();

        foreach (var id in departmentIds)
        {
            if (!departmentsDict.TryGetValue(id, out var department))
            {
                errors.Add(DepartmentApplicationErrors.NotFound(id));
                continue;
            }

            if (!department.IsActive)
            {
                errors.Add(DepartmentApplicationErrors.Inactive(id));
                continue;
            }

            validIds.Add(id);
        }

        if (errors.Any())
            return new Errors(errors);

        return validIds;
    }
}