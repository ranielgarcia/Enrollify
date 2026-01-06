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

    public SqlConnectionFactory(string connectionString)
    {
        // Early validation
        _connectionString = new SqlConnectionStringBuilder(connectionString).ConnectionString;
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
        var retryPolicy = Policy
            .Handle<SqlException>(ex => IsTransient(ex))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    // Optional: Add logging here if needed
                });

        return await retryPolicy.ExecuteAsync(async token =>
        {
            var conn = new SqlConnection(_connectionString);
            try
            {
                await conn.OpenAsync(token).ConfigureAwait(false);
                return conn;
            }
            catch
            {
                await conn.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }, ct).ConfigureAwait(false);
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
