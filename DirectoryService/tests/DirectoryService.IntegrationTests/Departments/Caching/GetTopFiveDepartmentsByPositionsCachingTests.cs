using DirectoryService.Application.Departments.Queries.TopFiveDepartmentsByPosiyions;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Caching;

[Collection("Sequential")]
public class GetTopFiveDepartmentsByPositionsCachingTests : CacheTestBase
{
    public GetTopFiveDepartmentsByPositionsCachingTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_FirstCall_ShouldSucceed_AndCacheResult()
    {
        await ClearAllDepartmentCacheAsync();

        var dept1 = await CreateDepartment("DepartmentOne", "dept-one-top");
        var dept2 = await CreateDepartment("DepartmentTwo", "dept-two-top");
        await CreatePosition(dept1);
        await CreatePosition(dept1);
        await CreatePosition(dept2);

        var result = await GetTopFive();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_SecondCall_ShouldUseCache()
    {
        await ClearAllDepartmentCacheAsync();

        var dept1 = await CreateDepartment("DepartmentOne", "dept-cached-top");
        await CreatePosition(dept1);
        await CreatePosition(dept1);

        var firstResult = await GetTopFive();
        var secondResult = await GetTopFive();

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(firstResult.Value.Count, secondResult.Value.Count);
        Assert.Equal(firstResult.Value[0].PositionCount, secondResult.Value[0].PositionCount);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_AfterAddingNewPosition_ShouldNeedManualCacheClear()
    {
        await ClearAllDepartmentCacheAsync();

        var dept1 = await CreateDepartment("DepartmentOne", "dept-invalidate");
        var dept2 = await CreateDepartment("DepartmentTwo", "dept-two-invalidate");
        await CreatePosition(dept1);
        await CreatePosition(dept1);
        await CreatePosition(dept2);

        var firstResult = await GetTopFive();
        Assert.True(firstResult.IsSuccess);

        await CreatePosition(dept2);

        var cachedResult = await GetTopFive();

        await ClearAllDepartmentCacheAsync();
        var freshResult = await GetTopFive();

        Assert.True(freshResult.IsSuccess);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_AfterDeletingDepartment_ShouldReturnUpdatedData()
    {
        await ClearAllDepartmentCacheAsync();

        var dept1 = await CreateDepartment("DepartmentOne", "dept-delete-top");
        var dept2 = await CreateDepartment("DepartmentTwo", "dept-two-delete-top");
        await CreatePosition(dept1);
        await CreatePosition(dept1);
        await CreatePosition(dept2);

        var firstResult = await GetTopFive();
        Assert.Equal(2, firstResult.Value.Count);

        await DeactivateDepartment(dept1);

        var secondResult = await GetTopFive();
        Assert.Single(secondResult.Value);
    }

    private async Task<CSharpFunctionalExtensions.Result<List<GetTopFiveDepartmentsByPositionsResponse>, Shared.Errors>> GetTopFive()
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetTopFiveDepartmentsByPositionsQueryHandler>();
        return await handler.Handle(new GetTopFiveDepartmentsByPositionsQuery(), CancellationToken.None);
    }
}