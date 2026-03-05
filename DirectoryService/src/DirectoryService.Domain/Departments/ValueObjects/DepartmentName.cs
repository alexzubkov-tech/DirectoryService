using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.Errors;
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
            return DepartmentDomainErrors.Name.Empty();

        if (value.Length < LengthConstants.LENGTH3)
            return DepartmentDomainErrors.Name.TooShort(LengthConstants.LENGTH3);

        if (value.Length > LengthConstants.LENGTH150)
            return DepartmentDomainErrors.Name.TooLong(LengthConstants.LENGTH150);

        return new DepartmentName(value);
    }
}