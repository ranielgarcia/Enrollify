using Enrollify.DatabaseMigration;
using Testcontainers.Azurite;
using Testcontainers.MsSql;

namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// Manages shared testcontainers for MsSQL and Azurite across all integration tests.
/// Implements singleton pattern to ensure containers are started once per test run.
/// Each test fixture can create its own isolated database with migrations.
/// </summary>
public class TestContainersManager : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string AzuriteImage = "mcr.microsoft.com/azure-storage/azurite:latest";

    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private static TestContainersManager? _instance;
    private static bool _isInitialized;
    
    public MsSqlContainer SqlDbContainer { get; private set; } = null!;
    public AzuriteContainer AzuriteContainer { get; private set; } = null!;
    
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
        SqlDbContainer = new MsSqlBuilder(SqlServerImage)
            .Build();

        // Start Azurite Container for Azure Blob Storage emulation
        AzuriteContainer = new AzuriteBuilder(AzuriteImage)
            .Build();

        await Task.WhenAll(
            SqlDbContainer.StartAsync(),
            AzuriteContainer.StartAsync()
        );

        Console.WriteLine("[TestContainers] Containers started successfully.");

        // Setup connection strings
        SetupAzuriteConnectionString();
    }

    /// <summary>
    /// Creates a new isolated database with migrations for a test fixture.
    /// Each test class (fixture instance) should call this to get its own database.
    /// </summary>
    /// <returns>Connection string for the newly created database</returns>
    public async Task<string> CreateDatabaseAsync()
    {
        var databaseName = $"EnrollifyTest_{Guid.NewGuid():N}";
        
        Console.WriteLine($"[TestContainers] Creating database: {databaseName}");

        // Create a unique database
        await SqlDbContainer.ExecScriptAsync(
            $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') CREATE DATABASE [{databaseName}]"
        );

        var connectionString = SqlDbContainer
            .GetConnectionString()
            .Replace("Database=master", $"Database={databaseName}");

        // Run database migrations with seed and mock data
        await RunDatabaseMigrations(connectionString, databaseName);

        Console.WriteLine($"[TestContainers] Database ready: {databaseName}");
        
        return connectionString;
    }

    private async Task RunDatabaseMigrations(string connectionString, string databaseName)
    {
        Console.WriteLine($"[TestContainers] Running migrations for {databaseName} (schema + seed + mock data)...");

        await Task.Run(() =>
        {
            DatabaseUpgrader.RunDbUpgradeActivities(
                connectionString: connectionString,
                forceEnsureDatabase: true,
                createMockData: true,  // Include mock data as per requirements
                createSeedData: true   // Include seed data
            );
        });

        Console.WriteLine($"[TestContainers] Migrations completed for {databaseName}");
    }

    private void SetupAzuriteConnectionString()
    {
        // Azurite connection string format
        AzuriteBlobConnectionString = AzuriteContainer.GetConnectionString();
        Console.WriteLine("[TestContainers] Azurite connection string configured.");
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
