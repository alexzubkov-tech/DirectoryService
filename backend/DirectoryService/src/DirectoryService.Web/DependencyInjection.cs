using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Framework.Logging;
using Framework.Swagger;

namespace DirectoryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddSerilogLogging(configuration, "DirectoryService")
            .AddWebDependencies()
            .AddApplication(configuration)
            .AddInfrastructurePostgres(configuration);

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerConfiguration("Directory Service API");

        return services;
    }
}