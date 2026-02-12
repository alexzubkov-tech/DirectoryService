using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<DirectoryServiceDbContext>((sp, options) =>
        { 
            string? connectionstring = configuration.GetConnectionString(Constants.DATABASE);

            IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            
            options.UseNpgsql(connectionstring);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            
            options.UseLoggerFactory(loggerFactory);
        });
        
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        
        return services;
    }
}