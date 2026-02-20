using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentIdentifier
{
    private static readonly Regex LatinRegex = new(@"^[a-zA-Z]+$", RegexOptions.Compiled);

    private DepartmentIdentifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentIdentifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("identifier");

        if (LatinRegex.IsMatch(value) == false)
            return GeneralErrors.ValueIsInvalid("identifier"); 

        if (value.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH150)
            return GeneralErrors.ValueIsInvalid("identifier");

        return new DepartmentIdentifier(value);
    }
}