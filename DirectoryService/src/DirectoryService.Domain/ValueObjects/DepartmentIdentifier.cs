using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentIdentifier
{
    public const int MIN_LENGTH = 3;

    public const short MAX_LENGTH = 150;

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

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            return Result.Failure<DepartmentIdentifier>("Identifier length must be between 3 and 150");

        if (!LatinRegex.IsMatch(value))
            return Result.Failure<DepartmentIdentifier>("Identifier must contain only Latin letters");

        return Result.Success(new DepartmentIdentifier(value));
    }

    public override string ToString() => Value;
}