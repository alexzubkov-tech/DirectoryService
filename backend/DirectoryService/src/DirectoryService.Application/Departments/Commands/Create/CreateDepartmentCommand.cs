using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Commands.Create;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;