using Microsoft.AspNetCore.Mvc;
using Shared.SharedKernel;

namespace Framework.Controllers;

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