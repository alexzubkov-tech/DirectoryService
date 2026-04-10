using System.Text.Json.Serialization;

namespace Shared;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);

public record Error
{
    public IEnumerable<ErrorMessage> Messages { get; } = [];

    public ErrorType Type { get; }

    [JsonConstructor]
    private Error(IEnumerable<ErrorMessage> messages, ErrorType type)
    {
        Messages = messages.ToArray();
        Type = type;
    }

    public static Error NotFound(string? code, string message, Guid? id = null)
        => new([new ErrorMessage(code ?? "record.not.found", message)], ErrorType.NOT_FOUND);

    public static Error Validation(string? code, string message, string? invalidField = null)
        => new([new ErrorMessage(code ?? "value.is.invalid", message, invalidField)], ErrorType.VALIDATION);

    public static Error Conflict(string? code, string message)
        => new([new ErrorMessage(code ?? "value.is.invalid", message)], ErrorType.CONFLICT);

    public static Error Failure(string? code, string message)
        => new([new ErrorMessage(code ?? "failure", message)], ErrorType.FAILURE);

    public static Error Authorization(string? code, string message)
        => new([new ErrorMessage(code ?? "error.authorization", message)], ErrorType.AUTHORIZATION);

    public static Error Authentication(string? code, string message)
        => new([new ErrorMessage(code ?? "error.authentication", message)], ErrorType.AUTHENTICATION);

    public Errors ToErrors() => this;

}


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    /// <summary>
    /// Ошибка с валидацией.
    /// </summary>
    VALIDATION,

    /// <summary>
    /// Ошибка - ничего не найдено.
    /// </summary>
    NOT_FOUND,

    /// <summary>
    /// Ошибка сервера.
    /// </summary>
    FAILURE,

    /// <summary>
    /// Ошибка конфликт.
    /// </summary>
    CONFLICT,

    /// <summary>
    /// Ошибка аутентификации.
    /// </summary>
    AUTHENTICATION,

    /// <summary>
    /// Ошибка авторизации
    /// </summary>
    AUTHORIZATION,
}