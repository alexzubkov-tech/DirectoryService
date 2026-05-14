using Core.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetById;

public record GetLocationByIdQuery(Guid LocationId) : IQuery;
