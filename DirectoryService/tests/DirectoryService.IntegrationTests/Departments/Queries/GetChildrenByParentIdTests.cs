using DirectoryService.Application.Departments.Queries.GetChildrenByParentId;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Queries;

[Collection("Sequential")]
public class GetChildrenByParentIdTests : DirectoryServiceBaseTests
{
    public GetChildrenByParentIdTests(DirectoryTestWebFactory factory)
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
    public async Task GetChildrenByParentId_ShouldReturn_AllActiveChildren()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentDepartment", "parent-dept");
        await CreateDepartment("ChildOne", "child-one", parentId);
        await CreateDepartment("ChildTwo", "child-two", parentId);
        await CreateDepartment("ChildThree", "child-three", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetChildrenByParentId_ShouldNotReturn_InactiveChildren()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentInactive", "parent-inactive");
        await CreateDepartment("ActiveChild", "active-child", parentId);
        var inactiveChildId = await CreateDepartment("InactiveChild", "inactive-child", parentId);

        await DeactivateDepartment(inactiveChildId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildrenByParentId_ShouldReturnEmptyList_WhenNoChildren()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentEmpty", "parent-empty");
        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task GetChildrenByParentId_ShouldReturnCorrectPagination_FirstPage()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentPageOne", "parent-page-one");

        await CreateDepartment("ChildPageOne-A", "child-page-one-a", parentId);
        await CreateDepartment("ChildPageOne-B", "child-page-one-b", parentId);
        await CreateDepartment("ChildPageOne-C", "child-page-one-c", parentId);
        await CreateDepartment("ChildPageOne-D", "child-page-one-d", parentId);
        await CreateDepartment("ChildPageOne-E", "child-page-one-e", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 3 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetChildrenByParentId_ShouldReturnCorrectPagination_SecondPage()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentPageTwo", "parent-page-two");

        await CreateDepartment("ChildPageTwo-A", "child-page-two-a", parentId);
        await CreateDepartment("ChildPageTwo-B", "child-page-two-b", parentId);
        await CreateDepartment("ChildPageTwo-C", "child-page-two-c", parentId);
        await CreateDepartment("ChildPageTwo-D", "child-page-two-d", parentId);
        await CreateDepartment("ChildPageTwo-E", "child-page-two-e", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 2, PageSize = 3 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetChildrenByParentId_WithInvalidPage_ShouldFail()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentInvalid", "parent-invalid-page");
        var request = new GetChildrenByParentIdRequest { Page = 0, PageSize = 10 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetChildrenByParentId_WithInvalidPageSize_Zero_ShouldFail()
    {
        await ClearCache();

        var parentId = await CreateDepartment("ParentZero", "parent-zero-size");
        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 0 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(parentId, request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetChildrenByParentId_WithNonExistentParent_ShouldFail()
    {
        await ClearCache();

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetChildrenByParentIdQuery(Guid.NewGuid(), request);
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<GetChildrenByParentIdQueryHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<GetChildrenByParentIdQueryHandler>();
        return await action(sut);
    }
}