using Enrollify.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// Test fixture for WebAPI integration tests.
/// Uses WebApplicationFactory to host the API with testcontainers.
/// Each test class gets its own isolated database with fresh migrations.
/// </summary>
public class WebApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private TestContainersManager _containersManager = null!;
    private string _sqlConnectionString = null!;

    public EnrollifyDbContext DbContext { get; private set; } = null!;
    public string SqlConnectionString => _sqlConnectionString;
    public string AzuriteBlobConnectionString => _containersManager.AzuriteBlobConnectionString;

    async Task IAsyncLifetime.InitializeAsync()
    {
        // Get shared testcontainers instance
        _containersManager = await TestContainersManager.GetInstanceAsync();
        
        // Create a unique database for this test class
        _sqlConnectionString = await _containersManager.CreateDatabaseAsync();
        
        // Create a scope to get DbContext for test setup/verification
        var scope = Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<EnrollifyDbContext>();
        
        Console.WriteLine("[WebApiTestFixture] Initialized with isolated database.");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (DbContext != null)
            await DbContext.DisposeAsync();
        Console.WriteLine("[WebApiTestFixture] Disposed.");
        // Note: We don't dispose _containersManager here as it's shared across all tests
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear existing configuration
            config.Sources.Clear();

            // Add test configuration with testcontainer settings
            var testConfig = TestConfigurationBuilder.BuildForWebApi(
                _sqlConnectionString,
                _containersManager.AzuriteBlobConnectionString
            );

            config.AddConfiguration(testConfig);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<EnrollifyDbContext>>();
            services.RemoveAll<EnrollifyDbContext>();

            // Re-register DbContext with testcontainer connection string
            services.AddDbContext<EnrollifyDbContext>(options =>
            {
                options.UseSqlServer(_sqlConnectionString);
                options.EnableSensitiveDataLogging();
            });

            // Override Wolverine's real IDbContextOutbox with a no-op stub so events
            // published during test handler execution are silently discarded and the
            // test server does not need Wolverine's SQL Server outbox tables at startup.
            // The last registration wins for GetRequiredService<IDbContextOutbox>().
            services.AddScoped<IDbContextOutbox, TestDbContextOutbox>();

            // TODO: Configure test authentication if needed
            // For now, tests will need to handle authentication bypass or use test auth handlers
        });

        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// Creates an HttpClient for making requests to the test server.
    /// </summary>
    public HttpClient CreateTestClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Creates a new service scope for accessing services with proper lifetime management.
    /// Useful for database operations that need to be isolated from the test scope.
    /// </summary>
    public IServiceScope CreateServiceScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// Executes an action within a new service scope and disposes it automatically.
    /// </summary>
    public async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = CreateServiceScope();
        await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Executes an action within a new service scope and returns a result.
    /// </summary>
    public async Task<T> ExecuteInScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = CreateServiceScope();
        return await action(scope.ServiceProvider);
    }
}
