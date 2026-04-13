using Core.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Create;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;