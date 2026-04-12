using DirectoryService.Application.Departments.Commands.Update.DepartmentParent;
using DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

[Collection("Sequential")]
public abstract class CacheTestBase : DirectoryServiceBaseTests
{
    protected CacheTestBase(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    protected async Task ClearAllDepartmentCacheAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync("departments:list", CancellationToken.None);
        await Task.Delay(500);
    }

    protected async Task UpdateDepartmentParent(Guid departmentId, Guid? newParentId)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<UpdateDepartmentParentHandler>();
        var command = new UpdateDepartmentParentCommand(new UpdateDepartmentParentRequest(departmentId, newParentId));
        await handler.Handle(command, CancellationToken.None);
        await Task.Delay(300);
    }

    protected async Task UpdateDepartmentLocations(Guid departmentId, List<Guid> locationIds)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<UpdateDepartmentsLocationsHandler>();
        var command = new UpdateDepartmentsLocationsCommand(new UpdateDepartmentsLocationsRequest(departmentId, locationIds));
        await handler.Handle(command, CancellationToken.None);
        await Task.Delay(300);
    }
}