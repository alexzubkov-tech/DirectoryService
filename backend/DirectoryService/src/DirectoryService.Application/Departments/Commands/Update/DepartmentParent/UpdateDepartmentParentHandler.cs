using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Commands.Update.DepartmentParent;

public class UpdateDepartmentParentHandler : ICommandHandler<UpdateDepartmentParentCommand>
{
    private readonly IValidator<UpdateDepartmentParentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentParentHandler> _logger;
    private readonly HybridCache _cache;

    public UpdateDepartmentParentHandler(
        IValidator<UpdateDepartmentParentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<UpdateDepartmentParentHandler> logger,
        HybridCache cache)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UnitResult<Errors>> Handle(
        UpdateDepartmentParentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToListError();
        }

        var transactionScopedResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopedResult.IsFailure)
        {
            return transactionScopedResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopedResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdWithLock(
            new DepartmentId(command.Request.DepartmentId),
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

        Department? parent = null;

        if (command.Request.ParentId.HasValue)
        {
            if (command.Request.ParentId.Value == department.Id.Value)
            {
                transactionScope.Rollback();
                return DepartmentApplicationErrors.SelfParent(department.Id.Value).ToErrors();
            }

            var parentResult = await _departmentsRepository.GetByIdWithLock(
                new DepartmentId(command.Request.ParentId.Value),
                cancellationToken);

            if (parentResult.IsFailure)
            {
                transactionScope.Rollback();
                return parentResult.Error.ToErrors();
            }

            parent = parentResult.Value;

            if (!parent.IsActive)
            {
                transactionScope.Rollback();
                return DepartmentApplicationErrors.ParentInactive(parent.Id.Value).ToErrors();
            }

            var isDescendant = await _departmentsRepository.IsDescendantAsync(
                parent.DepartmentPath.Value,
                department.DepartmentPath.Value,
                cancellationToken);

            if (isDescendant)
            {
                transactionScope.Rollback();
                return DepartmentApplicationErrors.CyclicHierarchy().ToErrors();
            }
        }

        var oldPath = department.DepartmentPath.Value;

        department.SetParent(parent?.Id, parent?.DepartmentPath);

        var lockDescendantsResult = await _departmentsRepository.LockDescendantsAsync(oldPath, cancellationToken);
        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            return lockDescendantsResult.Error.ToErrors();
        }

        var updateDescendantsResult = await _departmentsRepository.UpdateDescendantsPathAsync(
            oldPath,
            department.DepartmentPath.Value,
            cancellationToken);

        if (updateDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateDescendantsResult.Error.ToErrors();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error.ToErrors();
        }

        var committedResult = transactionScope.Commit();
        if (committedResult.IsFailure)
        {
            return committedResult.Error.ToErrors();
        }

        await _cache.RemoveByTagAsync(CacheTags.DEPARTMENTS_LIST, cancellationToken);

        _logger.LogInformation(
            "Отдел {Department} успешно перемещен в {Parent}",
            department.DepartmentName,
            parent?.DepartmentName);

        return UnitResult.Success<Errors>();
    }
}