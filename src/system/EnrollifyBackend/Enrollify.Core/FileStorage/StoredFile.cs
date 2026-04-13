namespace Enrollify.Core.FileStorage;

/// <summary>
/// Represents a file retrieved from storage, including its content stream and metadata.
/// The caller is responsible for disposing the <see cref="Content"/> stream.
/// </summary>
public sealed class StoredFile
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required Stream Content { get; init; }
    public required long Size { get; init; }
    public DateTimeOffset? LastModified { get; init; }
}
