using System.Data.Common;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DirectoryServiceDbContext>();
            services.RemoveAll<IReadDbContext>();
            services.RemoveAll<IDbConnectionFactory>();

            services.AddScoped(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DirectoryServiceDbContext>();
                optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString());
                return new DirectoryServiceDbContext(optionsBuilder.Options);
            });

            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<DirectoryServiceDbContext>());

            services.AddSingleton<IDbConnectionFactory>(sp =>
                new NpgSlqConnectionFactory(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _dbContainer.StartAsync();

            var optionsBuilder = new DbContextOptionsBuilder<DirectoryServiceDbContext>();
            optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString());
            await using var dbContext = new DirectoryServiceDbContext(optionsBuilder.Options);
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _dbConnection.OpenAsync();

            await InitializeRespawner();
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== CONTAINER INIT EXCEPTION ===");
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }
}