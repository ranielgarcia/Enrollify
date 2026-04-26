using Enrollify.Application.Storage;

namespace Enrollify.Infrastructure.Storage.Blob.Builders;

public interface IBlobBuilder<TSettings>
    where TSettings : IStorageSettings
{
    IReadOnlyList<BlobOptionsBuilder<TSettings>> Options { get; }

    IBlobBuilder<TSettings> Configure<T>(
        TSettings settings,
        Action<BlobOptionsBuilder<TSettings>> blobOptions) where T : class;

    // Add to IBlobBuilder<TSettings>
    IBlobBuilder<TSettings> Configure(
        TSettings settings,
        Type entityType,
        Action<BlobOptionsBuilder<TSettings>> blobOptions);
}

public class BlobBuilder<TSettings>
    : IBlobBuilder<TSettings>
    where TSettings : IStorageSettings
{
    private readonly List<BlobOptionsBuilder<TSettings>> _options = new();

    public IReadOnlyList<BlobOptionsBuilder<TSettings>> Options => _options;

    public IBlobBuilder<TSettings> Configure<T>(
        TSettings settings,
        Action<BlobOptionsBuilder<TSettings>> blobOptions)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(blobOptions);

        BlobOptionsBuilder<TSettings> optionsBuilder = new(settings, typeof(T));
        blobOptions(optionsBuilder);
        _options.Add(optionsBuilder);
        return this;
    }

    public IBlobBuilder<TSettings> Configure(
    TSettings settings,
    Type entityType,
    Action<BlobOptionsBuilder<TSettings>> blobOptions)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(blobOptions);

        BlobOptionsBuilder<TSettings> optionsBuilder = new(settings, entityType);
        blobOptions(optionsBuilder);
        _options.Add(optionsBuilder);
        return this;
    }
}
