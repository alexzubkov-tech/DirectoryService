using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.Errors;
using Shared;
using Shared.Extensions;

namespace DirectoryService.Domain.Locations.ValueObjects;

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
           return GeneralErrors.ValueIsRequired("LocationTimeZone");
        }

        string normalized = value.NormalizeSpaces();

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(normalized, out _))
        {
           return LocationTimeZoneErrors.Invalid(normalized);
        }

        return new LocationTimeZone(normalized);
    }

    public override string ToString() => Value;
}