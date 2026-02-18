using DirectoryService.Application;
using DirectoryService.Infrastructure;

namespace DirectoryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddWebDependencies()
            .AddApplication()
            .AddInfrastructurePostgres(configuration);

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        //services.AddOpenApi();

        return services;
    }
}