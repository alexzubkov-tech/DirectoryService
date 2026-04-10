namespace DirectoryService.Web.Middlewares;

public static class ExceptionMiddlewareExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this WebApplication app) => app.UseMiddleware<ExceptionMiddleware>();
}