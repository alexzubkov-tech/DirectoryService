using Core.Validation;
using DirectoryService.Application.Common.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;

public class GetRootsWithFirstNChildrenQueryValidator: AbstractValidator<GetRootsWithFirstNChildrenQuery>
{
    public GetRootsWithFirstNChildrenQueryValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0)
            .WithError(PaginationErrors.PageMustBePositive());

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .WithError(PaginationErrors.PageSizeMustBePositive())
            .LessThanOrEqualTo(100)
            .WithError(PaginationErrors.PageSizeTooLarge(100));

        RuleFor(x => x.Request.Prefetch)
            .GreaterThanOrEqualTo(0)
            .WithError(PaginationErrors.PrefetchMustBeNonNegative())
            .LessThanOrEqualTo(100)
            .WithError(PaginationErrors.PrefetchTooLarge(100));
    }
}