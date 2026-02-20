using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.Errors;
using Shared;
using Shared.Extensions;

namespace DirectoryService.Domain.Locations.ValueObjects;

public partial record LocationName
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
            return LocationNameErrors.Empty();
        }

        string normalized = name.NormalizeSpaces();

        if (normalized.Length < LengthConstants.LENGTH3)
            return LocationNameErrors.TooShort(LengthConstants.LENGTH3);

        if (normalized.Length > LengthConstants.LENGTH120)
            return LocationNameErrors.TooLong(LengthConstants.LENGTH120);

        return new LocationName(normalized);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRemoveRegex();

}