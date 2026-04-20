using System;
using System.Collections.Generic;
using System.Text;

namespace Enrollify.Infrastructure.Storage.Blob.Models;

/// <summary>
/// Resolved, immutable configuration for a single blob entity type.
/// Created once at startup from BlobOptionsBuilder and cached.
/// </summary>
public class BlobOptions
{
    public Type Type { get; }
    public string ContainerName { get; }
    public int BlockSize { get; }
    public int ConcurrentCount { get; }

    public BlobOptions(Type type, string containerName, int blockSize, int concurrentCount)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        ContainerName = string.IsNullOrWhiteSpace(containerName)
            ? throw new ArgumentException("Container name cannot be empty.", nameof(containerName))
            : containerName;
        BlockSize = blockSize;
        ConcurrentCount = concurrentCount;
    }
}
