using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record LocationName
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 120;

    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<LocationName>("Location name cannot be empty");

        if (name.Length < MIN_LENGTH)
            return Result.Failure<LocationName>($"Location name must be at least {MIN_LENGTH} characters");

        if (name.Length > MAX_LENGTH)
            return Result.Failure<LocationName>($"Location name must be less than {MAX_LENGTH} characters");

        return Result.Success(new LocationName(name));
    }

    public override string ToString() => Value;
}