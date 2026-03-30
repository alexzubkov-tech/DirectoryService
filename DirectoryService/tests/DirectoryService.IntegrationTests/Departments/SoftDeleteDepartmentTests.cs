using DirectoryService.Application.Departments.Commands.SoftDelete;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

[Collection("Sequential")]
public class SoftDeleteDepartmentTests(DirectoryTestWebFactory factory) : DirectoryServiceBaseTests(factory)
{
    // ==================== Успешные сценарии ====================
    [Fact]
    public async Task SoftDeleteDepartment_with_valid_data_should_succeed()
    {
        // Arrange
        var locationId = await CreateLocation();
        var departmentId = await CreateDepartment(
            name: "TestDepartment",
            identifier: "test-department",
            locationIds: [locationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
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
        // Arrange
        var locationId = await CreateLocation();
        var parentId = await CreateDepartment(
            name: "ParentDepartment",
            identifier: "parent-dept",
            locationIds: [locationId.Value]);

        var childId = await CreateDepartment(
            name: "ChildDepartment",
            identifier: "child-dept",
            parentId: parentId,
            locationIds: [locationId.Value]);

        var grandChildId = await CreateDepartment(
            name: "GrandChildDepartment",
            identifier: "grandchild-dept",
            parentId: childId,
            locationIds: [locationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(parentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
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

            // Parent должен быть неактивен
            Assert.False(parent.IsActive);
            Assert.NotNull(parent.DeletedAt);
            Assert.Contains("deleted", parent.DepartmentPath.Value);

            // Children должны остаться активными
            Assert.True(child.IsActive);
            Assert.True(grandChild.IsActive);
            Assert.Null(child.DeletedAt);
            Assert.Null(grandChild.DeletedAt);

            // Пути детей должны обновиться
            Assert.Contains("deleted", child.DepartmentPath.Value);
            Assert.Contains("deleted", grandChild.DepartmentPath.Value);
            Assert.StartsWith(parent.DepartmentPath.Value + ".", child.DepartmentPath.Value);
            Assert.StartsWith(child.DepartmentPath.Value + ".", grandChild.DepartmentPath.Value);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_location_used_only_in_this_department_should_deactivate_location()
    {
        // Arrange
        var locationId = await CreateLocation(name: "UniqueLocation");
        var departmentId = await CreateDepartment(
            name: "Department",
            identifier: "department-with-unique-location",
            locationIds: [locationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
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
        // Arrange
        var locationId = await CreateLocation(name: "SharedLocation");

        var department1Id = await CreateDepartment(
            name: "DepartmentOne",
            identifier: "department-one",
            locationIds: [locationId.Value]);

        var department2Id = await CreateDepartment(
            name: "DepartmentTwo",
            identifier: "department-two",
            locationIds: [locationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(department1Id);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .IgnoreQueryFilters()
                .FirstAsync(l => l.Id == locationId);

            // Локация должна остаться активной, так как используется в department2
            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_multiple_locations_used_only_here_should_deactivate_all()
    {
        // Arrange
        var location1Id = await CreateLocation(name: "LocationAlpha");
        var location2Id = await CreateLocation(name: "LocationBeta");
        var location3Id = await CreateLocation(name: "LocationGamma");

        var departmentId = await CreateDepartment(
            name: "Department",
            identifier: "multi-location-dept",
            locationIds: [location1Id.Value, location2Id.Value, location3Id.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
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
        // Arrange
        var sharedLocationId = await CreateLocation(name: "SharedOffice");
        var uniqueLocationId = await CreateLocation(name: "PrivateOffice");

        var department1Id = await CreateDepartment(
            name: "DepartmentAlpha",
            identifier: "dept-alpha",
            locationIds: [sharedLocationId.Value, uniqueLocationId.Value]);

        var department2Id = await CreateDepartment(
            name: "DepartmentBeta",
            identifier: "dept-beta",
            locationIds: [sharedLocationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(department1Id);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var sharedLocation = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == sharedLocationId);
            var uniqueLocation = await dbContext.Locations.IgnoreQueryFilters().FirstAsync(l => l.Id == uniqueLocationId);

            // Shared location должна остаться активной
            Assert.True(sharedLocation.IsActive);
            Assert.Null(sharedLocation.DeletedAt);

            // Unique location должна быть деактивирована
            Assert.False(uniqueLocation.IsActive);
            Assert.NotNull(uniqueLocation.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_already_inactive_department_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation();
        var departmentId = await CreateDepartment(
            name: "Department",
            identifier: "department-to-deactivate",
            locationIds: [locationId.Value]);

        await DeactivateDepartment(departmentId);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_non_existent_department_should_fail()
    {
        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(Guid.NewGuid());
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_keep_department_locations_relations()
    {
        // Arrange
        var locationId = await CreateLocation();
        var departmentId = await CreateDepartment(
            name: "Department",
            identifier: "department-with-relations",
            locationIds: [locationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var relations = await dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == new DepartmentId(departmentId))
                .ToListAsync();

            // Связи должны остаться
            Assert.NotEmpty(relations);
            Assert.Contains(relations, r => r.LocationId == locationId);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_position_used_only_in_this_department_should_deactivate_position()
    {
        // Arrange
        var locationId = await CreateLocation();
        var departmentId = await CreateDepartment(
            name: "Department",
            identifier: "dept-with-unique-position",
            locationIds: [locationId.Value]);

        // Создаем позицию и привязываем к департаменту
        var positionId = await CreatePosition(departmentId);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
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
        // Arrange
        var locationId = await CreateLocation();
        var department1Id = await CreateDepartment(
            name: "DepartmentOne",
            identifier: "dept-one",
            locationIds: [locationId.Value]);

        var department2Id = await CreateDepartment(
            name: "DepartmentTwo",
            identifier: "dept-two",
            locationIds: [locationId.Value]);

        // Создаем позицию и привязываем к обоим департаментам
        var positionId = await CreatePosition(department1Id, department2Id);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(department1Id);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .IgnoreQueryFilters()
                .FirstAsync(p => p.Id == new PositionId(positionId));

            // Позиция должна остаться активной
            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_not_deactivate_child_departments()
    {
        // Arrange
        var locationId = await CreateLocation();
        var parentId = await CreateDepartment(
            name: "ParentDepartment",
            identifier: "parent-dept",
            locationIds: [locationId.Value]);

        var childId = await CreateDepartment(
            name: "ChildDepartment",
            identifier: "child-dept",
            parentId: parentId,
            locationIds: [locationId.Value]);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(parentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var child = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == new DepartmentId(childId));

            // Дочерний отдел должен остаться активным
            Assert.True(child.IsActive);
            Assert.Null(child.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_update_updated_at()
    {
        // Arrange
        var locationId = await CreateLocation();
        var departmentId = await CreateDepartment(
            name: "Department",
            identifier: "department-for-update-test",
            locationIds: [locationId.Value]);

        DateTime? originalUpdatedAt = null;
        await ExecuteInDb(async dbContext =>
        {
            var dept = await dbContext.Departments.FirstAsync(d => d.Id == new DepartmentId(departmentId));
            originalUpdatedAt = dept.UpdatedAt;
        });

        // Act
        await Task.Delay(1000);
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
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