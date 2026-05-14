using Core.Abstractions;
using DirectoryService.Application.Locations.Commands.Create;
using DirectoryService.Application.Locations.Commands.Restore;
using DirectoryService.Application.Locations.Commands.SoftDelete;
using DirectoryService.Application.Locations.Commands.Update;
using DirectoryService.Application.Locations.Queries.GetById;
using DirectoryService.Application.Locations.Queries.GetDapper;
using DirectoryService.Contracts.Locations;
using Framework.Controllers;
using Framework.ResponseExtensions;
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<Guid, UpdateLocationCommand> handler,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(id, request);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsFailure ? result.Error.ToResponse() : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<SoftDeleteLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteLocationCommand(id);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsFailure ? result.Error.ToResponse() : Ok(null);
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RestoreLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new RestoreLocationCommand(id);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsFailure ? result.Error.ToResponse() : Ok(null);
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationDto>> GetLocationById(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<LocationDto, GetLocationByIdQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationByIdQuery(id);

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