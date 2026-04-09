using DirectoryService.Application.Departments.Queries.TopFiveDepartmentsByPosiyions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Queries;

[Collection("Sequential")]
public class GetTopFiveDepartmentsByPositionsTests : DirectoryServiceBaseTests
{
    public GetTopFiveDepartmentsByPositionsTests(DirectoryTestWebFactory factory)
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
    public async Task GetTopFiveDepartmentsByPositions_ShouldReturn_TopFiveOrderedByPositionCount()
    {
        await ClearCache();

        var deptA = await CreateDepartment("DepartmentA", "dept-a");
        var deptB = await CreateDepartment("DepartmentB", "dept-b");
        var deptC = await CreateDepartment("DepartmentC", "dept-c");
        var deptD = await CreateDepartment("DepartmentD", "dept-d");
        var deptE = await CreateDepartment("DepartmentE", "dept-e");
        var deptF = await CreateDepartment("DepartmentF", "dept-f");

        for (int i = 0; i < 6; i++) await CreatePosition(deptF);
        for (int i = 0; i < 5; i++) await CreatePosition(deptE);
        for (int i = 0; i < 4; i++) await CreatePosition(deptD);
        for (int i = 0; i < 3; i++) await CreatePosition(deptC);
        for (int i = 0; i < 2; i++) await CreatePosition(deptB);
        await CreatePosition(deptA);

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetTopFiveDepartmentsByPositionsQuery();
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Count);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_ShouldReturn_LessThanFive_WhenNotEnoughDepartments()
    {
        await ClearCache();

        var deptOne = await CreateDepartment("FirstDept", "first-dept");
        var deptTwo = await CreateDepartment("SecondDept", "second-dept");

        await CreatePosition(deptOne);
        await CreatePosition(deptOne);
        await CreatePosition(deptTwo);

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetTopFiveDepartmentsByPositionsQuery();
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_ShouldReturnEmpty_WhenNoPositions()
    {
        await ClearCache();

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetTopFiveDepartmentsByPositionsQuery();
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_ShouldOnlyCount_ActiveDepartments()
    {
        await ClearCache();

        var activeDept = await CreateDepartment("ActiveDepartment", "active-dept");
        var inactiveDept = await CreateDepartment("InactiveDepartment", "inactive-dept");

        await CreatePosition(activeDept);
        await CreatePosition(activeDept);
        await CreatePosition(inactiveDept);

        await DeactivateDepartment(inactiveDept);

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetTopFiveDepartmentsByPositionsQuery();
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(2, result.Value[0].PositionCount);
    }

    [Fact]
    public async Task GetTopFiveDepartmentsByPositions_ShouldCountPositions_FromMultipleDepartments()
    {
        await ClearCache();

        var deptOne = await CreateDepartment("MultiDeptOne", "multi-dept-one");
        var deptTwo = await CreateDepartment("MultiDeptTwo", "multi-dept-two");
        var deptThree = await CreateDepartment("MultiDeptThree", "multi-dept-three");

        await CreatePosition(deptOne, deptTwo, deptThree);
        await CreatePosition(deptOne);
        await CreatePosition(deptOne);

        var result = await ExecuteHandler(handler =>
        {
            var query = new GetTopFiveDepartmentsByPositionsQuery();
            return handler.Handle(query, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
    }

    private async Task<T> ExecuteHandler<T>(Func<GetTopFiveDepartmentsByPositionsQueryHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<GetTopFiveDepartmentsByPositionsQueryHandler>();
        return await action(sut);
    }
}