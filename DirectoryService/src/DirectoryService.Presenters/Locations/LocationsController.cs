using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presenters.Controllers;
using DirectoryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;


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
}