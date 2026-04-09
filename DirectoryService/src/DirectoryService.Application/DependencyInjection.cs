using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Common.Options;
using DirectoryService.Application.ReferenceValidation;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IReferenceValidator, ReferenceValidator>();

        var assembly = typeof(DependencyInjection).Assembly;

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(IQueryHandler<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        // Регистрация Options Pattern
        services.Configure<RedisOptions>(options =>
            configuration.GetSection("RedisOptions").Bind(options));

        services.Configure<CacheOptions>(options =>
            configuration.GetSection("CacheOptions").Bind(options));

        // Redis
        services.AddStackExchangeRedisCache(setup =>
        {
            var redisOptions = new RedisOptions();
            configuration.GetSection("RedisOptions").Bind(redisOptions);

            setup.Configuration = redisOptions.Configuration;
        });

        // HybridCache
        services.AddHybridCache(options =>
        {
            var cacheOptions = new CacheOptions();
            configuration.GetSection("CacheOptions").Bind(cacheOptions);

            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(cacheOptions.LocalCacheExpirationMinutes),
                Expiration = TimeSpan.FromMinutes(cacheOptions.DistributedCacheExpirationMinutes),
            };
        });

        return services;
    }
}