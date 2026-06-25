using Enrollify.WebAPI.Authentication;
using Enrollify.WebAPI.Infrastructure.Exceptions;
using Enrollify.WebAPI.Plumbing;
using Enrollify.WebAPI.StartupServices;
using Serilog;

Log.Logger = ConfigureSerilogLogging.BootstrapLogger;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddSerilogLogging(builder.Configuration);

    // Currently remove App Insights logging
    // Due the following:
    // Root cause: Microsoft.ApplicationInsights.AspNetCore 3.x is a complete rewrite that uses OpenTelemetry under the hood.
    // Calling AddApplicationInsightsTelemetry() now registers Azure.Monitor.OpenTelemetry.Exporter,
    // which unconditionally requires an Application Insights connection string — even locally.

    // Exception handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddFastEndpointsConfigs();
    builder.Services.AddGlobalCorsPolicy(builder.Configuration);

    // Get ILogger instance before adding infrastructure services
    using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
    var startupLogger = loggerFactory.CreateLogger<Program>();

    startupLogger.LogInformation("Starting web host");


    builder.Services.AddAzureADAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddServiceConfigs(startupLogger, builder);
    builder.Services.AddStartupServices(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.MapDefaultEndpoints();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogLogging();
    app.UseExceptionHandler();
    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseGlobalCorsPolicy();
    app.UseAzureADAuthentication();
    //app.MapControllers();
    app.UseFastEndpointsConfigs();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}


// Make Program accessible for integration tests (WebApplicationFactory)
public partial class Program { }
