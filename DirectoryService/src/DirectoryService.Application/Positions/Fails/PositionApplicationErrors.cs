using Shared;

namespace DirectoryService.Application.Positions.Fails;

public class PositionApplicationErrors
{
    public static Error NotFound(Guid positionId) =>
        Error.NotFound(
            code: "position.not.found",
            message: $"Position with id '{positionId}' not found.",
            id: positionId);

    public static Error Inactive(Guid positionId) =>
        Error.Validation(
            code: "position.inactive",
            message: $"Position with id '{positionId}' is inactive.",
            invalidField: "positionId");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict(
            code: "position.name.conflict",
            message: $"Position with name '{name}' already exists.");
}