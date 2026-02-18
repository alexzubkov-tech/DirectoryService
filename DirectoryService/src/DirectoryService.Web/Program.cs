using DirectoryService.Application;
using DirectoryService.Application.Locations.Fails;
using DirectoryService.Infrastructure;
using DirectoryService.Presenters;
using DirectoryService.Web;
using DirectoryService.Web.Middlewares;
using Microsoft.OpenApi.Models;
using Shared;
using Errors = DirectoryService.Application.Locations.Fails.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependencies(builder.Configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
        {
            if (schema.Properties.TryGetValue("errors", out var errorsProp))
            {
                errorsProp.Items.Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.Schema,
                    Id = "Error",
                };
            }
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DirectoryService.Web",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseExceptionMiddleware();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.MapOpenApi();

app.Run();