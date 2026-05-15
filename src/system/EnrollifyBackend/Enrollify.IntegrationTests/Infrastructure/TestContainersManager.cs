using Enrollify.DatabaseMigration;
using Testcontainers.Azurite;
using Testcontainers.MsSql;

namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// Manages shared testcontainers for MsSQL and Azurite across all integration tests.
/// Implements singleton pattern to ensure containers are started once per test run.
/// </summary>
public class TestContainersManager : IAsyncLifetime
{
    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private static TestContainersManager? _instance;
    private static bool _isInitialized;

    private string DatabaseName { get; set; } = $"EnrollifyIntegrationTest_{Guid.NewGuid():N}";
    
    public MsSqlContainer SqlDbContainer { get; private set; } = null!;
    public AzuriteContainer AzuriteContainer { get; private set; } = null!;
    
    public string SqlConnectionString { get; private set; } = null!;
    public string AzuriteBlobConnectionString { get; private set; } = null!;

    private TestContainersManager()
    {
    }

    /// <summary>
    /// Gets or creates the singleton instance of TestContainersManager.
    /// </summary>
    public static async Task<TestContainersManager> GetInstanceAsync()
    {
        if (_instance != null && _isInitialized)
            return _instance;

        await _initLock.WaitAsync();
        try
        {
            if (_instance == null)
            {
                _instance = new TestContainersManager();
            }

            if (!_isInitialized)
            {
                await _instance.InitializeAsync();
                _isInitialized = true;
            }

            return _instance;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("[TestContainers] Starting MsSQL and Azurite containers...");

        // Start MsSQL Container
        SqlDbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        // Start Azurite Container for Azure Blob Storage emulation
        AzuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .Build();

        await Task.WhenAll(
            SqlDbContainer.StartAsync(),
            AzuriteContainer.StartAsync()
        );

        Console.WriteLine("[TestContainers] Containers started successfully.");

        // Setup connection strings
        await SetupSqlDatabase();
        SetupAzuriteConnectionString();

        // Run database migrations with seed and mock data
        await RunDatabaseMigrations();

        Console.WriteLine("[TestContainers] Database migrations completed.");
    }

    private async Task SetupSqlDatabase()
    {
        // Create a unique database for this test run
        // https://github.com/testcontainers/testcontainers-dotnet/issues/541#issuecomment-1925329033
        await SqlDbContainer.ExecScriptAsync(
            $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{DatabaseName}') CREATE DATABASE [{DatabaseName}]"
        );

        SqlConnectionString = SqlDbContainer
            .GetConnectionString()
            .Replace("Database=master", $"Database={DatabaseName}");

        Console.WriteLine($"[TestContainers] SQL Database created: {DatabaseName}");
    }

    private void SetupAzuriteConnectionString()
    {
        // Azurite connection string format
        AzuriteBlobConnectionString = AzuriteContainer.GetConnectionString();
        Console.WriteLine("[TestContainers] Azurite connection string configured.");
    }

    private async Task RunDatabaseMigrations()
    {
        Console.WriteLine("[TestContainers] Running database migrations (schema + seed + mock data)...");

        await Task.Run(() =>
        {
            DatabaseUpgrader.RunDbUpgradeActivities(
                connectionString: SqlConnectionString,
                forceEnsureDatabase: true,
                createMockData: true,  // Include mock data as per requirements
                createSeedData: true   // Include seed data
            );
        });
    }

    public async Task DisposeAsync()
    {
        Console.WriteLine("[TestContainers] Disposing containers...");

        if (SqlDbContainer != null)
            await SqlDbContainer.DisposeAsync();

        if (AzuriteContainer != null)
            await AzuriteContainer.DisposeAsync();

        Console.WriteLine("[TestContainers] Containers disposed.");
    }

    /// <summary>
    /// Resets the singleton instance. Use with caution - primarily for testing the manager itself.
    /// </summary>
    public static async Task ResetAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (_instance != null)
            {
                await _instance.DisposeAsync();
                _instance = null;
                _isInitialized = false;
            }
        }
        finally
        {
            _initLock.Release();
        }
    }
}
