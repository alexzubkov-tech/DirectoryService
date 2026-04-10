using Shared;

namespace DirectoryService.Infrastructure.Locations.Errors;

public class LocationInfrastructureErrors
{
    public static Error DatabaseError() =>
        Error.Conflict("location.database.error", "Ошибка базы данных при работе с локацией");

    public static Error OperationCancelled() =>
        Error.Failure("location.operation.cancelled", "Операция была отменена");
}