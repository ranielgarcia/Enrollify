using Microsoft.Data.SqlClient;
using Polly;

namespace Enrollify.Infrastructure.Data;

public interface IDbConnectionFactory
{
    SqlConnection Create();
    SqlConnection CreateOpen();
    Task<SqlConnection> CreateOpenAsync(CancellationToken ct = default);
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly AsyncPolicy _retryPolicy;

    public SqlConnectionFactory(IConfiguration configuration)
    {
      string? connectionString = configuration.GetConnectionString("cleanarchitecture")
                                ?? configuration.GetConnectionString("DefaultConnection")
                                ?? configuration.GetConnectionString("SqliteConnection");
      Guard.Against.Null(connectionString);
        // Early validation and ensure reasonable connection timeout
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (builder.ConnectTimeout < 30)
        {
            builder.ConnectTimeout = 30; // Ensure at least 30 seconds timeout
        }
        _connectionString = builder.ConnectionString;

        // Create retry policy once for reuse
        _retryPolicy = Policy
            .Handle<SqlException>(IsTransient)
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt - 1)));
    }

    public SqlConnection Create()
        => new SqlConnection(_connectionString);

    public SqlConnection CreateOpen()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public async Task<SqlConnection> CreateOpenAsync(CancellationToken ct = default)
    {
        // Fast path: check for cancellation before any work
        ct.ThrowIfCancellationRequested();

        SqlConnection? conn = null;
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();

                // Dispose previous connection attempt if retrying
                if (conn != null)
                {
                    await conn.DisposeAsync().ConfigureAwait(false);
                }

                conn = new SqlConnection(_connectionString);

                // Use a linked token with its own timeout to distinguish
                // between user cancellation and connection timeout
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                try
                {
                    await conn.OpenAsync(linkedCts.Token).ConfigureAwait(false);
                    return conn;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // User requested cancellation - don't retry, just throw
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    // Connection timeout - throw TimeoutException for retry logic
                    throw new TimeoutException("Connection timeout while opening database connection");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception) when (conn != null)
        {
            await conn.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static bool IsTransient(SqlException ex)
    {
        // Transient error numbers from SQL Server
        int[] transientErrorNumbers =
        [
            -2,     // Timeout
            20,     // Instance does not support encryption
            64,     // Connection was successfully established but login failed
            233,    // Connection initialization error
            10053,  // Transport-level error
            10054,  // Transport-level error
            10060,  // Network or instance-specific error
            10928,  // Resource ID limit reached
            10929,  // Resource ID limit reached
            40143,  // Connection could not be initialized
            40197,  // Service error processing request
            40501,  // Service is busy
            40613,  // Database not currently available
            49918,  // Not enough resources to process request
            49919,  // Too many create/update requests
            49920   // Too many requests
        ];

        return transientErrorNumbers.Contains(ex.Number);
    }
}
