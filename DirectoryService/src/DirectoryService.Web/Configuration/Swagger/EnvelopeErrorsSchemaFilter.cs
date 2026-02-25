using DirectoryService.Presenters;
using Microsoft.OpenApi.Models;
using Shared;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DirectoryService.Web.Configuration.Swagger;

public class EnvelopeErrorsSchemaFilter : ISchemaFilter
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