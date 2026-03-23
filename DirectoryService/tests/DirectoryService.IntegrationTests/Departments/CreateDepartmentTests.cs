using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests(DirectoryTestWebFactory factory) : DirectoryServiceBaseTests(factory)
{
    // Успешные сценарии
    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        var locationId = await CreateLocation();

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Отдел", "otdel", null, [locationId.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value));

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_child_should_succeed()
    {
        var locationId = await CreateLocation();

        var parentId = await CreateDepartment(
            name: "родитель",
            identifier: "parent",
            locationIds: [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Child",
                    "child",
                    parentId,
                    [locationId.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async db =>
        {
            var department = await db.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value));

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
            Assert.NotNull(department.ParentId);
            Assert.Equal(parentId, department.ParentId.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_should_succeed()
    {
        var loc1 = await CreateLocation();
        var loc2 = await CreateLocation(name: "Локация2");
        var loc3 = await CreateLocation(name: "Локация3");

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept-multi",
                    null,
                    [loc1.Value, loc2.Value, loc3.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value));

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_identifier_containing_spaces_should_normalize_and_succeed()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "abc   def", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value));
            Assert.Equal("abc-def", department.DepartmentIdentifier.Value);
        });
    }

    // Негативные тесты: дубликат идентификатора
    [Fact]
    public async Task CreateDepartment_with_duplicate_identifier_should_fail()
    {
        var locationId = await CreateLocation();

        await CreateDepartment(identifier: "same-id", locationIds: [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "same-id",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    // Негативные тесты: родитель
    [Fact]
    public async Task CreateDepartment_with_not_found_parent_should_fail()
    {
        var locationId = await CreateLocation();

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept",
                    Guid.NewGuid(),
                    [locationId.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_parent_should_fail()
    {
        var locationId = await CreateLocation();

        var parentId = await CreateDepartment(locationIds: [locationId.Value]);

        await DeactivateDepartment(parentId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Child",
                    "child",
                    parentId,
                    [locationId.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    // Негативные тесты: локации
    [Fact]
    public async Task CreateDepartment_with_not_found_location_should_fail()
    {
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept",
                    null,
                    [Guid.NewGuid()]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_location_should_fail()
    {
        var locationId = await CreateLocation();

        await DeactivateLocation(locationId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_one_inactive_should_fail()
    {
        var loc1 = await CreateLocation(name: "loc1");
        var loc2 = await CreateLocation(name: "loc2");
        var loc3 = await CreateLocation(name: "loc3");

        await DeactivateLocation(loc2);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept-multi2",
                    null,
                    [loc1.Value, loc2.Value, loc3.Value]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_one_not_found_should_fail()
    {
        var loc1 = await CreateLocation(name: "loc1");
        var loc2 = await CreateLocation(name: "loc2");

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept-multi3",
                    null,
                    [loc1.Value, loc2.Value, Guid.NewGuid()]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_three_locations_one_valid_one_inactive_one_not_found_should_fail()
    {
        var validLocation = await CreateLocation(name: "valid");
        var inactiveLocation = await CreateLocation(name: "inactive");

        await DeactivateLocation(inactiveLocation);

        var notFoundLocation = Guid.NewGuid();

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Dept",
                    "dept-mixed",
                    null,
                    [
                        validLocation.Value,
                        inactiveLocation.Value,
                        notFoundLocation
                    ]));

            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    // Валидация Name
    [Fact]
    public async Task CreateDepartment_with_name_too_short_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Ab", "valid-id", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_name_too_long_should_fail()
    {
        var locationId = await CreateLocation();
        var longName = new string('a', 151);
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(longName, "valid-id", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_name_empty_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("", "valid-id", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_name_whitespace_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("   ", "valid-id", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    // Валидация Identifier
    [Fact]
    public async Task CreateDepartment_with_identifier_too_short_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "ab", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_identifier_too_long_should_fail()
    {
        var locationId = await CreateLocation();
        var longIdentifier = new string('a', 151);
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", longIdentifier, null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_identifier_empty_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_identifier_whitespace_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "   ", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_identifier_invalid_characters_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "abc123", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    // Валидация списка локаций
    [Fact]
    public async Task CreateDepartment_with_empty_location_ids_should_fail()
    {
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "valid-id", null, []));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_duplicate_location_ids_should_fail()
    {
        var locationId = await CreateLocation();
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "valid-id", null, [locationId.Value, locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }
}