using Shared;

namespace DirectoryService.Domain.Positions.Errors;

public class PositionDomainErrors
{
    public static class Name
    {
        public static Error Empty() =>
            GeneralErrors.ValueIsRequired("PositionName");

        public static Error TooShort(int minLength) =>
            Error.Validation(
                "position.name.too.short",
                $"Название позиции должно содержать минимум {minLength} символа");

        public static Error TooLong(int maxLength) =>
            Error.Validation(
                "position.name.too.long",
                $"Название позиции должно содержать не более {maxLength} символов");
    }

    public static class Description
    {
        public static Error TooLong(int maxLength) =>
            Error.Validation(
                "position.description.too.long",
                $"Описание должно быть не больше {maxLength} символов");
    }

    public static class List
    {
        public static Error Empty() =>
             Error.Validation(
            "position.departments.empty",
            "Должность должна быть закреплена хотя бы за одним отделом");

        public static Error Duplicates() =>
             Error.Validation(
            "position.departments.duplicates",
            "Список отделов не должен содержать дубликаты");
    }
}