using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

[Collection("Sequential")]
public class CreateDepartmentTests(DirectoryTestWebFactory factory) : DirectoryServiceBaseTests(factory)
{
    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        var locationId = await CreateLocation("Location-Valid");
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Отдел", "otdel-valid", null, [locationId.Value]));
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
        var locationId = await CreateLocation("Location-Child");
        var parentId = await CreateDepartment("родитель", "parent-dept", null, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Child", "child-dept", parentId, [locationId.Value]));
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
        var loc1 = await CreateLocation("Локация-Один");
        var loc2 = await CreateLocation("Локация-Два");
        var loc3 = await CreateLocation("Локация-Три");

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-multi", null, [loc1.Value, loc2.Value, loc3.Value]));
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
        var locationId = await CreateLocation("Location-Spaces");
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

    [Fact]
    public async Task CreateDepartment_with_duplicate_identifier_should_fail()
    {
        var locationId = await CreateLocation("Location-Duplicate");
        await CreateDepartment("First Dept", "same-identifier", null, [locationId.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "same-identifier", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_not_found_parent_should_fail()
    {
        var locationId = await CreateLocation("Location-NotFoundParent");

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-notfound", Guid.NewGuid(), [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_parent_should_fail()
    {
        var locationId = await CreateLocation("Location-InactiveParent");
        var parentId = await CreateDepartment("InactiveParent", "inactive-parent", null, [locationId.Value]);
        await DeactivateDepartment(parentId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Child", "child-inactive", parentId, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_not_found_location_should_fail()
    {
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-notfound-loc", null, [Guid.NewGuid()]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_location_should_fail()
    {
        var locationId = await CreateLocation("Location-Inactive");
        await DeactivateLocation(locationId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-inactive-loc", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_one_inactive_should_fail()
    {
        var loc1 = await CreateLocation("loc-valid");
        var loc2 = await CreateLocation("loc-inactive");
        var loc3 = await CreateLocation("loc-extra");
        await DeactivateLocation(loc2);

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-multi-two", null, [loc1.Value, loc2.Value, loc3.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_one_not_found_should_fail()
    {
        var loc1 = await CreateLocation("loc-one");
        var loc2 = await CreateLocation("loc-two");

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-multi-three", null, [loc1.Value, loc2.Value, Guid.NewGuid()]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_three_locations_one_valid_one_inactive_one_not_found_should_fail()
    {
        var validLocation = await CreateLocation("valid-loc");
        var inactiveLocation = await CreateLocation("inactive-loc");
        await DeactivateLocation(inactiveLocation);
        var notFoundLocation = Guid.NewGuid();

        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Dept", "dept-mixed", null,
                    [validLocation.Value, inactiveLocation.Value, notFoundLocation]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_name_too_short_should_fail()
    {
        var locationId = await CreateLocation("Location-Short");
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
        var locationId = await CreateLocation("Location-Long");
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
        var locationId = await CreateLocation("Location-Empty");
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
        var locationId = await CreateLocation("Location-Whitespace");
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("   ", "valid-id", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_identifier_too_short_should_fail()
    {
        var locationId = await CreateLocation("Location-IdShort");
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
        var locationId = await CreateLocation("Location-IdLong");
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
        var locationId = await CreateLocation("Location-IdEmpty");
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
        var locationId = await CreateLocation("Location-IdWhitespace");
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
        var locationId = await CreateLocation("Location-IdInvalid");
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Valid Name", "abc123", null, [locationId.Value]));
            return sut.Handle(command, CancellationToken.None);
        });
        Assert.True(result.IsFailure);
    }

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
        var locationId = await CreateLocation("Location-DuplicateInList");
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