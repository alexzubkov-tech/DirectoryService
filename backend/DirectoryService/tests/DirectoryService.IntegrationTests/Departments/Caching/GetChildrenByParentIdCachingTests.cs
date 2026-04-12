using DirectoryService.Application.Departments.Queries.GetChildrenByParentId;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Caching;

[Collection("Sequential")]
public class GetChildrenByParentIdCachingTests : CacheTestBase
{
    public GetChildrenByParentIdCachingTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetChildrenByParentId_FirstCall_ShouldSucceed_AndCacheResult()
    {
        await ClearAllDepartmentCacheAsync();
        var parentId = await CreateDepartment("Parent", "parent-cache-one");
        await CreateDepartment("ChildOne", "child-one-cache", parentId);
        await CreateDepartment("ChildTwo", "child-two-cache", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };
        var result = await GetChildren(parentId, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildrenByParentId_SecondCall_WithSameParameters_ShouldUseCache()
    {
        await ClearAllDepartmentCacheAsync();
        var parentId = await CreateDepartment("Parent", "parent-cache-two");
        await CreateDepartment("ChildOne", "child-one-cache-two", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var firstResult = await GetChildren(parentId, request);
        var secondResult = await GetChildren(parentId, request);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(firstResult.Value.TotalCount, secondResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildrenByParentId_DifferentPageParameters_ShouldReturnDifferentData()
    {
        await ClearAllDepartmentCacheAsync();
        var parentId = await CreateDepartment("Parent", "parent-cache-three");

        await CreateDepartment("ChildA", "child-a-cache", parentId);
        await CreateDepartment("ChildB", "child-b-cache", parentId);
        await CreateDepartment("ChildC", "child-c-cache", parentId);
        await CreateDepartment("ChildD", "child-d-cache", parentId);
        await CreateDepartment("ChildE", "child-e-cache", parentId);

        var requestPage1 = new GetChildrenByParentIdRequest { Page = 1, PageSize = 2 };
        var requestPage2 = new GetChildrenByParentIdRequest { Page = 2, PageSize = 2 };
        var requestPage3 = new GetChildrenByParentIdRequest { Page = 3, PageSize = 2 };

        var resultPage1 = await GetChildren(parentId, requestPage1);
        var resultPage2 = await GetChildren(parentId, requestPage2);
        var resultPage3 = await GetChildren(parentId, requestPage3);

        Assert.Equal(2, resultPage1.Value.Items.Count);
        Assert.Equal(2, resultPage2.Value.Items.Count);
        Assert.Single(resultPage3.Value.Items);

        Assert.NotEqual(resultPage1.Value.Items[0].Id, resultPage2.Value.Items[0].Id);
    }

    [Fact]
    public async Task GetChildrenByParentId_DifferentParents_ShouldReturnDifferentData()
    {
        await ClearAllDepartmentCacheAsync();
        var parent1Id = await CreateDepartment("ParentOne", "parent-one-cache");
        var parent2Id = await CreateDepartment("ParentTwo", "parent-two-cache");

        await CreateDepartment("ChildOne", "child-one-parentone", parent1Id);
        await CreateDepartment("ChildTwo", "child-two-parenttwo", parent2Id);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var resultParent1 = await GetChildren(parent1Id, request);
        var resultParent2 = await GetChildren(parent2Id, request);

        Assert.Equal(1, resultParent1.Value.TotalCount);
        Assert.Equal(1, resultParent2.Value.TotalCount);
        Assert.NotEqual(resultParent1.Value.Items[0].Id, resultParent2.Value.Items[0].Id);
    }

    [Fact]
    public async Task GetChildrenByParentId_AfterCreatingNewChild_ShouldReturnUpdatedData()
    {
        await ClearAllDepartmentCacheAsync();
        var parentId = await CreateDepartment("Parent", "parent-cache-invalidate");
        await CreateDepartment("ChildOne", "child-one-invalidate", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var firstResult = await GetChildren(parentId, request);
        Assert.Equal(1, firstResult.Value.TotalCount);

        await CreateDepartmentViaHandler("ChildTwo", "child-two-invalidate", parentId, null);

        var secondResult = await GetChildren(parentId, request);
        Assert.Equal(2, secondResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildrenByParentId_AfterUpdatingDepartmentParent_ShouldReturnUpdatedData()
    {
        await ClearAllDepartmentCacheAsync();
        var parent1Id = await CreateDepartment("ParentOne", "parent-one-update");
        var parent2Id = await CreateDepartment("ParentTwo", "parent-two-update");
        var childId = await CreateDepartment("Child", "child-update", parent1Id);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var beforeParent1 = await GetChildren(parent1Id, request);
        var beforeParent2 = await GetChildren(parent2Id, request);
        Assert.Equal(1, beforeParent1.Value.TotalCount);
        Assert.Equal(0, beforeParent2.Value.TotalCount);

        await UpdateDepartmentParent(childId, parent2Id);

        var afterParent1 = await GetChildren(parent1Id, request);
        var afterParent2 = await GetChildren(parent2Id, request);
        Assert.Equal(0, afterParent1.Value.TotalCount);
        Assert.Equal(1, afterParent2.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildrenByParentId_AfterSoftDeletingDepartment_ShouldReturnUpdatedData()
    {
        await ClearAllDepartmentCacheAsync();
        var parentId = await CreateDepartment("Parent", "parent-delete-cache");
        var childId = await CreateDepartment("Child", "child-delete-cache", parentId);

        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var firstResult = await GetChildren(parentId, request);
        Assert.Equal(1, firstResult.Value.TotalCount);

        await DeactivateDepartment(childId);

        var secondResult = await GetChildren(parentId, request);
        Assert.Equal(0, secondResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildrenByParentId_WithNonExistentParent_ShouldFail()
    {
        await ClearAllDepartmentCacheAsync();
        var nonExistentParentId = Guid.NewGuid();
        var request = new GetChildrenByParentIdRequest { Page = 1, PageSize = 10 };

        var result = await GetChildren(nonExistentParentId, request);
        Assert.True(result.IsFailure);
    }

    private async Task<CSharpFunctionalExtensions.Result<GetChildrenByParentIdResponse, Errors>> GetChildren(
        Guid parentId,
        GetChildrenByParentIdRequest request)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetChildrenByParentIdQueryHandler>();
        return await handler.Handle(new GetChildrenByParentIdQuery(parentId, request), CancellationToken.None);
    }
}