using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Fails;
using DirectoryService.Application.ReferenceValidation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionHandler: ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly IReferenceValidator _referenceValidator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IReferenceValidator referenceValidator,
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        ILogger<CreatePositionHandler> logger)
    {
        _validator = validator;
        _referenceValidator = referenceValidator;
        _positionsRepository = positionsRepository;
        _logger = logger;
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

        // 3. Бизнес-валидация: уникальность имени среди всех позиций (включая неактивные)
        var existingPosition = await _positionsRepository.GetBy(
            p => p.PositionName.Value == name.Value,
            includeInactive: true,
            cancellationToken);

        if (existingPosition.IsSuccess && existingPosition.Value.IsActive)
            return PositionApplicationErrors.NameAlreadyExists(name.Value).ToErrors();

        // 4. Проверка departments: существуют и активны
        var validDepartmentIdsResult =
            await _referenceValidator.ExistAndActiveDepartmentsAsync(
                command.Request.DepartmentIds,
                cancellationToken);

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
}