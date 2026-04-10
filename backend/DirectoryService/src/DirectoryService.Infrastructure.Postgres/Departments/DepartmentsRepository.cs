using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Fails;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
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

     public async Task<Result<Department, Error>> GetBy(
        Expression<Func<Department, bool>> predicate,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Departments.AsQueryable();

        if (includeInactive)
        {
            query = query.IgnoreQueryFilters();
        }

        var department = await query.FirstOrDefaultAsync(predicate, cancellationToken);

        if (department is null)
        {
            return DepartmentApplicationErrors.NotFound();
        }

        return department;
    }

     public async Task<List<Department>> GetListByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var departmentIds = ids
            .Select(id => new DepartmentId(id))
            .ToList();

        return await _dbContext.Departments
            .IgnoreQueryFilters()
            .Where(d => departmentIds.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }

     public async Task<UnitResult<Error>> DeleteLocationsByDepartmentIdAsync(
         DepartmentId departmentId,
         CancellationToken cancellationToken)
     {
         await _dbContext.DepartmentLocations
             .Where(dl => dl.DepartmentId == departmentId)
             .ExecuteDeleteAsync(cancellationToken);

         return UnitResult.Success<Error>();
     }

     public async Task<Result<Department, Error>> GetByIdWithLock(
         DepartmentId departmentId,
         CancellationToken cancellationToken)
     {
         var department = await _dbContext.Departments
             .FromSql($"SELECT * FROM departments WHERE department_id = {departmentId.Value} FOR UPDATE")
             .IgnoreQueryFilters()
             .FirstOrDefaultAsync(cancellationToken);

         if (department is null)
         {
             return DepartmentApplicationErrors.NotFound(departmentId.Value);
         }

         return department;
     }

     public async Task<bool> IsDescendantAsync(
         string candidateParentPath,
         string departmentPath,
         CancellationToken cancellationToken)
     {
         var sql = """
                   SELECT (@candidate::ltree <@ @department::ltree) AS "Value"
                   """;

         return await _dbContext.Database
             .SqlQueryRaw<bool>(
                 sql,
                 new NpgsqlParameter("candidate", candidateParentPath),
                 new NpgsqlParameter("department", departmentPath))
             .FirstOrDefaultAsync(cancellationToken);
     }

     public async Task<UnitResult<Error>> LockDescendantsAsync(
         string rootPath,
         CancellationToken cancellationToken)
     {
         var sql = """
                   SELECT department_id
                   FROM departments
                   WHERE department_path <@ @rootPath::ltree
                   AND department_path != @rootPath::ltree
                   FOR UPDATE
                   """;

         try
         {
             await _dbContext.Database.ExecuteSqlRawAsync(
                 sql,
                 [new NpgsqlParameter("rootPath", rootPath)],
                 cancellationToken);

             return UnitResult.Success<Error>();
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Failed to lock descendants for path {RootPath}", rootPath);
             return DepartmentInfrastructureErrors.DatabaseError();
         }
     }

     public async Task<UnitResult<Error>> UpdateDescendantsPathAsync(
         string oldPath,
         string newPath,
         CancellationToken cancellationToken)
     {
         var sql = $"""
                    UPDATE departments
                    SET department_path =
                            @newPath::ltree
                            || subpath(department_path, nlevel(@oldPath::ltree)),
                        depth =
                            nlevel(
                                @newPath::ltree
                                || subpath(department_path, nlevel(@oldPath::ltree))
                            ) - 1,
                        updated_at = now()
                    WHERE department_path <@ @oldPath::ltree
                    AND department_path != @oldPath::ltree
                    """;

         try
         {
             await _dbContext.Database.ExecuteSqlRawAsync(
                 sql,
                 [
                     new NpgsqlParameter("oldPath", oldPath),
                     new NpgsqlParameter("newPath", newPath)
                 ],
                 cancellationToken);

             return UnitResult.Success<Error>();
         }
         catch (Exception ex)
         {
             _logger.LogError(ex,
                 "Failed to update descendants path from {OldPath} to {NewPath}",
                 oldPath,
                 newPath);

             return DepartmentInfrastructureErrors.DatabaseError();
         }
     }

 public async Task<UnitResult<Error>> DeactivateUnusedLocationsAndPositionsAsync(
    DepartmentId departmentId,
    CancellationToken cancellationToken)
{
    try
    {
        var sql = """
                  -- Блокируем локации, которые планируем деактивировать
                  WITH locked_locations AS (
                      SELECT dl.location_id
                      FROM department_locations dl
                      JOIN locations l ON l.location_id = dl.location_id
                      WHERE dl.department_id = @departmentId
                        AND l.is_active = true
                        AND NOT EXISTS (
                            SELECT 1
                            FROM department_locations dl2
                            JOIN departments d ON d.department_id = dl2.department_id
                            WHERE dl2.location_id = dl.location_id
                              AND d.is_active = true
                              AND d.department_id != @departmentId
                        )
                      FOR UPDATE
                  ),
                  -- Блокируем позиции, которые планируем деактивировать
                  locked_positions AS (
                      SELECT dp.position_id
                      FROM department_positions dp
                      JOIN positions p ON p.position_id = dp.position_id
                      WHERE dp.department_id = @departmentId
                        AND p.is_active = true
                        AND NOT EXISTS (
                            SELECT 1
                            FROM department_positions dp2
                            JOIN departments d ON d.department_id = dp2.department_id
                            WHERE dp2.position_id = dp.position_id
                              AND d.is_active = true
                              AND d.department_id != @departmentId
                        )
                      FOR UPDATE
                  ),
                  -- Деактивируем локации
                  deactivated_locations AS (
                      UPDATE locations
                      SET is_active = false,
                          deleted_at = now(),
                          updated_at = now()
                      WHERE location_id IN (SELECT location_id FROM locked_locations)
                  ),
                  -- Деактивируем позиции
                  deactivated_positions AS (
                      UPDATE positions
                      SET is_active = false,
                          deleted_at = now(),
                          updated_at = now()
                      WHERE position_id IN (SELECT position_id FROM locked_positions)
                  )
                  SELECT 1
                  """;

        await _dbContext.Database.ExecuteSqlRawAsync(
            sql,
            new[] { new NpgsqlParameter("departmentId", departmentId.Value) },
            cancellationToken);

        return UnitResult.Success<Error>();
    }
    catch (Exception ex)
    {
        _logger.LogError(
            ex,
            "Failed to deactivate unused locations and positions for department {DepartmentId}",
            departmentId.Value);
        return DepartmentInfrastructureErrors.DatabaseError();
    }
}
}

