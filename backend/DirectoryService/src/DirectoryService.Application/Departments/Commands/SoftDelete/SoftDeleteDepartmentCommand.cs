using Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.SoftDelete;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;