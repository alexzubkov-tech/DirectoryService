using Shared;

namespace DirectoryService.Application.Locations.Fails;

public static class LocationApplicationErrors
{
    public static Error NotFound(Guid locationId) =>
        Error.NotFound(
            code: "location.not.found",
            message: $"Локация с идентификатором '{locationId}' не найдена.",
            id: locationId);

    public static Error Inactive(Guid locationId) =>
        Error.Validation(
            code: "location.inactive",
            message: $"Локация с идентификатором '{locationId}' неактивна.",
            invalidField: "locationId");

    public static Error AlreadyExistsByName(string name) =>
        Error.Conflict(
            code: "location.already.exists.by.name",
            message: $"Локация с названием '{name}' уже существует.");

    public static Error AlreadyExistsByAddress(string address) =>
        Error.Conflict(
            code: "location.already.exists.by.address",
            message: $"Локация с адресом '{address}' уже существует.");

    public static class Pagination
    {
        public static Error PageMustBePositive() =>
            Error.Validation("pagination.page.positive", "Номер страницы должен быть больше 0.");

        public static Error PageSizeMustBePositive() =>
            Error.Validation("pagination.pageSize.positive", "Размер страницы должен быть больше 0.");

        public static Error PageSizeTooLarge(int max) =>
            Error.Validation("pagination.pageSize.max", $"Размер страницы не может превышать {max}.");
    }

    public static class DepartmentIds
    {
        public static Error Duplicate() =>
            Error.Validation("departmentIds.duplicate", "Список идентификаторов отделов не должен содержать дубликатов.");
    }
}