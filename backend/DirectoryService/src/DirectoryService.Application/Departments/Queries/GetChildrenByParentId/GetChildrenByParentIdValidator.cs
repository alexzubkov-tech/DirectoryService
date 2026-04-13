using Core.Validation;
using DirectoryService.Application.Common.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Queries.GetChildrenByParentId;

public class GetChildrenByParentIdValidator : AbstractValidator<GetChildrenByParentIdQuery>
{
    public GetChildrenByParentIdValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0)
            .WithError(PaginationErrors.PageMustBePositive());

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .WithError(PaginationErrors.PageSizeMustBePositive())
            .LessThanOrEqualTo(100)
            .WithError(PaginationErrors.PageSizeTooLarge(100));
    }
}