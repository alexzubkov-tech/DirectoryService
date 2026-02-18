using Shared;

namespace DirectoryService.Domain.Locations.Errors;

public class LocationTimeZoneErrors
{
    public static Error Invalid(string value) =>
        Error.Validation(
            "location.timezone.invalid",
            $"'{value}' не является корректной временной зоной");
}