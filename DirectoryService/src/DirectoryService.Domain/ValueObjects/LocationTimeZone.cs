using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.ValueObjects;

public sealed record LocationTimeZone
{
    private LocationTimeZone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationTimeZone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LocationTimeZone, Error>(
                Error.Validation(
                    "location.timezone.empty",
                    "Time zone cannot be empty"));
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out _))
        {
            return Result.Failure<LocationTimeZone, Error>(
                Error.Validation(
                    "location.timezone.invalid",
                    "Value is not a valid IANA time zone"));
        }

        return Result.Success<LocationTimeZone, Error>(
            new LocationTimeZone(value));
    }

    public override string ToString() => Value;
}