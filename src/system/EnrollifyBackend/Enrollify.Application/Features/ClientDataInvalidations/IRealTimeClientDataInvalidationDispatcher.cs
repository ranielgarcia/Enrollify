using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.ClientDataInvalidations;

public interface IRealTimeClientDataInvalidationDispatcher
{
  Task SendToUsersAsync(IEnumerable<UserId> userIds, string type, CancellationToken ct);
}
