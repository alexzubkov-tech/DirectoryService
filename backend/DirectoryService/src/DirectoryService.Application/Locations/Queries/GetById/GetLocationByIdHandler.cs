using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Queries.GetById;

public class GetLocationByIdHandler : IQueryHandler<LocationDto, GetLocationByIdQuery>
{
    private readonly IReadDbContext _context;

    public GetLocationByIdHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LocationDto, Errors>> Handle(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var locationId = new LocationId(query.LocationId);

        var location = await _context.LocationsRead
            .IgnoreQueryFilters()
            .Where(l => l.Id == locationId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            return LocationApplicationErrors.NotFound(query.LocationId).ToErrors();
        }

        return location;
    }
}
