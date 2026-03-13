using DirectoryService.Application.Departments.Update.DepartmentParent;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentParentTests(DirectoryTestWebFactory factory)
    : DirectoryServiceBaseTests(factory)
{
    // Успешные сценарии
    [Fact]
    public async Task Move_department_to_another_parent_should_succeed()
    {
        var location = await CreateLocation();

        var hq = await CreateDepartment("Headquarters", "hqu", null, [location.Value]);
        var it = await CreateDepartment("IT Department", "itt", hq, [location.Value]);
        var dev = await CreateDepartment("Development", "dev", it, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(dev, hq));

            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async db =>
        {
            var department = await db.Departments
                .FirstAsync(d => d.Id == new DepartmentId(dev));

            Assert.True(result.IsSuccess);
            Assert.Equal(hq, department.ParentId!.Value);
        });
    }

    [Fact]
    public async Task Move_department_should_update_children_paths()
    {
        var location = await CreateLocation();

        var hq = await CreateDepartment("Headquarters", "hqu", null, [location.Value]);
        var sales = await CreateDepartment("Sales", "sales", hq, [location.Value]);
        var regional = await CreateDepartment("Regional", "regional", sales, [location.Value]);
        var south = await CreateDepartment("South", "south", regional, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(regional, hq));

            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async db =>
        {
            var regionalDept = await db.Departments
                .FirstAsync(d => d.Id == new DepartmentId(regional));

            var southDept = await db.Departments
                .FirstAsync(d => d.Id == new DepartmentId(south));

            Assert.True(result.IsSuccess);
            Assert.Equal("hqu.regional", regionalDept.DepartmentPath.Value);
            Assert.Equal("hqu.regional.south", southDept.DepartmentPath.Value);
        });
    }

    [Fact]
    public async Task Move_department_to_root_should_succeed()
    {
        var location = await CreateLocation();

        var hq = await CreateDepartment("Headquarters", "hqu", null, [location.Value]);
        var it = await CreateDepartment("IT Department", "itt", hq, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(it, null));

            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async db =>
        {
            var department = await db.Departments
                .FirstAsync(d => d.Id == new DepartmentId(it));

            Assert.True(result.IsSuccess);
            Assert.Null(department.ParentId);
        });
    }

    // Негативные тесты: проблемы с исходным отделом
    [Fact]
    public async Task Move_department_with_not_found_department_should_fail()
    {
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(Guid.NewGuid(), null));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Move_department_with_inactive_department_should_fail()
    {
        var location = await CreateLocation();

        var dept = await CreateDepartment("Department", "dept", null, [location.Value]);

        await DeactivateDepartment(dept);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(dept, null));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    // Негативные тесты: проблемы с родителем
    [Fact]
    public async Task Move_department_with_not_found_parent_should_fail()
    {
        var location = await CreateLocation();

        var dept = await CreateDepartment("Department", "dept", null, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(dept, Guid.NewGuid()));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Move_department_to_inactive_parent_should_fail()
    {
        var location = await CreateLocation();

        var parent = await CreateDepartment("Parent", "parent", null, [location.Value]);
        var child = await CreateDepartment("Child", "child", null, [location.Value]);

        await DeactivateDepartment(parent);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(child, parent));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    // Негативные тесты: недопустимый родитель (циклические ссылки)
    [Fact]
    public async Task Move_department_to_self_should_fail()
    {
        var location = await CreateLocation();

        var dept = await CreateDepartment("Department", "dept", null, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(dept, dept));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Move_department_to_descendant_should_fail()
    {
        var location = await CreateLocation();

        var hq = await CreateDepartment("Headquarters", "hqu", null, [location.Value]);
        var sales = await CreateDepartment("Sales", "sales", hq, [location.Value]);
        var regional = await CreateDepartment("Regional", "regional", sales, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                new UpdateDepartmentParentRequest(hq, regional));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentParentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentParentHandler>();
        return await action(sut);
    }
}