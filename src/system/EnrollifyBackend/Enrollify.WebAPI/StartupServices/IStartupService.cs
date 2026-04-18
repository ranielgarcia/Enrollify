namespace Enrollify.WebAPI.StartupServices;

public interface IStartupService
{
    Task Initialize(CancellationToken cancellation);
}
