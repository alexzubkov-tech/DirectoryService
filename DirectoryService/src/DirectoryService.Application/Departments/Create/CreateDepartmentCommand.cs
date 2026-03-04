using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Create;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;