using DirectoryService.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.ResponseExtensions;

public static class ResponseExtensions
{
    public static ActionResult ToResponse(this Error error)
    {
        return error.Type switch
        {
            ErrorType.VALIDATION => new BadRequestObjectResult(error.Messages),
            ErrorType.NOT_FOUND => new NotFoundObjectResult(error.Messages),
            ErrorType.FAILURE => new ObjectResult(error.Messages),
            ErrorType.CONFLICT => new ConflictObjectResult(error.Messages),
            _ => new ObjectResult(error.Messages),
        };
    }
}