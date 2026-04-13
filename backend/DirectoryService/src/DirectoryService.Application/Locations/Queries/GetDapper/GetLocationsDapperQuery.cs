using Core.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Queries.GetDapper;


public record GetLocationsDapperQuery(GetLocationsRequest Request) : IQuery;