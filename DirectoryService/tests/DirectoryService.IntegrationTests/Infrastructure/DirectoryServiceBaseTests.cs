using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Commands.SoftDelete;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    protected async Task<Guid> CreatePosition(params Guid[] departmentIds)
    {
        return await ExecuteInDb(async db =>
        {
            var positionName = PositionName.Create($"Position_{Guid.NewGuid()}").Value;
            var positionDescription = PositionDescription.Create("Test description").Value;

            var departmentIdObjects = departmentIds
                .Select(id => new DepartmentId(id))
                .ToList();

            var positionResult = Position.Create(positionName, positionDescription, departmentIdObjects);
            var position = positionResult.Value;

            db.Positions.Add(position);
            await db.SaveChangesAsync();

            return position.Id.Value;
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

    protected async Task RunCleanupAsync(int thresholdDays = 30)
    {
        var scope = Services.CreateAsyncScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        var service = new DepartmentsCleanupBackgroundService(
            connectionFactory,
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CleanupOptions>>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DepartmentsCleanupBackgroundService>>());

        await service.RunCleanupForTestsAsync(thresholdDays, CancellationToken.None);
    }

    protected async Task DeactivateDepartmentDaysAgo(Guid departmentId, int daysAgo)
    {
        var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();

        await handler.Handle(new SoftDeleteDepartmentCommand(departmentId), CancellationToken.None);

        await ExecuteInDb(async db =>
        {
            var deletedAt = DateTime.UtcNow.AddDays(-daysAgo).AddMinutes(1);
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE departments SET deleted_at = {0} WHERE department_id = {1}",
                deletedAt, departmentId);
        });
    }

}