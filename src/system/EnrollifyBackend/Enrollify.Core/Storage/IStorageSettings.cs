namespace Enrollify.Core.Storage;

/// <summary>
/// Marker + configuration interface for a storage account.
/// Each storage account you connect to gets its own implementation.
/// </summary>
public interface IStorageSettings
{
    /// <summary>Azure Storage connection string or account endpoint.</summary>
    string ConnectionString { get; set; }
}
