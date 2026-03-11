using Shared;

namespace DirectoryService.Domain.Departments.Errors;

public class DepartmentDomainErrors
{

    public static class Id
    {
        public static Error Empty() =>
            Error.Validation(
                code: "department.id.empty",
                message: "Идентификатор отдела не может быть пустым (Guid.Empty).",
                invalidField: "departmentId");

        public static Error Null() =>
            Error.Validation(
                code: "department.id.null",
                message: "Идентификатор отдела не может быть null.",
                invalidField: "departmentId");

        public static Error ParentIdEmpty() => Error.Validation(
            code: "department.parent.id.empty",
            message: "Идентификатор родительского отдела не может быть пустым (Guid.Empty).",
            invalidField: "parentId");
    }

    public static class Name
    {
        public static Error Empty() =>
            GeneralErrors.ValueIsRequired("department_name");

        public static Error TooShort(int min) =>
            Error.Validation(
                code: "department.name.too.short",
                message: $"Название отдела должно содержать не менее {min} символов.",
                invalidField: "department_name");

        public static Error TooLong(int max) =>
            Error.Validation(
                code: "department.name.too.long",
                message: $"Название отдела должно содержать не более {max} символов.",
                invalidField: "department_name");
    }

    public static class Identifier
    {
        public static Error Empty() =>
            GeneralErrors.ValueIsRequired("identifier");

        public static Error InvalidFormat() =>
            Error.Validation(
                code: "department.identifier.invalid.format",
                message: "Идентификатор должен содержать только латинские буквы и дефисы (пробелы заменяются на дефисы).",
                invalidField: "identifier");

        public static Error InvalidLength(int min, int max) =>
            Error.Validation(
                code: "department.identifier.invalid.length",
                message: $"Идентификатор должен содержать от {min} до {max} символов.",
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