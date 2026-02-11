using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.ValueObjects;

public record LocationName
{
    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<LocationName, Error>(
                Error.Validation("name.empty", "Location name cannot be empty"));
        }

        if (name.Length < LengthConstants.LENGTH3)
        {
            return Result.Failure<LocationName, Error>(
                Error.Validation("name.too_short", "Location name must be at least 3 characters"));
        }

        if (name.Length > LengthConstants.LENGTH120)
        {
            return Result.Failure<LocationName, Error>(
                Error.Validation("name.too_long", "Location name must be less than 120 characters"));
        }

        return Result.Success<LocationName, Error>(new LocationName(name));
    }

    public override string ToString() => Value;
}