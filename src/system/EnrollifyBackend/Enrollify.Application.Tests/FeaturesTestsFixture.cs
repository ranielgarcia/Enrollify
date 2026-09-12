using Enrollify.DatabaseMigration;
using Enrollify.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Enrollify.Application.Tests;

public class FeaturesTestsFixture : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

    private string DatabaseName { get; set; } = $"EnrollifyTest_{Guid.NewGuid():N}";
    public MsSqlContainer SqlDbContainer { get; private set; } = null!;
    public EnrollifyDbContext DbContext { get; private set; } = null!;
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        SqlDbContainer = new MsSqlBuilder(SqlServerImage)
            .Build();

        await SqlDbContainer.StartAsync();
        await SetupServices();
        await RunDbMigration();
    }

    public async Task SetupServices()
    {
        var sqlDbConnectionString = await GetSqlDbConnectionString();
        ServiceProvider = TestsServiceProvider.GetNewServiceProvider(sqlDbConnectionString);
        DbContext = ServiceProvider.GetRequiredService<EnrollifyDbContext>();
    }

    public async Task RunDbMigration()
    {
        var connectionString = await GetSqlDbConnectionString();
        DatabaseUpgrader.RunDbUpgradeActivities(connectionString, forceEnsureDatabase: true, createMockData: false, createSeedData: true);
    }

    public async Task<string> GetSqlDbConnectionString()
    {
        // https://github.com/testcontainers/testcontainers-dotnet/issues/541#issuecomment-1925329033
        await SqlDbContainer.ExecScriptAsync($"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{DatabaseName}') CREATE DATABASE [{DatabaseName}]");

        string connectionString =
            SqlDbContainer
                .GetConnectionString()
                .Replace("Database=master", $"Database={DatabaseName}");
        return connectionString;
    }

    public async Task DisposeAsync()
    {
        await SqlDbContainer.DisposeAsync();
        await DbContext.DisposeAsync();
    }
}
