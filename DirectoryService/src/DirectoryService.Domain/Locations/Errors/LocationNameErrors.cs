using Shared;

namespace DirectoryService.Domain.Locations.Errors;

public static class LocationNameErrors
{
    public static Error Empty() =>
        GeneralErrors.ValueIsRequired("LocationName");

    public static Error TooShort(int minLength) =>
        Error.Validation(
            "location.name.too.short",
            $"Название локации должно содержать минимум {minLength} символа");

    public static Error TooLong(int maxLength) =>
        Error.Validation(
            "location.name.too.long",
            $"Название локации должно содержать не более {maxLength} символов");
}