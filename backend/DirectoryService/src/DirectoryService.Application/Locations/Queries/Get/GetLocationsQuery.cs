using Core.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Queries.Get;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery;