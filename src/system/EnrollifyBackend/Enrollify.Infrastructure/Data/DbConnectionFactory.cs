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
        var conn = new SqlConnection(_connectionString);

        await Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(200))
            .ExecuteAsync(() => conn.OpenAsync(ct))
            .ConfigureAwait(false);

        return conn;
    }
}
