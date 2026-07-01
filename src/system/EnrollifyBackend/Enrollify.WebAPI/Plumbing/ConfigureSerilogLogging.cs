using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;

namespace Enrollify.WebAPI.Plumbing;

// I would only bring Serilog back if you later need capabilities such as:
//
// Writing to Seq
// Rolling file logs
// Elasticsearch, Loki, Splunk, etc.
// Advanced enrichers
// JSON log formatting
// Complex object destructuring
// Specialized filtering
public static class ConfigureSerilogLogging
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
                    .WriteTo.Debug(), writeToProviders: true);

        return services;
    }

    public static IApplicationBuilder UseSerilogLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}
