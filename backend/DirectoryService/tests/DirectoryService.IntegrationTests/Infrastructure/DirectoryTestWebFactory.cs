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
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithCleanUp(true)
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;
    private IConnectionMultiplexer _redisConnection = null!;

    public string DbConnectionString => _dbContainer.GetConnectionString();
    public string RedisConnectionString => _redisContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DirectoryServiceDbContext>();
            services.RemoveAll<IReadDbContext>();
            services.RemoveAll<IDbConnectionFactory>();
            services.RemoveAll<IConnectionMultiplexer>();

            services.AddScoped(_ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DirectoryServiceDbContext>();
                optionsBuilder.UseNpgsql(DbConnectionString);
                return new DirectoryServiceDbContext(optionsBuilder.Options);
            });

            services.AddScoped<IReadDbContext>(sp =>
                sp.GetRequiredService<DirectoryServiceDbContext>());

            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgSlqConnectionFactory(DbConnectionString));

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var configuration = ConfigurationOptions.Parse(RedisConnectionString);
                configuration.AbortOnConnectFail = false;
                configuration.ConnectTimeout = 10000;
                configuration.SyncTimeout = 10000;
                configuration.AllowAdmin = true;
                return ConnectionMultiplexer.Connect(configuration);
            });

            // ЯВНАЯ регистрация распределённого кэша
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = RedisConnectionString;
                options.InstanceName = "DirectoryService-Test";
            });
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            var postgresTask = _dbContainer.StartAsync();
            var redisTask = _redisContainer.StartAsync();
            await Task.WhenAll(postgresTask, redisTask);

            _redisConnection = ConnectionMultiplexer.Connect(RedisConnectionString);

            var optionsBuilder = new DbContextOptionsBuilder<DirectoryServiceDbContext>();
            optionsBuilder.UseNpgsql(DbConnectionString);

            await using (var dbContext = new DirectoryServiceDbContext(optionsBuilder.Options))
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            _dbConnection = new NpgsqlConnection(DbConnectionString);
            await _dbConnection.OpenAsync();

            await InitializeRespawner();

            _ = CreateClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== CONTAINER INIT EXCEPTION ===");
            Console.WriteLine(ex);
            throw;
        }
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        _redisConnection?.Dispose();
        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner != null && _dbConnection != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
        await ResetRedisAsync();
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

    private async Task ResetRedisAsync()
    {
        if (_redisConnection == null || !_redisConnection.IsConnected)
            return;

        foreach (var endpoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endpoint);
            var database = _redisConnection.GetDatabase();
            var keys = server.Keys().ToArray();
            foreach (var key in keys)
            {
                await database.KeyDeleteAsync(key);
            }
        }
    }
}