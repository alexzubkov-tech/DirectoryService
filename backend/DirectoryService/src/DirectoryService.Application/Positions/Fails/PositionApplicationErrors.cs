using Shared;

namespace DirectoryService.Application.Positions.Fails;

public class PositionApplicationErrors
{
    public static Error NotFound(Guid? positionId = null) =>
        Error.NotFound(
            code: "position.not.found",
            message: positionId.HasValue
                ? $"Позиция с идентификатором '{positionId}' не найдена."
                : "Позиция не найдена",
            id: positionId);

    public static Error Inactive(Guid positionId) =>
        Error.Validation(
            code: "position.inactive",
            message: $"Позиция с идентификатором '{positionId}' неактивна.",
            invalidField: "positionId");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict(
            code: "position.name.conflict",
            message: $"Позиция с названием '{name}' уже существует.");
}