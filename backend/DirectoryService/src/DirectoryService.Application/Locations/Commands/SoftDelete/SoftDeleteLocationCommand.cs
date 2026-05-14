using Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.SoftDelete;

public record SoftDeleteLocationCommand(Guid LocationId) : ICommand;