using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;

namespace Enrollify.WebAPI.Plumbing;

public static class ConfigureLogging
{
    public static ReloadableLogger BootstrapLogger => new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console()
                .WriteTo.Debug()
                .CreateBootstrapLogger();

    // If you would like to see timing and dependency information in Seq,
    // SerilogTracing is a Serilog extension that supports both logs and traces.
    // https://github.com/datalust/serilog-sinks-seq?tab=readme-ov-file
    // https://github.com/serilog-tracing/serilog-tracing
    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services, IConfiguration configuration)
    {
        var levelSwitch = new LoggingLevelSwitch();

        services.AddSerilog((services, loggerConfiguration) =>
                loggerConfiguration
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "Enrollify.WebAPI")
                    .WriteTo.Console()
                    .WriteTo.Debug()
                    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces));

        return services;
    }

    public static IApplicationBuilder UseSerilogLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}
