using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Controllers;

[ApiController]
[Route("[controller]")]

public abstract class ApplicationController: ControllerBase
{
    public override OkObjectResult Ok(object? value)
    {
        var envelope = Envelope.Ok(value);

        return new OkObjectResult(envelope);
    }
}