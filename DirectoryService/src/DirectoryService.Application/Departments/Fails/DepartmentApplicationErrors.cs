using Shared;

namespace DirectoryService.Application.Departments.Fails;

public class DepartmentApplicationErrors
{
    public static Error NotFound(Guid departmentId) =>
        Error.NotFound(
            code: "department.not.found",
            message: $"Department with id '{departmentId}' not found.",
            id: departmentId);

    public static Error Inactive(Guid departmentId) =>
        Error.Validation(
            code: "department.inactive",
            message: $"Department with id '{departmentId}' is inactive.",
            invalidField: "departmentId");

    public static Error ParentNotFound(Guid parentId) =>
        Error.NotFound(
            code: "department.parent.not.found",
            message: $"Parent department with id '{parentId}' not found.",
            id: parentId);

    // не обязательно, но для удобства
    public static Error ParentInactive(Guid parentId) =>
        Error.Validation(
            code: "department.parent.inactive",
            message: $"Parent department with id '{parentId}' is inactive.",
            invalidField: "parentId");

    public static Error IdentifierAlreadyExists(string identifier) =>
        Error.Conflict(
            "department.identifier.conflict",
            $"Department with identifier '{identifier}' already exists");
}