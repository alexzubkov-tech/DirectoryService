using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentName
{

    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentName>("Name cannot be empty");

        if (value.Length < LengthConstants.LENGTH3)
            return Result.Failure<DepartmentName>($"Name must be at least {LengthConstants.LENGTH3} characters long");

        if (value.Length > LengthConstants.LENGTH150)
            return Result.Failure<DepartmentName>($"Name must be less than {LengthConstants.LENGTH150} characters long");

        return Result.Success(new DepartmentName(value));
    }

    public override string ToString() => Value;
}