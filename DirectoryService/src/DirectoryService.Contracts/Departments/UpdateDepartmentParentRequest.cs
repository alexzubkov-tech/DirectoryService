namespace DirectoryService.Contracts.Departments;

public record UpdateDepartmentParentRequest(Guid DepartmentId, Guid? ParentId);