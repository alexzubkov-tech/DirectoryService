using DirectoryService.Application.Departments.Queries.RootsWithFirstNChildren;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Queries;

[Collection("Sequential")]
public class GetRootsWithFirstNChildrenTests : DirectoryServiceBaseTests
{
    public GetRootsWithFirstNChildrenTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    private async Task ClearCache()
    {
        var scope = Services.CreateAsyncScope();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync("departments:list", CancellationToken.None);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_ShouldReturn_AllActiveRoots()
    {
        await ClearCache();

        await CreateDepartment("RootOne", "root-one");
        await CreateDepartment("RootTwo", "root-two");
        await CreateDepartment("RootThree", "root-three");

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 10,
            Prefetch = 5,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_ShouldNotReturn_InactiveRoots()
    {
        await ClearCache();

        await CreateDepartment("ActiveRoot", "active-root");
        var inactiveRootId = await CreateDepartment("InactiveRoot", "inactive-root");
        await DeactivateDepartment(inactiveRootId);

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 10,
            Prefetch = 5,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_ShouldReturnChildren_UpToPrefetchLimit()
    {
        await ClearCache();

        var root = await CreateDepartment("RootWithChildren", "root-with-children");
        await CreateDepartment("ChildOne", "child-one-root", root);
        await CreateDepartment("ChildTwo", "child-two-root", root);
        await CreateDepartment("ChildThree", "child-three-root", root);
        await CreateDepartment("ChildFour", "child-four-root", root);
        await CreateDepartment("ChildFive", "child-five-root", root);

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 10,
            Prefetch = 3,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        var rootResult = result.Value.Items.First();
        Assert.NotNull(rootResult.Children);
        Assert.Equal(3, rootResult.Children.Count);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_ShouldReturnEmptyList_WhenNoRoots()
    {
        await ClearCache();

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 10,
            Prefetch = 5,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_ShouldReturnCorrectPagination_FirstPage()
    {
        await ClearCache();

        await CreateDepartment("RootPageOne-A", "root-page-one-a");
        await CreateDepartment("RootPageOne-B", "root-page-one-b");
        await CreateDepartment("RootPageOne-C", "root-page-one-c");
        await CreateDepartment("RootPageOne-D", "root-page-one-d");
        await CreateDepartment("RootPageOne-E", "root-page-one-e");

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 3,
            Prefetch = 2,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_ShouldReturnCorrectPagination_SecondPage()
    {
        await ClearCache();

        await CreateDepartment("RootPageTwo-A", "root-page-two-a");
        await CreateDepartment("RootPageTwo-B", "root-page-two-b");
        await CreateDepartment("RootPageTwo-C", "root-page-two-c");
        await CreateDepartment("RootPageTwo-D", "root-page-two-d");
        await CreateDepartment("RootPageTwo-E", "root-page-two-e");

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 2,
            PageSize = 3,
            Prefetch = 2,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_WithInvalidPage_ShouldFail()
    {
        await ClearCache();

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 0,
            PageSize = 10,
            Prefetch = 5,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_WithInvalidPageSize_TooLarge_ShouldFail()
    {
        await ClearCache();

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 1000,
            Prefetch = 5,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetRootsWithFirstNChildren_WithNegativePrefetch_ShouldFail()
    {
        await ClearCache();

        var request = new GetRootsWithFirstNChildrenRequest
        {
            Page = 1,
            PageSize = 10,
            Prefetch = -1,
        };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetRootsWithFirstNChildrenQuery(request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<GetRootsWithFirstNChildrenQueryHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<GetRootsWithFirstNChildrenQueryHandler>();
        return await action(sut);
    }
}