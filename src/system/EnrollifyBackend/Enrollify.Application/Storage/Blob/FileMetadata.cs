using Enrollify.Core.ValueObjects.Storage;

namespace Enrollify.Application.Storage.Blob;

/// <summary>
/// Represents metadata about a file in storage without its content.
/// </summary>
public sealed class FileMetadata
{
    public required FileName FileName { get; init; }
    public required string Path { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
    public DateTimeOffset? LastModified { get; init; }
}
