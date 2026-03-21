using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Commands.Create;
using DirectoryService.Application.Locations.Queries.GetDapper;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presenters.Controllers;
using DirectoryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using GetLocationsQuery = DirectoryService.Application.Locations.Queries.Get.GetLocationsQuery;

namespace DirectoryService.Presenters.Locations;

[Route("api/locations")]
public class LocationsController: ApplicationController
{
    [HttpPost]

    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<GetLocationsResponse>> GetLocations(
        [FromServices] IQueryHandler<GetLocationsResponse, GetLocationsQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return result.IsFailure
            ? result.Error.ToResponse()
            : Ok(result.Value);
    }

    [HttpGet("dapper")]
    public async Task<ActionResult<GetLocationsResponseDapper>> GetLocationsDapper(
        [FromServices] IQueryHandler<GetLocationsResponseDapper, GetLocationsDapperQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsDapperQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return result.IsFailure
            ? result.Error.ToResponse()
            : Ok(result.Value);
    }

}