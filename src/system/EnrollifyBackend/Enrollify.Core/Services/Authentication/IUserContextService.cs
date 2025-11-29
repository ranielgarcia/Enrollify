using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Services.Authentication;

public interface IUserContextService
{
    ValueTask<UserContext?> GetUserContextByEmail(UserEmail email, CancellationToken cancellationToken);
}
