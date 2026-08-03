using Enrollify.Application;
using Enrollify.SharedKernel;
using MediatR;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.Infrastructure;

public class ApplicationEventDispatcher(IMediator mediator, IDbContextOutbox outbox) : IApplicationEventDispatcher
{
  public async Task DispatchAsync(IApplicationEvent applicationEvent, CancellationToken ct = default)
  {
    await mediator.Publish(applicationEvent, ct);
  }

  public async Task DispatchDeferredAsync(IApplicationEvent applicationEvent, CancellationToken ct = default)
  {
    await outbox.PublishAsync(applicationEvent);
  }
}
