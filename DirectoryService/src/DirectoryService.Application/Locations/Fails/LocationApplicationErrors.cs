using Shared;

namespace DirectoryService.Application.Locations.Fails;

public static class LocationApplicationErrors
{
    public static Error NotFound(Guid locationId) =>
        Error.NotFound(
            code: "location.not.found",
            message: $"Локация с идентификатором '{locationId}' не найдена.",
            id: locationId);

    public static Error Inactive(Guid locationId) =>
        Error.Validation(
            code: "location.inactive",
            message: $"Локация с идентификатором '{locationId}' неактивна.",
            invalidField: "locationId");

    public static Error AlreadyExistsByName(string name) =>
        Error.Conflict(
            code: "location.already.exists.by.name",
            message: $"Локация с названием '{name}' уже существует.");

    public static Error AlreadyExistsByAddress(string address) =>
        Error.Conflict(
            code: "location.already.exists.by.address",
            message: $"Локация с адресом '{address}' уже существует.");
}