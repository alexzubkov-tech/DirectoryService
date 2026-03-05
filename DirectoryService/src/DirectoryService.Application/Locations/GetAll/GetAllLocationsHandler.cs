using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations.GetAll;

public class GetAllLocationsHandler : IQueryHandler<IReadOnlyList<LocationDto>, GetAllLocationsQuery>
{
    private readonly ILogger<GetAllLocationsHandler> _logger;
    private readonly ILocationsRepository _repository;


    public GetAllLocationsHandler(ILogger<GetAllLocationsHandler> logger, ILocationsRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<LocationDto>, Errors>> Handle(GetAllLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var locations = await _repository.GetAllAsync(cancellationToken);

        var dtos = locations
            .Select(l => new LocationDto(
                l.Id.Value,
                l.LocationName.Value,
                new AddressDto(
                    l.LocationAddress.Country,
                    l.LocationAddress.City,
                    l.LocationAddress.Street,
                    l.LocationAddress.BuildingNumber),
                l.Timezone.Value))
            .ToList();

        return Result.Success<IReadOnlyList<LocationDto>, Errors>(dtos);
    }
}