using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Commands.Create;

public record CreateLocationCommand(CreateLocationRequest Request): ICommand;