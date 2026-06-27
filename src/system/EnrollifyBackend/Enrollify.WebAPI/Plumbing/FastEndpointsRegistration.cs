using FastEndpoints.Swagger;
using System.Text.Json;

namespace Enrollify.WebAPI.Plumbing;

public static class FastEndpointsRegistration
{
    public static IServiceCollection AddFastEndpointsConfigs(
       this IServiceCollection services)
    {
        services.AddFastEndpoints()
            .SwaggerDocument(o =>
            {
                o.UsePropertyNamingPolicy = false;
                o.DocumentSettings = s =>
                {
                    s.Title = "Laundro API";
                    s.Version = "v1";

                    // Map Vogen value objects to their underlying primitive types in OpenAPI
                    s.SchemaSettings.SchemaProcessors.Add(new VogenNSwagSchemaProcessor());
                };
            });
        return services;
    }

    public static IApplicationBuilder UseFastEndpointsConfigs(this IApplicationBuilder app)
    {
        //app.UseDefaultExceptionHandler(); // Let's use the global exception handlers in Infrastructure dir for now
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.RoutePrefix = "api";
            c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            c.Serializer.Options.WriteIndented = true;
            c.Errors.UseProblemDetails(
              x =>
              {
                x.AllowDuplicateErrors = true;  //allows duplicate errors for the same error name
                x.IndicateErrorCode = true;     //serializes the fluentvalidation error code
                x.IndicateErrorSeverity = true; //serializes the fluentvalidation error severity
                x.TypeValue = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1";
                x.TitleValue = "One or more validation errors occurred.";
                x.TitleTransformer = pd => pd.Status switch
                {
                  400 => "Validation Error",
                  404 => "Not Found",
                  _ => "One or more errors occurred!"
                };
              });
        }).UseSwaggerGen();

        return app;
    }
}
