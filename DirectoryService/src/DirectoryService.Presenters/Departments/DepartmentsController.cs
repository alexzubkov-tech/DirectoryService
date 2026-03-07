using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.Update.DepartmentsLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presenters.Controllers;
using DirectoryService.Presenters.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Departments;

[Route("api/departments")]
public class DepartmentsController: ApplicationController
{
    [HttpPost]

    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(result.Value);
    }

    [HttpPatch("{departmentId:guid}/locations")]
    public async Task<IActionResult> UpdateDepartmentsLocations(
        [FromServices] ICommandHandler<Guid, UpdateDepartmentsLocationsCommand> handler,
        [FromBody] UpdateDepartmentsLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentsLocationsCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(result.Value);
    }
}