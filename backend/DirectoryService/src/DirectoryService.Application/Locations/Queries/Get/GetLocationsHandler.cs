using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Queries.Get;

public class GetLocationsHandler : IQueryHandler<GetLocationsResponse, GetLocationsQuery>
{
    private readonly IReadDbContext _context;
    private readonly IValidator<GetLocationsQuery> _validator;

    public GetLocationsHandler(IReadDbContext context, IValidator<GetLocationsQuery> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result<GetLocationsResponse, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToListError();

        var request = query.Request;

        int page = request.Page;
        int pageSize = request.PageSize;

        var filteredQuery = _context.LocationsRead
            .IgnoreQueryFilters()
            .AsNoTracking();

        if (request.IsActive.HasValue)
            filteredQuery = filteredQuery.Where(l => l.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.ToLower();

            filteredQuery = filteredQuery
                .Where(l => EF.Functions.Like(l.LocationName.Value.ToLower(), $"%{search}%"));
        }

        if (request.DepartmentIds is { Length: > 0 })
        {
            var departmentIdObjects = request.DepartmentIds
                .Select(id => new DepartmentId(id))
                .ToArray();

            filteredQuery = filteredQuery
                .Where(l => l.DepartmentLocations
                    .Any(dl => departmentIdObjects.Contains(dl.DepartmentId)));
        }

        long totalCount = await filteredQuery.LongCountAsync(cancellationToken);

        int totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await filteredQuery
            .OrderBy(l => l.CreatedAt)
            .ThenBy(l => l.LocationName.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationDto
            {
                Id = l.Id.Value,
                Name = l.LocationName.Value,
                Address = new AddressDto
                {
                    Country = l.LocationAddress.Country,
                    City = l.LocationAddress.City,
                    Street = l.LocationAddress.Street,
                    BuildingNumber = l.LocationAddress.BuildingNumber,
                },
                TimeZone = l.Timezone.Value,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                Departments = (
                    from dl in _context.DepartmentLocationsRead
                    join d in _context.DepartmentsRead on dl.DepartmentId equals d.Id
                    where dl.LocationId == l.Id && d.IsActive
                    select new DepartmentInfoDto
                    {
                        Id = d.Id.Value,
                        Identificator = d.DepartmentIdentifier.Value,
                    }
                ).ToList(),
            })
            .ToListAsync(cancellationToken);

        var response = new GetLocationsResponse(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );

        return Result.Success<GetLocationsResponse, Errors>(response);
    }
}
