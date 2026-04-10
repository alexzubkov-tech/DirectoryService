using DirectoryService.Application.Departments.Commands.SoftDelete;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Commands;

[Collection("Sequential")]
public class SoftDeleteDepartmentTests(DirectoryTestWebFactory factory) : DirectoryServiceBaseTests(factory)
{
    [Fact]
    public async Task SoftDeleteDepartment_with_valid_data_should_succeed()
    {
        var locationId = await CreateLocation("Location-SoftDelete");
        var departmentId = await CreateDepartment("TestDepartment", "test-department-delete", null, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(departmentId));

            Assert.False(department.IsActive);
            Assert.NotNull(department.DeletedAt);
            Assert.Contains("deleted", department.DepartmentPath.Value);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_children_should_update_children_paths()
    {
        var locationId = await CreateLocation("Location-Children");
        var parentId = await CreateDepartment("ParentDepartment", "parent-dept", null, [locationId.Value]);
        var childId = await CreateDepartment("ChildDepartment", "child-dept", parentId, [locationId.Value]);
        var grandChildId = await CreateDepartment("GrandChildDepartment", "grandchild-dept", childId, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(parentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var parent = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(parentId));

            var child = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(childId));

            var grandChild = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(grandChildId));

            Assert.False(parent.IsActive);
            Assert.NotNull(parent.DeletedAt);
            Assert.Contains("deleted", parent.DepartmentPath.Value);

            Assert.True(child.IsActive);
            Assert.True(grandChild.IsActive);
            Assert.Null(child.DeletedAt);
            Assert.Null(grandChild.DeletedAt);

            Assert.Contains("deleted", child.DepartmentPath.Value);
            Assert.Contains("deleted", grandChild.DepartmentPath.Value);
            Assert.StartsWith(parent.DepartmentPath.Value + ".", child.DepartmentPath.Value);
            Assert.StartsWith(child.DepartmentPath.Value + ".", grandChild.DepartmentPath.Value);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_location_used_only_in_this_department_should_deactivate_location()
    {
        var locationId = await CreateLocation("UniqueLocation");
        var departmentId = await CreateDepartment("Department", "department-unique-loc", null, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .IgnoreQueryFilters()
                .FirstAsync(l => l.Id == locationId);

            Assert.False(location.IsActive);
            Assert.NotNull(location.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_location_used_in_other_active_department_should_not_deactivate_location()
    {
        var locationId = await CreateLocation("SharedLocation");

        var department1Id = await CreateDepartment("DepartmentOne", "department-one", null, [locationId.Value]);
        var department2Id = await CreateDepartment("DepartmentTwo", "department-two", null, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(department1Id);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .IgnoreQueryFilters()
                .FirstAsync(l => l.Id == locationId);

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_multiple_locations_used_only_here_should_deactivate_all()
    {
        var location1Id = await CreateLocation("LocationAlpha");
        var location2Id = await CreateLocation("LocationBeta");
        var location3Id = await CreateLocation("LocationGamma");

        var departmentId = await CreateDepartment("Department", "multi-location-dept", null,
            [location1Id.Value, location2Id.Value, location3Id.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var location1 = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == location1Id);
            var location2 = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == location2Id);
            var location3 = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == location3Id);

            Assert.False(location1.IsActive);
            Assert.False(location2.IsActive);
            Assert.False(location3.IsActive);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_multiple_locations_some_shared_should_deactivate_only_unshared()
    {
        var sharedLocationId = await CreateLocation("SharedOffice");
        var uniqueLocationId = await CreateLocation("PrivateOffice");

        var department1Id = await CreateDepartment("DepartmentAlpha", "dept-alpha", null,
            [sharedLocationId.Value, uniqueLocationId.Value]);
        var department2Id = await CreateDepartment("DepartmentBeta", "dept-beta", null, [sharedLocationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(department1Id);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var sharedLocation = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == sharedLocationId);
            var uniqueLocation = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == uniqueLocationId);

            Assert.True(sharedLocation.IsActive);
            Assert.Null(sharedLocation.DeletedAt);

            Assert.False(uniqueLocation.IsActive);
            Assert.NotNull(uniqueLocation.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_already_inactive_department_should_fail()
    {
        var locationId = await CreateLocation("Location-Inactive");
        var departmentId = await CreateDepartment("Department", "department-to-deactivate", null, [locationId.Value]);

        await DeactivateDepartment(departmentId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_non_existent_department_should_fail()
    {
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(Guid.NewGuid());
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_keep_department_locations_relations()
    {
        var locationId = await CreateLocation("Location-Relations");
        var departmentId = await CreateDepartment("Department", "department-with-relations", null, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var relations = await dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == new DepartmentId(departmentId))
                .ToListAsync();

            Assert.NotEmpty(relations);
            Assert.Contains(relations, r => r.LocationId == locationId);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_position_used_only_in_this_department_should_deactivate_position()
    {
        var locationId = await CreateLocation("Location-Position");
        var departmentId = await CreateDepartment("Department", "dept-unique-position", null, [locationId.Value]);

        var positionId = await CreatePosition(departmentId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .IgnoreQueryFilters()
                .FirstAsync(p => p.Id == new PositionId(positionId));

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_position_used_in_other_active_department_should_not_deactivate_position()
    {
        var locationId = await CreateLocation("Location-SharedPosition");
        var department1Id = await CreateDepartment("DepartmentOne", "dept-one", null, [locationId.Value]);
        var department2Id = await CreateDepartment("DepartmentTwo", "dept-two", null, [locationId.Value]);

        var positionId = await CreatePosition(department1Id, department2Id);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(department1Id);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .IgnoreQueryFilters()
                .FirstAsync(p => p.Id == new PositionId(positionId));

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_not_deactivate_child_departments()
    {
        var locationId = await CreateLocation("Location-ChildActive");
        var parentId = await CreateDepartment("ParentDepartment", "parent-dept-child", null, [locationId.Value]);
        var childId = await CreateDepartment("ChildDepartment", "child-dept-active", parentId, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(parentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var child = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(childId));

            Assert.True(child.IsActive);
            Assert.Null(child.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_update_updated_at()
    {
        var locationId = await CreateLocation("Location-UpdatedAt");
        var departmentId = await CreateDepartment("Department", "department-for-update-test", null, [locationId.Value]);

        DateTime? originalUpdatedAt = null;
        await ExecuteInDb(async dbContext =>
        {
            var dept = await dbContext.Departments.FirstAsync(d => d.Id == new DepartmentId(departmentId));
            originalUpdatedAt = dept.UpdatedAt;
        });

        await Task.Delay(1000);
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var dept = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(departmentId));

            Assert.NotEqual(originalUpdatedAt, dept.UpdatedAt);
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<SoftDeleteDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();
        return await action(sut);
    }
}