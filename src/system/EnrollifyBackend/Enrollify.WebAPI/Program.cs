using Enrollify.Infrastructure;
using Enrollify.WebAPI.Authentication;
using Enrollify.WebAPI.Infrastructure.Exceptions;
using Enrollify.WebAPI.Plumbing;
using Serilog;

Log.Logger = ConfigureSerilogLogging.BootstrapLogger;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddSerilogLogging(builder.Configuration);


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
    builder.Services.AddServiceConfigs(startupLogger, builder);

    var app = builder.Build();

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


