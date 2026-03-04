using Shared;

namespace DirectoryService.Domain.Departments.Errors;

public class DepartmentDomainErrors
{
    public static class Name
    {
        public static Error Empty() =>
            GeneralErrors.ValueIsRequired("department_name");

        public static Error TooShort(int min) =>
            Error.Validation(
                code: "department.name.too.short",
                message: $"Department name must be at least {min} characters.",
                invalidField: "department_name");

        public static Error TooLong(int max) =>
            Error.Validation(
                code: "department.name.too.long",
                message: $"Department name must be at most {max} characters.",
                invalidField: "department_name");
    }

    public static class Identifier
    {
        public static Error Empty() =>
            GeneralErrors.ValueIsRequired("identifier");

        public static Error InvalidFormat() =>
            Error.Validation(
                code: "department.identifier.invalid.format",
                message: "Identifier must contain only latin letters and hyphens (spaces are replaced with hyphens",
                invalidField: "identifier");

        public static Error InvalidLength(int min, int max) =>
            Error.Validation(
                code: "department.identifier.invalid.length",
                message: $"Identifier must be between {min} and {max} characters.",
                invalidField: "identifier");
    }

    public static class List
    {
        public static Error Empty() =>
            Error.Validation(
                "department.locations.empty",
                "Отдел должен содержать хотя бы одну локацию");

        public static Error Duplicates() =>
            Error.Validation(
                "department.locations.duplicates",
                "Список локаций не должен содержать дубликаты");
    }
}