using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record PositionName
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 100;

    private PositionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionName>("Name cannot be empty");

        if (value.Length < MIN_LENGTH)
            return Result.Failure<PositionName>($"Name must be at least {MIN_LENGTH} characters long");

        if (value.Length > MAX_LENGTH)
            return Result.Failure<PositionName>($"Name must be less than {MAX_LENGTH} characters long");

        return Result.Success(new PositionName(value));
    }

    public override string ToString() => Value;
}