using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.Errors;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentIdentifier
{
    // Разрешены: латинские буквы и дефис
    private static readonly Regex AllowedCharsRegex = new(@"^[a-zA-Z-]+$", RegexOptions.Compiled);

    // Для замены любых последовательностей пробельных символов на дефис
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private DepartmentIdentifier(string value) => Value = value;

    public string Value { get; }

    public static Result<DepartmentIdentifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DepartmentDomainErrors.Identifier.Empty();

        string normalized = WhitespaceRegex.Replace(value.Trim(), "-");

        if (string.IsNullOrEmpty(normalized))
            return DepartmentDomainErrors.Identifier.Empty();

        if (normalized.Length < LengthConstants.LENGTH3 || normalized.Length > LengthConstants.LENGTH150)
            return DepartmentDomainErrors.Identifier.InvalidLength(LengthConstants.LENGTH3, LengthConstants.LENGTH150);

        if (!AllowedCharsRegex.IsMatch(normalized))
            return DepartmentDomainErrors.Identifier.InvalidFormat();

        return new DepartmentIdentifier(normalized);
    }
}