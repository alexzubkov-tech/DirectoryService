using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions.Errors;
using Shared;
using Shared.Extensions;

namespace DirectoryService.Domain.Positions.ValueObjects;

public partial record PositionDescription
{
    private PositionDescription(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static Result<PositionDescription, Error> Create(string? value)
    {
        string? normalized = value?.NormalizeSpaces();

        if (normalized != null && normalized.Length > LengthConstants.LENGTH1000)
        {
            return PositionDomainErrors.Description.TooLong(LengthConstants.LENGTH1000);
        }

        return new PositionDescription(normalized);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRemoveRegex();

}