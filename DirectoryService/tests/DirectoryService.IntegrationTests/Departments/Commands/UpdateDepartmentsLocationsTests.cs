using DirectoryService.Application.Departments.Commands.Update.DepartmentsLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments.Commands;

[Collection("Sequential")]
public class UpdateDepartmentsLocationsTests(DirectoryTestWebFactory factory) : DirectoryServiceBaseTests(factory)
{
    [Fact]
    public async Task UpdateDepartmentLocations_with_one_location_should_succeed()
    {
        var loc1 = await CreateLocation("loc-update-one");
        var loc2 = await CreateLocation("loc-update-two");
        var departmentId = await CreateDepartment("DeptOneLocation", "dept-one-loc", null, [loc1.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [loc2.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        await ExecuteInDb(async db =>
        {
            var department = await db.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(departmentId));

            Assert.True(result.IsSuccess);
            Assert.Single(department.DepartmentLocations);
            Assert.Equal(loc2.Value, department.DepartmentLocations.First().LocationId.Value);
        });
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_multiple_locations_should_succeed()
    {
        var loc1 = await CreateLocation("loc-multi-one");
        var loc2 = await CreateLocation("loc-multi-two");
        var loc3 = await CreateLocation("loc-multi-three");

        var departmentId = await CreateDepartment("DeptMultiLocation", "dept-multi-loc", null, [loc1.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [loc1.Value, loc2.Value, loc3.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            var department = await db.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(departmentId));

            Assert.NotNull(department);
            Assert.Equal(3, department.DepartmentLocations.Count);

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .OrderBy(id => id)
                .ToList();

            var expectedLocationIds = new[] { loc1.Value, loc2.Value, loc3.Value }
                .OrderBy(id => id)
                .ToList();

            Assert.Equal(expectedLocationIds, actualLocationIds);
        });
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_not_found_department_should_fail()
    {
        var location = await CreateLocation("loc-notfound-dept");

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(Guid.NewGuid(), [location.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_inactive_department_should_fail()
    {
        var location = await CreateLocation("loc-inactive-dept");
        var departmentId = await CreateDepartment("DeptToInactivate", "dept-to-inactivate", null, [location.Value]);
        await DeactivateDepartment(departmentId);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [location.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_not_found_location_should_fail()
    {
        var location = await CreateLocation("loc-valid-notfound");
        var departmentId = await CreateDepartment("DeptNotFoundLoc", "dept-notfound-loc", null, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [Guid.NewGuid()]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_inactive_location_should_fail()
    {
        var location = await CreateLocation("loc-to-inactivate");
        var departmentId = await CreateDepartment("DeptInactiveLoc", "dept-inactive-loc", null, [location.Value]);
        await DeactivateLocation(location);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [location.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_duplicate_locations_should_fail()
    {
        var location = await CreateLocation("loc-duplicate");
        var departmentId = await CreateDepartment("DeptDuplicateLoc", "dept-duplicate-loc", null, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [location.Value, location.Value]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_empty_locations_should_fail()
    {
        var location = await CreateLocation("loc-empty");
        var departmentId = await CreateDepartment("DeptEmptyLoc", "dept-empty-loc", null, [location.Value]);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, []));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_three_locations_one_valid_one_inactive_one_not_found_should_fail()
    {
        var valid = await CreateLocation("loc-valid-mixed");
        var inactive = await CreateLocation("loc-inactive-mixed");
        var departmentId = await CreateDepartment("DeptMixedLoc", "dept-mixed-loc", null, [valid.Value]);
        await DeactivateLocation(inactive);

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentsLocationsCommand(
                new UpdateDepartmentsLocationsRequest(departmentId, [valid.Value, inactive.Value, Guid.NewGuid()]));
            return sut.Handle(command, CancellationToken.None);
        });

        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentsLocationsHandler, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentsLocationsHandler>();
        return await action(sut);
    }
}