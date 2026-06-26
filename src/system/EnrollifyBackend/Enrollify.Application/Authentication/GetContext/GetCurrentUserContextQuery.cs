using Enrollify.Core.Authentication;

namespace Enrollify.Application.Authentication.GetContext;

public record GetCurrentUserContextQuery() : IRequest<Result<UserContext>>;

public class GetCurrentUserContextQueryHandler(ICurrentUserAccessor currentUserAccessor) :
    IRequestHandler<GetCurrentUserContextQuery, Result<UserContext>>
{
    public Task<Result<UserContext>> Handle(GetCurrentUserContextQuery query, CancellationToken cancellationToken)
    {
        var userContext = currentUserAccessor.GetCurrentUser();
        if (userContext == null) return Task.FromResult(Result<UserContext>.NotFound(""));
        return Task.FromResult(Result.Success(userContext));
    }
}
