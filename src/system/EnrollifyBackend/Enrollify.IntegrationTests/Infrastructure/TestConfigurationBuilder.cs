using Microsoft.Extensions.Configuration;

namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// Builds IConfiguration for integration tests with testcontainer settings.
/// </summary>
public static class TestConfigurationBuilder
{
    /// <summary>
    /// Builds a test configuration with testcontainer connection strings and storage settings.
    /// </summary>
    /// <param name="sqlConnectionString">SQL Server connection string from testcontainer.</param>
    /// <param name="azuriteBlobConnectionString">Azurite blob storage connection string.</param>
    /// <returns>IConfiguration instance for test environment.</returns>
    public static IConfiguration Build(string sqlConnectionString, string azuriteBlobConnectionString)
    {
        var configurationBuilder = new ConfigurationBuilder();

        // Add in-memory configuration with testcontainer values
        var inMemorySettings = new Dictionary<string, string?>
        {
            // Database connection strings
            ["ConnectionStrings:DefaultConnection"] = sqlConnectionString,
            ["ConnectionStrings:cleanarchitecture"] = sqlConnectionString,
            
            // Azure Storage (Azurite) settings
            ["Storage:ConnectionString"] = azuriteBlobConnectionString,
            ["Storage:UseAzureCredential"] = "false",
            
            // Disable authentication for tests (can be overridden per test)
            ["Authentication:Disabled"] = "true",
            
            // Logging settings for tests
            ["Logging:LogLevel:Default"] = "Information",
            ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
            ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
        };

        configurationBuilder.AddInMemoryCollection(inMemorySettings);

        return configurationBuilder.Build();
    }

    /// <summary>
    /// Builds a minimal test configuration for application layer tests (no WebAPI settings).
    /// </summary>
    public static IConfiguration BuildForApplicationLayer(string sqlConnectionString, string azuriteBlobConnectionString)
    {
        return Build(sqlConnectionString, azuriteBlobConnectionString);
    }

    /// <summary>
    /// Builds configuration for WebAPI tests with additional web-specific settings.
    /// </summary>
    public static IConfiguration BuildForWebApi(string sqlConnectionString, string azuriteBlobConnectionString)
    {
        var configurationBuilder = new ConfigurationBuilder();

        var inMemorySettings = new Dictionary<string, string?>
        {
            // Database connection strings
            ["ConnectionStrings:DefaultConnection"] = sqlConnectionString,
            ["ConnectionStrings:cleanarchitecture"] = sqlConnectionString,
            
            // Azure Storage (Azurite) settings
            ["Storage:ConnectionString"] = azuriteBlobConnectionString,
            ["Storage:UseAzureCredential"] = "false",
            
            // Disable Azure AD authentication for tests
            ["AzureAd:Enabled"] = "false",
            
            // CORS settings for tests
            ["Cors:AllowedOrigins:0"] = "http://localhost",
            ["Cors:AllowedOrigins:1"] = "https://localhost",
            
            // Environment
            ["Environment"] = "Test",
            
            // Logging
            ["Logging:LogLevel:Default"] = "Information",
            ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
            ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
        };

        configurationBuilder.AddInMemoryCollection(inMemorySettings);

        return configurationBuilder.Build();
    }
}
