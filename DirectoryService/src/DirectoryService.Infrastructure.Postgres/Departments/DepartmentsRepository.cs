using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.Errors;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.Errors;
using DirectoryService.Infrastructure.Departments.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentsRepository: IDepartmentsRepository
{
     private readonly DirectoryServiceDbContext _dbContext;
     private readonly ILogger<DepartmentsRepository> _logger;

     public DepartmentsRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

     public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _dbContext.Departments.AddAsync(department, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success<Guid, Error>(department.Id.Value);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                    pgEx.ConstraintName == "ix_department_identifier")
                {
                    return DepartmentApplicationErrors.IdentifierAlreadyExists(
                        department.DepartmentIdentifier.Value);
                }
            }

            _logger.LogError(ex, "Database update error while creating department {Name}",
                department.DepartmentName.Value);

            return DepartmentInfrastructureErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was canceled while creating department with name {departmentName}",
                department.DepartmentName.Value);
            return DepartmentInfrastructureErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department with name {departmentName}",
                department.DepartmentName.Value);
            return DepartmentInfrastructureErrors.DatabaseError();
        }
    }

     public async Task<bool> ExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var id = new DepartmentId(departmentId);
        return await _dbContext.Departments
            .AnyAsync(d => d.Id == id, cancellationToken);
    }

     public async Task<Department?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var departmentId = new DepartmentId(id);
        return await _dbContext.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
    }

     public async Task<bool> ExistsByIdentifierAsync(DepartmentIdentifier identifier, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments
            .AnyAsync(d => d.DepartmentIdentifier == identifier, cancellationToken);
    }
}