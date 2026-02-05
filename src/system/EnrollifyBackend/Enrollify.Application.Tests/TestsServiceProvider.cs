using Enrollify.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit.Abstractions;

namespace Enrollify.Application.Tests;

/// <summary>
/// Marker class used as a category for FakeLogger since static types cannot be used as type arguments.
/// </summary>
public class TestLoggerCategory;

public static class TestsServiceProvider
{
    public static IServiceProvider GetNewServiceProvider(string dbConnectionString)
    {
        var services = new ServiceCollection();
        services.AddApplicationServices(dbConnectionString);
        return services.BuildServiceProvider();
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string dbConnectionString)
    {
        var initialData = new Dictionary<string, string?>()
        {
            { "ConnectionStrings:DefaultConnection", dbConnectionString },
        };

        var configurationBuilder = new ConfigurationManager();
        configurationBuilder.AddInMemoryCollection(initialData);

        var logger = GetNewFakeLogger();
        services.AddInfrastructureServices(configurationBuilder, logger);

        return services;
    }

    public static FakeLogger<TestLoggerCategory> GetNewFakeLogger()
    {
        var options = new FakeLogCollectorOptions();
        options.FilteredLevels.Add(LogLevel.Error);
        options.FilteredLevels.Add(LogLevel.Warning);
        var collection = FakeLogCollector.Create(options);
        var fakeLogger = new FakeLogger<TestLoggerCategory>(collection);
        return fakeLogger;
    }

    public static FakeLogger<T> GetNewFakeLogger<T>(ITestOutputHelper output) where T : class
    {
        var options = new FakeLogCollectorOptions()
        {
            //We can override disabled log levels and collect them
            //CollectRecordsForDisabledLogLevels = true,
            //Write the log messages to console
            OutputSink = output.WriteLine
        };
        //Filter to certain levels for validation
        options.FilteredLevels.Add(LogLevel.Error);
        options.FilteredLevels.Add(LogLevel.Warning);
        var collection = FakeLogCollector.Create(options);
        var fakeLogger = new FakeLogger<T>(collection);
        return fakeLogger;
    }
}
