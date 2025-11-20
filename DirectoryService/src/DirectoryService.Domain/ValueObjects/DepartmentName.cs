using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentName
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 150;

    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentName>("Name cannot be empty");

        if (value.Length < MIN_LENGTH)
            return Result.Failure<DepartmentName>($"Name must be at least {MIN_LENGTH} characters long");

        if (value.Length > MAX_LENGTH)
            return Result.Failure<DepartmentName>($"Name must be less than {MAX_LENGTH} characters long");

        return Result.Success(new DepartmentName(value));
    }

    public override string ToString() => Value;
}