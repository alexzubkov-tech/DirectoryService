using Shared.SharedKernel;

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
                $"Название должно содержать не менее {min} символов");

        public static Error TooLong(int max) =>
            Error.Validation(
                "location.name.too.long",
                $"Название должно содержать не более {max} символов");
    }

    public static class TimeZone
    {
        public static Error Invalid(string value) =>
            Error.Validation(
                "location.timezone.invalid",
                $"'{value}' не является допустимым часовым поясом");
    }
}