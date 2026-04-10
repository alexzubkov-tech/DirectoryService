using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;

public record UpdateDepartmentsLocationsCommand(UpdateDepartmentsLocationsRequest Request) : ICommand;