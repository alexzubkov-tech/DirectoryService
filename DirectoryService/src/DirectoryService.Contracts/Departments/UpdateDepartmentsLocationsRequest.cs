namespace DirectoryService.Contracts.Departments;

public record UpdateDepartmentsLocationsRequest(
    Guid DepartmentId,
    List<Guid> LocationIds);