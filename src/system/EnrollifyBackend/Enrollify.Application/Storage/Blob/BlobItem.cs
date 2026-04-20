namespace Enrollify.Application.Storage.Blob;

public sealed class BlobItem
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required Stream Content { get; init; }
    public required long Size { get; init; }
    public DateTimeOffset? LastModified { get; init; }
}
