using Core.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Commands.Update;

public record UpdateLocationCommand(Guid LocationId, UpdateLocationRequest Request) : ICommand;