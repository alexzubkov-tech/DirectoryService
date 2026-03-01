using Shared;

namespace DirectoryService.Domain.Locations.Errors;

public static class LocationErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict("location.name.conflict", $"Локация с именем {name} уже существует");

    public static Error AddressConflict(string address) =>
        Error.Conflict("location.address.conflict", $"Локация с адресом: {address} уже существует.");

    public static Error DatabaseError() =>
        Error.Conflict("location.database.error", "Ошибка базы данных при работе с локацией");

    public static Error OperationCancelled() =>
        Error.Failure("location.operation.cancelled", "Операция была отменена");
}