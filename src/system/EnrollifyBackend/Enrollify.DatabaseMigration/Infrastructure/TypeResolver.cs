using Spectre.Console.Cli;

namespace Enrollify.DatabaseMigration.Infrastructure;

// https://github.com/spectreconsole/examples/blob/main/examples/Cli/Injection/Infrastructure/MyTypeResolver.cs
public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return _provider.GetService(type);
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}