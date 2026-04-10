using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Application.Departments.Commands.SoftDelete;
using DirectoryService.Application.Departments.Commands.Update.DepartmentParent;
using DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Departments.Queries.GetChildrenByParentId;
using DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;
using DirectoryService.Application.Departments.Queries.TopFiveDepartmentsByPosiyions;
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

    [HttpGet("roots")]
    public async Task<IActionResult> GetRootDepartmentsWithPreloadChildren(
        [FromServices] IQueryHandler<GetRootsWithFirstNChildrenResponse,
            GetRootsWithFirstNChildrenQuery> handler,
        [FromQuery] GetRootsWithFirstNChildrenRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetRootsWithFirstNChildrenQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return result.IsFailure
            ? result.Error.ToResponse()
            : Ok(result.Value);
    }

    [HttpGet("{parentId:guid}/children")]
    public async Task<IActionResult> GetChildrenByParentId(
        [FromServices] IQueryHandler<GetChildrenByParentIdResponse,
            GetChildrenByParentIdQuery> handler,
        [FromRoute] Guid parentId,
        [FromQuery] GetChildrenByParentIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChildrenByParentIdQuery(parentId, request);

        var result = await handler.Handle(query, cancellationToken);

        return result.IsFailure
            ? result.Error.ToResponse()
            : Ok(result.Value);
    }

    [HttpDelete("{departmentId:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<SoftDeleteDepartmentCommand> handler,
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsFailure
            ? result.Error.ToResponse()
            : Ok(null);
    }
}