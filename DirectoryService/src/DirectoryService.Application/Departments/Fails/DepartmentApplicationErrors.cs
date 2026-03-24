using Shared;

namespace DirectoryService.Application.Departments.Fails;

public class DepartmentApplicationErrors
{
    public static Error NotFound(Guid departmentId) =>
        Error.NotFound(
            code: "department.not.found",
            message: $"Отдел с идентификатором '{departmentId}' не найден.",
            id: departmentId);

    public static Error Inactive(Guid departmentId) =>
        Error.Validation(
            code: "department.inactive",
            message: $"Отдел с идентификатором '{departmentId}' неактивен.",
            invalidField: "departmentId");

    public static Error CyclicHierarchy() =>
        Error.Conflict(
            "department.cyclic.hierarchy",
            "Нельзя сделать дочерний отдел родительским");

    public static Error SelfParent(Guid departmentId) =>
        Error.Validation(
            code: "department.parent.self",
            message: $"Отдел с идентификатором '{departmentId}' не может быть собственным родителем.",
            invalidField: "parentId");

    public static Error ParentInactive(Guid parentId) =>
        Error.Validation(
            code: "department.parent.inactive",
            message: $"Родительский отдел с идентификатором '{parentId}' неактивен.",
            invalidField: "parentId");

    public static Error ParentNotFound(Guid parentId) =>
        Error.NotFound(
            code: "department.parent.not.found",
            message: $"Родительский отдел с идентификатором '{parentId}' не найден.",
            id: parentId);

    public static Error IdentifierAlreadyExists(string identifier) =>
        Error.Conflict(
            "department.identifier.conflict",
            $"Отдел с идентификатором '{identifier}' уже существует");

    public static class Pagination
    {
        public static Error PageMustBePositive() =>
            Error.Validation("pagination.page.positive", "Номер страницы должен быть больше 0.");

        public static Error PageSizeMustBePositive() =>
            Error.Validation("pagination.pageSize.positive", "Размер страницы должен быть больше 0.");

        public static Error PageSizeTooLarge(int max) =>
            Error.Validation("pagination.pageSize.max", $"Размер страницы не может превышать {max}.");
    }
}