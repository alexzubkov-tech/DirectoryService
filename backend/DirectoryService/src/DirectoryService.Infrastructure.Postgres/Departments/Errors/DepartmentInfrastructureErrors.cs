using Shared.SharedKernel;

namespace DirectoryService.Infrastructure.Departments.Errors;

public class DepartmentInfrastructureErrors
{
    public static Error DatabaseError() =>
        Error.Conflict("department.database.error", "Ошибка базы данных при работе с отделами");

    public static Error OperationCancelled() =>
        Error.Failure("department.operation.cancelled", "Операция была отменена");
}