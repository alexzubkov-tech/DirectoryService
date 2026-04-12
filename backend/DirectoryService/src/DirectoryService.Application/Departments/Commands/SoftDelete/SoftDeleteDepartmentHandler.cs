using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Common.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.SoftDelete;

public class SoftDeleteDepartmentHandler : ICommandHandler<SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly HybridCache _cache;

    public SoftDeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<SoftDeleteDepartmentHandler> logger,
        HybridCache cache)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UnitResult<Errors>> Handle(
        SoftDeleteDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var transactionScopedResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopedResult.IsFailure)
        {
            return transactionScopedResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopedResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdWithLock(
            new DepartmentId(command.DepartmentId),
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

        string oldPath = department.DepartmentPath.Value;

        department.Deactivate();
        string newPath = department.DepartmentPath.Value;

        var lockDescendantsResult = await _departmentsRepository.LockDescendantsAsync(
            oldPath,
            cancellationToken);

        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            return lockDescendantsResult.Error.ToErrors();
        }

        await _departmentsRepository.UpdateDescendantsPathAsync(oldPath, newPath, cancellationToken);

        var deactivateUnusedResult = await _departmentsRepository.DeactivateUnusedLocationsAndPositionsAsync(
            department.Id,
            cancellationToken);

        if (deactivateUnusedResult.IsFailure)
        {
            transactionScope.Rollback();
            return deactivateUnusedResult.Error.ToErrors();
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
            "Отдел {DepartmentName} (Id: {DepartmentId}) успешно soft удален",
            department.DepartmentName.Value,
            department.Id.Value);

        return UnitResult.Success<Errors>();
    }
}