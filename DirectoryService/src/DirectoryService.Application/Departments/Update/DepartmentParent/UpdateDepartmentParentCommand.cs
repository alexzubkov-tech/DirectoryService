using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Update.DepartmentParent;

public record UpdateDepartmentParentCommand(UpdateDepartmentParentRequest Request): ICommand;