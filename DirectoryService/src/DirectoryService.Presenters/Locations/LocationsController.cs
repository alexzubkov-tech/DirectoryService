using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;


namespace DirectoryService.Presenters.Locations;

[ApiController]
[Route("api/locations")]

public class LocationsController: ControllerBase
{
    [HttpPost]

    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(result.Value);

    }
}