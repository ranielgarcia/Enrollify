using Enrollify.Infrastructure;
using Enrollify.WebAPI.Infrastructure.Exceptions;
using Enrollify.WebAPI.Plumbing;
using Serilog;

Log.Logger = ConfigureLogging.BootstrapLogger;

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

    builder.Services.AddGlobalCorsPolicy(builder.Configuration);

    // Get ILogger instance before adding infrastructure services
    using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
    var startupLogger = loggerFactory.CreateLogger<Program>();

    startupLogger.LogInformation("Starting web host");

    builder.Services.AddInfrastructureServices(builder.Configuration, startupLogger);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

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


