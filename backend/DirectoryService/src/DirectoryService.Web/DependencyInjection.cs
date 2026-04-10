using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Web.Configuration.Swagger;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddSerilogLogging(configuration)
            .AddWebDependencies()
            .AddApplication(configuration)
            .AddInfrastructurePostgres(configuration);

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Directory Service API",
                Version = "v1",
            });
            options.SchemaFilter<EnvelopeErrorsSchemaFilter>();
        });

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DepartmentService"));

        return services;
    }
}