using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions.Errors;
using Shared;
using Shared.Extensions;

namespace DirectoryService.Domain.Positions.ValueObjects;

public partial record PositionName
{

    private PositionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PositionName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PositionDomainErrors.Name.Empty();

        string normalized = name.NormalizeSpaces();

        if (normalized.Length < LengthConstants.LENGTH3)
            return PositionDomainErrors.Name.TooShort(LengthConstants.LENGTH3);

        if (normalized.Length > LengthConstants.LENGTH100)
            return PositionDomainErrors.Name.TooLong(LengthConstants.LENGTH100);

        return new PositionName(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRemoveRegex();
}

