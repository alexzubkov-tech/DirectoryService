using DirectoryService.Web.Middlewares;
using Serilog;

namespace DirectoryService.Web.Configuration;

public static class AppExstensions
{
    public static IApplicationBuilder UseWebConfiguration(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Directory Service V1");
        });

        app.MapControllers();

        return app;

    }
}