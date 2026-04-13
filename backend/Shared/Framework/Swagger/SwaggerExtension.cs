using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Shared.SharedKernel;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Framework.Swagger;

public static class SwaggerExtension
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        string title,
        string version = "v1")
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
            });

            options.SchemaFilter<EnvelopeErrorsSchemaFilter>();
        });

        return services;
    }

    public static WebApplication UseSwaggerConfiguration(
        this WebApplication app,
        string swaggerName,
        string version = "v1")
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", swaggerName);
        });

        app.MapGet("/", () => Results.Redirect("/swagger"));

        return app;
    }

    private class EnvelopeErrorsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Envelope<Errors>))
            {
                if (schema.Properties.TryGetValue("errors", out var errorsProperty))
                {
                    errorsProperty.Items = new OpenApiSchema
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = nameof(Errors),
                        },
                    };
                }
            }
        }
    }
}