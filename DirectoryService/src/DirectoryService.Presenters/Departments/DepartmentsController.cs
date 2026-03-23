using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Application.Departments.Commands.Update.DepartmentParent;
using DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;
using DirectoryService.Application.Departments.Queries;
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
        [FromServices] ICommandHandler<UpdateDepartmentsLocationsCommand> handler,
        [FromBody] UpdateDepartmentsLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentsLocationsCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(null);
    }

    [HttpPatch("{departmentId:guid}/parent")]
    public async Task<IActionResult> UpdateDepartmentParent(
        [FromServices] ICommandHandler<UpdateDepartmentParentCommand> handler,
        [FromBody] UpdateDepartmentParentRequest request,
        CancellationToken cancellationToken)
    {

        var command = new UpdateDepartmentParentCommand(request);
        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure ? result.Error.ToResponse() : Ok(null);
    }

    [HttpGet("top-positions")]
    public async Task<IActionResult> GetTopDepartmentsByPositions(
        [FromServices] IQueryHandler<
            List<GetTopFiveDepartmentsByPositionsResponse>,
            GetTopFiveDepartmentsByPositionsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTopFiveDepartmentsByPositionsQuery();

        var result = await handler.Handle(query, cancellationToken);

        return result.IsFailure
            ? result.Error.ToResponse()
            : Ok(result.Value);
    }

}