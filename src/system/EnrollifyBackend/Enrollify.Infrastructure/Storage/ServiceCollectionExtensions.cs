using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Storage;
using Enrollify.Application.Storage.Blob;
using Enrollify.Infrastructure.Storage.Blob;
using Enrollify.Infrastructure.Storage.Blob.Models;
using Enrollify.Infrastructure.Storage.Blob.Providers;
using Enrollify.Infrastructure.Storage.Blob.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Enrollify.Infrastructure.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageSettings(
        this IServiceCollection services,
      ConfigurationManager config)
    {
        var settings = new DefaultStorageSettings();
        config.GetRequiredSection(DefaultStorageSettings.Key).Bind(settings);

        services.AddStorageBlob<DefaultStorageSettings>(blob =>
        {
            blob.Settings = settings;
            blob.BlobBuilder.Configure<TeacherBlobConfig>(settings, opt => opt.WithOptions("teacher-files"));
        });

        return services;
    }


    /// <summary>
    /// Registers the blob storage infrastructure for a given storage account (TSettings).
    /// Call once per storage account.
    /// </summary>
    /// <example>
    /// services.AddStorageBlob&lt;DefaultStorageSettings&gt;(blob =>
    /// {
    ///     blob.Settings = settings;
    ///     blob.BlobBuilder.Configure&lt;MyBlobConfig&gt;(settings, opt => opt.WithOptions("my-container"));
    /// });
    /// </example>
    public static IServiceCollection AddStorageBlob<TSettings>(
        this IServiceCollection services,
        Action<BlobRepositoryOptions<TSettings>> configure)
        where TSettings : class, IStorageSettings
    {
        // 1. Build and register the options
        var options = new BlobRepositoryOptions<TSettings>();
        configure(options);

        services.Configure<BlobRepositoryOptions<TSettings>>(opt =>
        {
            opt.Settings = options.Settings;
            // Copy registrations from the builder into the IOptions instance
            foreach (var blobOpt in options.BlobOptions)
            {
                opt.BlobBuilder.Configure(
                    (TSettings)blobOpt.Settings,
                    blobOpt.Type,
                    builder =>
                    {
                        builder.WithContainerName(blobOpt.ContainerName)
                               .WithBlockSize(blobOpt.BlockSize)
                               .WithConcurrentCount(blobOpt.ConcurrentCount);
                    });
            }
        });

        // Alternative simpler registration — register the options object directly
        services.AddSingleton(options);

        // 2. Register infrastructure services (once per TSettings)
        services.TryAddSingleton<IBlobServiceClientProvider<TSettings>, BlobServiceClientProvider<TSettings>>();
        services.TryAddSingleton<IContainerNameProvider<TSettings>, ContainerNameProvider<TSettings>>();
        services.TryAddSingleton<IBlobOptionsProvider<TSettings>, BlobOptionsProvider<TSettings>>();
        services.TryAddSingleton<IBlobContainerService<TSettings>, BlobContainerService<TSettings>>();

        // 3. Register open-generic types (these resolve per TEntity)
        services.TryAddSingleton(
            typeof(IBlobContainerProvider<,>),
            typeof(BlobContainerProvider<,>));

        services.TryAddSingleton(
            typeof(IBlobRepository<,>),
            typeof(DefaultBlobRepository<,>));

        return services;
    }
}
