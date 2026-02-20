using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions.ValueObjects;

public record PositionDescription
{
    private PositionDescription(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static Result<PositionDescription> Create(string? value)
    {
        if (value != null && value.Length > LengthConstants.LENGTH1000)
        {
            return Result.Failure<PositionDescription>(
                $"Description must be less than {LengthConstants.LENGTH1000} characters long");
        }

        return Result.Success(new PositionDescription(value));
    }
}