using Shared;

namespace DirectoryService.Application.Common.Errors;

public class PaginationErrors
{
    public static Error PageMustBePositive() =>
        Error.Validation("pagination.page.positive", "Номер страницы должен быть больше 0.");

    public static Error PageSizeMustBePositive() =>
        Error.Validation("pagination.pageSize.positive", "Размер страницы должен быть больше 0.");

    public static Error PageSizeTooLarge(int max) =>
        Error.Validation("pagination.pageSize.max", $"Размер страницы не может превышать {max}.");

    public static Error PrefetchMustBeNonNegative() =>
        Error.Validation("prefetch.nonNegative", "Количество предзагружаемых детей должно быть больше или равно 0.");

    public static Error PrefetchTooLarge(int max) =>
        Error.Validation("prefetch.tooLarge", $"Количество предзагружаемых детей не может превышать {max}.");
}