using CSharpFunctionalExtensions;
using System;
using DirectoryService.Domain.Shared;
using TimeZoneConverter; // Нужно установить NuGet пакет TimeZoneConverter

namespace DirectoryService.Domain.ValueObjects;

public sealed record LocationTimeZone
{
    private LocationTimeZone(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationTimeZone, Error> Create(string ianaTimeZone)
    {
        if (string.IsNullOrWhiteSpace(ianaTimeZone))
        {
            return Result.Failure<LocationTimeZone, Error>(
                Error.Validation("timezone.empty", "The time zone code cannot be empty"));
        }

        try
        {
            string windowsTimeZoneId = TZConvert.IanaToWindows(ianaTimeZone);

            TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);

            return Result.Success<LocationTimeZone, Error>(new LocationTimeZone(ianaTimeZone));
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure<LocationTimeZone, Error>(
                Error.Validation("timezone.invalid", $"Invalid IANA time zone code: '{ianaTimeZone}'"));
        }
        catch (InvalidTimeZoneException)
        {
            return Result.Failure<LocationTimeZone, Error>(
                Error.Validation("timezone.invalid", $"Invalid IANA time zone code: '{ianaTimeZone}'"));
        }
    }

    public override string ToString() => Value;

}