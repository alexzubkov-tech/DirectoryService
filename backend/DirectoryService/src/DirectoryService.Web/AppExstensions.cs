using Framework.Middlewares;
using Serilog;

namespace DirectoryService.Web;

public static class AppExstensions
{
    public static IApplicationBuilder UseWebConfiguration(this WebApplication app)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Directory Service V1");
        });

        app.MapGet("/", () => Results.Redirect("/swagger"));

        app.MapControllers();

        return app;

    }
}