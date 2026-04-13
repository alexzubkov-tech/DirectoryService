using System.Text.RegularExpressions;

namespace Shared.SharedKernel;

public static partial class StringExtensions
{
    public static string NormalizeSpaces(this string value)
    {
        return SpaceRegex().Replace(value.Trim(), " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
}