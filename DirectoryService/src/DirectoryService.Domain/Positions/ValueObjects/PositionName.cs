using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions.ValueObjects;

public record PositionName
{

    private PositionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionName>("Name cannot be empty");

        if (value.Length < LengthConstants.LENGTH3)
            return Result.Failure<PositionName>($"Name must be at least {LengthConstants.LENGTH3} characters long");

        if (value.Length > LengthConstants.LENGTH100)
            return Result.Failure<PositionName>($"Name must be less than {LengthConstants.LENGTH100} characters long");

        return Result.Success(new PositionName(value));
    }

    public override string ToString() => Value;
}