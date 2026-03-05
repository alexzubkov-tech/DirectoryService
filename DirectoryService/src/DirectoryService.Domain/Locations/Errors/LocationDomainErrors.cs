using Shared;

namespace DirectoryService.Domain.Locations.Errors;

public static class LocationDomainErrors
{
    public static class Name
    {
        public static Error Empty() =>
            GeneralErrors.ValueIsRequired("name");

        public static Error TooShort(int min) =>
            Error.Validation(
                "location.name.too.short",
                $"Name must be at least {min} characters");

        public static Error TooLong(int max) =>
            Error.Validation(
                "location.name.too.long",
                $"Name must be at most {max} characters");
    }

    public static class TimeZone
    {
        public static Error Invalid(string value) =>
            Error.Validation(
                "location.timezone.invalid",
                $"'{value}' is not a valid timezone");
    }
}