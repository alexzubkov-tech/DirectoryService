using Shared;

namespace DirectoryService.Infrastructure.Positions.Errors;

public class PositionInfrastructureErrors
{
    public static Error DatabaseError() =>
        Error.Conflict("position.database.error", "Ошибка базы данных при работе с позициями");

    public static Error OperationCancelled() =>
        Error.Failure("position.operation.cancelled", "Операция была отменена");
}