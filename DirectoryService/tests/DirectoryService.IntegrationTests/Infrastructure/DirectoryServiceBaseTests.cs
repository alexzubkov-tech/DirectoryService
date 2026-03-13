using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryServiceBaseTests: IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    protected IServiceProvider Services { get; set; }

    protected DirectoryServiceBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<LocationId> CreateLocation(
        string name = "DefaultLocation",
        string country = "Россия",
        string city = "Москва",
        string street = "Ленина",
        string buildingNumber = "1",
        string timeZone = "Europe/Moscow")
    {
        return await ExecuteInDb(async db =>
        {
            var locationResult = Location.Create(
                LocationName.Create(name).Value,
                LocationAddress.Create(country, city, street, buildingNumber).Value,
                LocationTimeZone.Create(timeZone).Value);

            var location = locationResult.Value;

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return location.Id;
        });
    }

    protected async Task<Guid> CreateDepartment(
        string name = "Department",
        string identifier = "department",
        Guid? parentId = null,
        List<Guid>? locationIds = null)
    {
        locationIds ??= [(await CreateLocation()).Value];

        return await ExecuteInDb(async db =>
        {
            var departmentName = DepartmentName.Create(name).Value;
            var departmentIdentifier = DepartmentIdentifier.Create(identifier).Value;

            var locationIdObjects = locationIds
                .Select(id => new LocationId(id))
                .ToList();

            Result<Department, Error> departmentResult;

            if (parentId is null)
            {
                departmentResult = Department.CreateParent(
                    departmentName,
                    departmentIdentifier,
                    locationIdObjects);
            }
            else
            {
                var parent = await db.Departments
                    .FirstAsync(d => d.Id == new DepartmentId(parentId.Value));

                departmentResult = Department.CreateChild(
                    departmentName,
                    departmentIdentifier,
                    parent,
                    locationIdObjects);
            }

            var department = departmentResult.Value;

            db.Departments.Add(department);
            await db.SaveChangesAsync();

            return department.Id.Value;
        });
    }

    protected async Task DeactivateDepartment(Guid departmentId)
    {
        await ExecuteInDb(async db =>
        {
            var department = await db.Departments.FirstAsync(d => d.Id == new DepartmentId(departmentId));
            department.Deactivate();
            await db.SaveChangesAsync();
        });
    }

    protected async Task DeactivateLocation(LocationId locationId)
    {
        await ExecuteInDb(async db =>
        {
            var location = await db.Locations.FirstAsync(l => l.Id == locationId);
            location.Deactivate();
            await db.SaveChangesAsync();
        });
    }

}