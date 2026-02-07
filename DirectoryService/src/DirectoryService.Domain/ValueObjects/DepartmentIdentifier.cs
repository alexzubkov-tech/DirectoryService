using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentIdentifier
{
    private static readonly Regex LatinRegex = new(@"^[a-zA-Z]+$", RegexOptions.Compiled);

    private DepartmentIdentifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentIdentifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentIdentifier>("Identifier cannot be empty");

        if (value.Length < LengthConstants.LENGTH3 || value.Length > LengthConstants.LENGTH150)
            return Result.Failure<DepartmentIdentifier>("Identifier length must be between 3 and 150");

        if (!LatinRegex.IsMatch(value))
            return Result.Failure<DepartmentIdentifier>("Identifier must contain only Latin letters");

        return Result.Success(new DepartmentIdentifier(value));
    }

    public override string ToString() => Value;
}