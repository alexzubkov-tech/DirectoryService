using DirectoryService.Application.Departments.Queries.GetChildrenByParentId;
using DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Caching;

[Collection("Sequential")]
public class CacheInvalidationTests : CacheTestBase
{
    public CacheInvalidationTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_ShouldInvalidate_AllCachedDepartmentQueries()
    {
        await ClearAllDepartmentCacheAsync();

        var root = await CreateDepartment("Root", "root-cache-invalidation");

        var childRequest = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };
        var firstChildrenResult = await GetChildren(root, childRequest);
        Assert.Equal(0, firstChildrenResult.TotalCount);

        await CreateDepartmentViaHandler("ChildNew", "child-new-invalidation", root, null);

        var secondChildrenResult = await GetChildren(root, childRequest);
        Assert.Equal(1, secondChildrenResult.TotalCount);
    }

    [Fact]
    public async Task UpdateDepartmentParent_ShouldInvalidate_AllCachedDepartmentQueries()
    {
        await ClearAllDepartmentCacheAsync();

        var root1 = await CreateDepartment("RootOne", "root-one-update");
        var root2 = await CreateDepartment("RootTwo", "root-two-update");
        var child = await CreateDepartment("Child", "child-update", root1);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var beforeRoot1 = await GetChildren(root1, request);
        var beforeRoot2 = await GetChildren(root2, request);
        Assert.Equal(1, beforeRoot1.TotalCount);
        Assert.Equal(0, beforeRoot2.TotalCount);

        await UpdateDepartmentParent(child, root2);

        var afterRoot1 = await GetChildren(root1, request);
        var afterRoot2 = await GetChildren(root2, request);
        Assert.Equal(0, afterRoot1.TotalCount);
        Assert.Equal(1, afterRoot2.TotalCount);
    }

    [Fact]
    public async Task SoftDeleteDepartment_ShouldInvalidate_AllCachedDepartmentQueries()
    {
        await ClearAllDepartmentCacheAsync();

        var root1 = await CreateDepartment("RootOne", "root-one-delete");
        var root2 = await CreateDepartment("RootTwo", "root-two-delete");

        var rootsRequest = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };

        var beforeRoots = await GetRoots(rootsRequest);
        Assert.Equal(2, beforeRoots.TotalCount);

        await DeactivateDepartment(root1);

        var afterRoots = await GetRoots(rootsRequest);
        Assert.Equal(1, afterRoots.TotalCount);
    }

    [Fact]
    public async Task UpdateDepartmentsLocations_ShouldInvalidate_AllCachedDepartmentQueries()
    {
        await ClearAllDepartmentCacheAsync();

        var location1 = await CreateLocation("Location-One");
        var location2 = await CreateLocation("Location-Two");
        var dept = await CreateDepartment("Dept", "dept-locations-cache", null, [location1.Value]);

        var rootsRequest = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };

        var beforeRoots = await GetRoots(rootsRequest);
        Assert.Equal(1, beforeRoots.TotalCount);

        await UpdateDepartmentLocations(dept, [location2.Value]);

        var afterRoots = await GetRoots(rootsRequest);
        Assert.Equal(1, afterRoots.TotalCount);
    }

    private async Task<GetChildrenByParentIdResponse> GetChildren(Guid parentId, GetChildrenByParentIdRequest request)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetChildrenByParentIdQueryHandler>();
        var result = await handler.Handle(new GetChildrenByParentIdQuery(parentId, request), CancellationToken.None);
        return result.Value;
    }

    private async Task<GetRootsWithFirstNChildrenResponse> GetRoots(GetRootsWithFirstNChildrenRequest request)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetRootsWithFirstNChildrenQueryHandler>();
        var result = await handler.Handle(new GetRootsWithFirstNChildrenQuery(request), CancellationToken.None);
        return result.Value;
    }
}