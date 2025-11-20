using System.Collections.ObjectModel;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public sealed record LocationTimeZone
{
    private static readonly ReadOnlyCollection<string> ValidTimezones =
        new(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).ToList());

    private LocationTimeZone(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationTimeZone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationTimeZone>("The time zone code cannot be empty");

        if (!ValidTimezones.Contains(value))
            return Result.Failure<LocationTimeZone>($"Invalid IANA time Zone code: '{value}'");

        return Result.Success(new LocationTimeZone(value));
    }

    public override string ToString() => Value;
}