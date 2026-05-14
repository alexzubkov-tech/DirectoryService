using Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.Restore;

public record RestoreLocationCommand(Guid LocationId) : ICommand;