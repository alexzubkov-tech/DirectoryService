using Shared;

namespace DirectoryService.Application.Locations.Fails;

public static class LocationApplicationErrors
{
    public static Error NotFound(Guid locationId) =>
        Error.NotFound(
            code: "location.not.found",
            message: $"Location with id '{locationId}' not found.",
            id: locationId);

    public static Error Inactive(Guid locationId) =>
        Error.Validation(
            code: "location.inactive",
            message: $"Location with id '{locationId}' is inactive.",
            invalidField: "locationId");

    public static Error AlreadyExistsByName(string name) =>
        Error.Conflict(
            code: "location.already.exists.by.name",
            message: $"Location with name '{name}' already exists.");

    public static Error AlreadyExistsByAddress(string address) =>
        Error.Conflict(
            code: "location.already.exists.by.address",
            message: $"Location with address '{address}' already exists.");
}