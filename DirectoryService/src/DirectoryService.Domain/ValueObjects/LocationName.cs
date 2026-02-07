using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record LocationName
{
    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<LocationName>("Location name cannot be empty");

        if (name.Length < LengthConstants.LENGTH3)
            return Result.Failure<LocationName>($"Location name must be at least {LengthConstants.LENGTH3} characters");

        if (name.Length > LengthConstants.LENGTH120)
            return Result.Failure<LocationName>($"Location name must be less than {LengthConstants.LENGTH120} characters");

        return Result.Success(new LocationName(name));
    }

    public override string ToString() => Value;
}