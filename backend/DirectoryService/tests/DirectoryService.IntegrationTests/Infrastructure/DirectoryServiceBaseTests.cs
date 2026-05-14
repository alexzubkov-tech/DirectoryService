using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Commands.Create;
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
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace DirectoryService.IntegrationTests.Infrastructure;

public abstract class DirectoryServiceBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly DirectoryTestWebFactory _factory;
    private readonly Func<Task> _resetDatabase;

    protected IServiceProvider Services => _factory.Services;

    protected DirectoryServiceBaseTests(DirectoryTestWebFactory factory)
    {
        _factory = factory;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        await action(dbContext);
    }

    protected async Task<LocationId> CreateLocation(
        string name,
        string country = "Россия",
        string city = "Москва",
        string street = "Ленина",
        string buildingNumber = "1",
        string timeZone = "Europe/Moscow")
    {
        return await ExecuteInDb(async db =>
        {
            var locationNameResult = LocationName.Create(name);
            if (locationNameResult.IsFailure)
                throw new InvalidOperationException($"Failed to create location name: {locationNameResult.Error}");

            var locationAddressResult = LocationAddress.Create(country, city, street, buildingNumber);
            if (locationAddressResult.IsFailure)
                throw new InvalidOperationException($"Failed to create location address: {locationAddressResult.Error}");

            var locationTimeZoneResult = LocationTimeZone.Create(timeZone);
            if (locationTimeZoneResult.IsFailure)
                throw new InvalidOperationException($"Failed to create timezone: {locationTimeZoneResult.Error}");

            var locationResult = Location.Create(
                locationNameResult.Value,
                locationAddressResult.Value,
                locationTimeZoneResult.Value);

            if (locationResult.IsFailure)
                throw new InvalidOperationException($"Failed to create location: {locationResult.Error}");

            var location = locationResult.Value;
            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return location.Id;
        });
    }

    /// <summary>
    /// Создаёт департамент НАПРЯМУЮ в БД (без инвалидации кэша).
    /// </summary>
    protected async Task<Guid> CreateDepartment(
        string name,
        string identifier,
        Guid? parentId = null,
        List<Guid>? locationIds = null)
    {
        locationIds ??= [(await CreateLocation($"Location-{name}")).Value];

        return await ExecuteInDb(async db =>
        {
            var departmentNameResult = DepartmentName.Create(name);
            if (departmentNameResult.IsFailure)
                throw new InvalidOperationException($"Failed to create department name: {departmentNameResult.Error}");

            var departmentIdentifierResult = DepartmentIdentifier.Create(identifier);
            if (departmentIdentifierResult.IsFailure)
                throw new InvalidOperationException($"Failed to create department identifier: {departmentIdentifierResult.Error}");

            var locationIdObjects = locationIds
                .Select(id => new LocationId(id))
                .ToList();

            Result<Department, Error> departmentResult;

            if (parentId is null)
            {
                departmentResult = Department.CreateParent(
                    departmentNameResult.Value,
                    departmentIdentifierResult.Value,
                    locationIdObjects);
            }
            else
            {
                var parent = await db.Departments
                    .FirstOrDefaultAsync(d => d.Id == new DepartmentId(parentId.Value));

                if (parent == null)
                    throw new InvalidOperationException($"Parent department {parentId} not found");

                departmentResult = Department.CreateChild(
                    departmentNameResult.Value,
                    departmentIdentifierResult.Value,
                    parent,
                    locationIdObjects);
            }

            if (departmentResult.IsFailure)
                throw new InvalidOperationException($"Failed to create department: {departmentResult.Error}");

            var department = departmentResult.Value;
            db.Departments.Add(department);
            await db.SaveChangesAsync();

            return department.Id.Value;
        });
    }

    /// <summary>
    /// Создаёт департамент ЧЕРЕЗ ХЕНДЛЕР (с инвалидацией кэша).
    /// </summary>
    protected async Task<Guid> CreateDepartmentViaHandler(
        string name,
        string identifier,
        Guid? parentId = null,
        List<Guid>? locationIds = null)
    {
        locationIds ??= [(await CreateLocation($"Location-{name}")).Value];

        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        var request = new CreateDepartmentRequest(name, identifier, parentId, locationIds);

        var result = await handler.Handle(new CreateDepartmentCommand(request), CancellationToken.None);

        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create department via handler: {result.Error}");

        return result.Value;
    }

    protected async Task<Guid> CreatePosition(params Guid[] departmentIds)
    {
        return await ExecuteInDb(async db =>
        {
            var positionNameResult = PositionName.Create($"Position-{Guid.NewGuid():N}");
            if (positionNameResult.IsFailure)
                throw new InvalidOperationException($"Failed to create position name: {positionNameResult.Error}");

            var positionDescriptionResult = PositionDescription.Create("Test description");
            if (positionDescriptionResult.IsFailure)
                throw new InvalidOperationException($"Failed to create position description: {positionDescriptionResult.Error}");

            var departmentIdObjects = departmentIds
                .Select(id => new DepartmentId(id))
                .ToList();

            var positionResult = Position.Create(
                positionNameResult.Value,
                positionDescriptionResult.Value,
                departmentIdObjects);

            if (positionResult.IsFailure)
                throw new InvalidOperationException($"Failed to create position: {positionResult.Error}");

            var position = positionResult.Value;
            db.Positions.Add(position);
            await db.SaveChangesAsync();

            return position.Id.Value;
        });
    }

    protected async Task DeactivateDepartment(Guid departmentId)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();

        await handler.Handle(new SoftDeleteDepartmentCommand(departmentId), CancellationToken.None);
    }

    protected async Task DeactivateLocation(LocationId locationId)
    {
        await ExecuteInDb(async db =>
        {
            var location = await db.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
            if (location != null)
            {
                location.Deactivate();
                await db.SaveChangesAsync();
            }
        });
    }

    protected async Task RunCleanupAsync(int thresholdDays = 30)
    {
        await using var scope = Services.CreateAsyncScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        var service = new DepartmentsCleanupBackgroundService(
            connectionFactory,
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<DepartmentCleanupOptions>>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DepartmentsCleanupBackgroundService>>(),
            scope.ServiceProvider.GetRequiredService<HybridCache>());

        await service.RunCleanupForTestsAsync(thresholdDays, CancellationToken.None);
    }

    protected async Task DeactivateDepartmentDaysAgo(Guid departmentId, int daysAgo)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();

        await handler.Handle(new SoftDeleteDepartmentCommand(departmentId), CancellationToken.None);

        await ExecuteInDb(async db =>
        {
            var deletedAt = DateTime.UtcNow.AddDays(-daysAgo).AddMinutes(1);
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE departments SET deleted_at = {0} WHERE department_id = {1}",
                deletedAt,
                departmentId);
        });
    }
}