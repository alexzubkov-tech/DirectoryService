using DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Caching;

[Collection("Sequential")]
public class GetRootsWithFirstNChildrenCachingTests : CacheTestBase
{
    public GetRootsWithFirstNChildrenCachingTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_FirstCall_ShouldSucceed_AndCacheResult()
    {
        await ClearAllDepartmentCacheAsync();

        var root1 = await CreateDepartment("RootOne", "root-one-cache");
        var root2 = await CreateDepartment("RootTwo", "root-two-cache");

        var request = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };
        var result = await GetRoots(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_SecondCall_WithSameParameters_ShouldUseCache()
    {
        await ClearAllDepartmentCacheAsync();

        var root1 = await CreateDepartment("RootOne", "root-one-cache-two");
        await CreateDepartment("ChildOne", "child-one-cache-two", root1);

        var request = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };

        var firstResult = await GetRoots(request);
        var secondResult = await GetRoots(request);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(firstResult.Value.TotalCount, secondResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_DifferentPrefetchParameters_ShouldReturnDifferentChildCounts()
    {
        await ClearAllDepartmentCacheAsync();

        var root1 = await CreateDepartment("RootOne", "root-prefetch");
        await CreateDepartment("ChildA", "child-a-prefetch", root1);
        await CreateDepartment("ChildB", "child-b-prefetch", root1);
        await CreateDepartment("ChildC", "child-c-prefetch", root1);
        await CreateDepartment("ChildD", "child-d-prefetch", root1);

        var requestPrefetch2 = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 2 };
        var requestPrefetch5 = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };

        var result2 = await GetRoots(requestPrefetch2);
        var result5 = await GetRoots(requestPrefetch5);

        var rootFrom2 = result2.Value.Items.First();
        var rootFrom5 = result5.Value.Items.First();

        Assert.Equal(2, rootFrom2.Children?.Count ?? 0);
        Assert.Equal(4, rootFrom5.Children?.Count ?? 0);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_AfterCreatingNewRoot_ShouldReturnUpdatedData()
    {
        await ClearAllDepartmentCacheAsync();
        await CreateDepartment("RootOne", "root-new-cache");

        var request = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };

        var firstResult = await GetRoots(request);
        Assert.Equal(1, firstResult.Value.TotalCount);

        await CreateDepartmentViaHandler("RootTwo", "root-two-new-cache", null, null);

        var secondResult = await GetRoots(request);
        Assert.Equal(2, secondResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_AfterSoftDeletingRoot_ShouldReturnUpdatedData()
    {
        await ClearAllDepartmentCacheAsync();

        var root1 = await CreateDepartment("RootOne", "root-delete-cache");
        var root2 = await CreateDepartment("RootTwo", "root-two-delete-cache");

        var request = new GetRootsWithFirstNChildrenRequest { Page = 1, PageSize = 10, Prefetch = 5 };

        var firstResult = await GetRoots(request);
        Assert.Equal(2, firstResult.Value.TotalCount);

        await DeactivateDepartment(root1);

        var secondResult = await GetRoots(request);
        Assert.Equal(1, secondResult.Value.TotalCount);
    }

    private async Task<CSharpFunctionalExtensions.Result<GetRootsWithFirstNChildrenResponse, Shared.Errors>> GetRoots(GetRootsWithFirstNChildrenRequest request)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetRootsWithFirstNChildrenQueryHandler>();
        return await handler.Handle(new GetRootsWithFirstNChildrenQuery(request), CancellationToken.None);
    }
}