using Core.Abstractions;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Contracts.Positions;
using Framework.Controllers;
using Framework.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Positions;

[Route("api/positions")]
public class PositionsController: ApplicationController
{
    [HttpPost]

    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(result.Value);
    }
}