namespace Enrollify.Application.Storage;

public class DefaultStorageSettings : IStorageSettings
{
    public const string Key = "Storage:Default";
    public string ConnectionString { get; set; } = string.Empty;
}
