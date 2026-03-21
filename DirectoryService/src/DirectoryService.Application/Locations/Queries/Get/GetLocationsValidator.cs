using DirectoryService.Application.Locations.Fails;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Locations.Queries.Get;

public class GetLocationsValidator: AbstractValidator<GetLocationsQuery>
{
    public GetLocationsValidator()
    {
        When(x => true, () =>
        {
            RuleFor(x => x.Request.DepartmentIds)
                .Must(ids => ids == null || ids.Distinct().Count() == ids.Length)
                .WithError(LocationApplicationErrors.DepartmentIds.Duplicate());

            RuleFor(x => x.Request.Search)
                .MaximumLength(120)
                .WithError(Error.Validation("search.too.long", "Поисковый запрос не может быть длиннее 120 символов."))
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Search));

            When(x => x.Request.Pagination != null, () =>
            {
                RuleFor(x => x.Request.Pagination!.Page)
                    .GreaterThan(0)
                    .WithError(LocationApplicationErrors.Pagination.PageMustBePositive());

                RuleFor(x => x.Request.Pagination!.PageSize)
                    .GreaterThan(0)
                    .WithError(LocationApplicationErrors.Pagination.PageSizeMustBePositive())
                    .LessThanOrEqualTo(100)
                    .WithError(LocationApplicationErrors.Pagination.PageSizeTooLarge(100));
            });
        });
    }
}