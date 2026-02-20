using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentName
{

    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department_name");

        if (value.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH150)
            return GeneralErrors.ValueIsInvalid("department_name");

        return new DepartmentName(value);
    }
}