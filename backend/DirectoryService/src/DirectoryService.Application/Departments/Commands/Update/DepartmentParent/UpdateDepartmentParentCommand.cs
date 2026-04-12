using Core.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Commands.Update.DepartmentParent;

public record UpdateDepartmentParentCommand(UpdateDepartmentParentRequest Request): ICommand;