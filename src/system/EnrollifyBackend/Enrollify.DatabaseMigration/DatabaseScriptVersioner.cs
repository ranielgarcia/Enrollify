using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Enrollify.DatabaseMigration;

public class DatabaseScriptVersioner
{
    private readonly string _connectionString;
    private string? _fileSystemVersion;

    public DatabaseScriptVersioner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool IsDatabaseAtCurrentScriptVersion()
    {
        var databaseVersion = GetCurrentScriptVersionHash();

        _fileSystemVersion = HashScriptResources();

        return _fileSystemVersion.Equals(databaseVersion);
    }

    private string? GetCurrentScriptVersionHash()
    {
        var currentHash = string.Empty;

        try
        {
            using var connection =
                new SqlConnection(new SqlConnectionStringBuilder(_connectionString)
                {
                    ConnectTimeout = 1 //Fail fast
                }.ConnectionString);

            connection.Open();

            using var command = new SqlCommand(@"
IF OBJECT_ID('ScriptVersion', 'U') IS NOT NULL
BEGIN
    SELECT [Hash] FROM ScriptVersion
END
ELSE
BEGIN
    SELECT '' AS [Hash]
END
", connection);

            using var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                return currentHash;
            }

            reader.Read();
            currentHash = reader["Hash"].ToString();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to retrieve script version hash");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        return currentHash;
    }

    public void UpdateScriptVersion()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        try
        {
            using var command = new SqlCommand("DELETE FROM ScriptVersion", connection);
            command.ExecuteNonQuery();

            command.CommandText = $"INSERT INTO ScriptVersion VALUES ('{_fileSystemVersion}')";
            command.ExecuteNonQuery();
        }
        catch(SqlException ex) when (IsInvalidObjectNameScriptVersion(ex))
        {
            using var command = new SqlCommand("CREATE TABLE ScriptVersion (Hash VARCHAR(64) NOT NULL)", connection);
            command.ExecuteNonQuery();

            command.CommandText = $"INSERT INTO ScriptVersion VALUES ('{_fileSystemVersion}')";
            command.ExecuteNonQuery();
        }
    }

    private bool IsInvalidObjectNameScriptVersion(SqlException ex) =>
        // SQL Server error 208 = Invalid object name
        ex.Number == 208 || ex.Message?.Contains("Invalid object name 'ScriptVersion'") == true;

    private static string HashScriptResources()
    {
        // if we make any change in the database project, get a new hash so the upgrader will run
        // This covers new SQL embedded resources as well as C# IScript files
        var assemblyPath = typeof(Program).Assembly.Location;

        using var hash = SHA256.Create();
        var bytes = File.ReadAllBytes(assemblyPath);
        var hashBytes = hash.ComputeHash(bytes);

        var builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
