using System.Text.Json.Serialization;

namespace Shared;

public record Error
{
    private const string SEPARATOR = "||";

    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public string? InvalidField { get; }

    [JsonConstructor]
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error NotFound(string? code, string message, Guid? id)
        => new(code ?? "record.not.found", message, ErrorType.NOT_FOUND);

    public static Error Validation(string? code, string message, string? invalidField = null)
        => new(code ?? "value.is.invalid", message, ErrorType.VALIDATION, invalidField);

    public static Error Conflict(string? code, string message)
        => new(code ?? "value.is.invalid", message, ErrorType.CONFLICT);

    public static Error Failure(string? code, string message)
        => new(code ?? "failure", message, ErrorType.FAILURE);

    public static Error Authorization(string? code, string message)
        => new (code ?? "error.authorization", message, ErrorType.AUTHORIZATION);

    public static Error Authentication(string? code, string message)
        => new (code ?? "error.authentication", message, ErrorType.AUTHENTICATION);

    public Errors ToErrors() => this;

    public string Serialize()
    {
        return string.Join(SEPARATOR, Code, Message, Type);
    }

    public static Error Deserialize(string serialized)
    {
        string[] parts = serialized.Split(SEPARATOR);

        if (parts.Length < 3)
        {
            throw new ArgumentException("Invalid serialized format");
        }

        if (Enum.TryParse<ErrorType>(parts[2], out var type) == false)
        {
            throw new ArgumentException("Invalid serialized format");
        }

        return new Error(parts[0], parts[1], type);
    }
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