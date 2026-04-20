using System;
using System.Collections.Generic;
using System.Text;
using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Constants;

namespace Enrollify.Infrastructure.Storage.Blob.Builders;

public class BlobOptionsBuilder<TSettings>
    where TSettings : IStorageSettings
{
    internal IStorageSettings Settings { get; set; }
    internal Type Type { get; set; }
    internal string ContainerName { get; set; } = string.Empty;
    internal int BlockSize { get; set; } = 8;
    internal int ConcurrentCount { get; set; } = Environment.ProcessorCount;

    public BlobOptionsBuilder(IStorageSettings settings, Type type)
    {
        Settings = settings;
        Type = type;
    }

    /// <summary>
    /// Configure container name and optional transfer settings.
    /// </summary>
    public BlobOptionsBuilder<TSettings> WithOptions(
        string containerName, int blockSize = 8, int concurrentCount = 2)
    {
        return WithContainerName(containerName)
            .WithBlockSize(blockSize)
            .WithConcurrentCount(concurrentCount);
    }

    public BlobOptionsBuilder<TSettings> WithContainerName(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentNullException(nameof(containerName));

        ContainerName = containerName;
        return this;
    }

    public BlobOptionsBuilder<TSettings> WithBlockSize(int blockSize)
    {
        if (blockSize is < BlobConstants.BlockSize.Min or > BlobConstants.BlockSize.Max)
        {
            throw new ArgumentOutOfRangeException(nameof(blockSize),
                $"Invalid block size ({blockSize}). Min: {BlobConstants.BlockSize.Min}, Max: {BlobConstants.BlockSize.Max}.");
        }

        BlockSize = blockSize;
        return this;
    }

    public BlobOptionsBuilder<TSettings> WithConcurrentCount(int concurrentCount)
    {
        if (concurrentCount is < BlobConstants.Concurrent.Min or > BlobConstants.Concurrent.Max)
        {
            throw new ArgumentOutOfRangeException(nameof(concurrentCount),
                $"Invalid concurrent count ({concurrentCount}). Min: {BlobConstants.Concurrent.Min}, Max: {BlobConstants.Concurrent.Max}.");
        }

        ConcurrentCount = concurrentCount;
        return this;
    }
}
