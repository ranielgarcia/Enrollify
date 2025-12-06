using Ardalis.Result;
using Enrollify.Core.Authentication;
using Mediator;

namespace Enrollify.Application.Authentication.GetContext;

public record GetCurrentUserContextQuery() : IQuery<Result<UserContext>>;

public class GetCurrentUserContextQueryHandler(ICurrentUserAccessor currentUserAccessor) :
    IQueryHandler<GetCurrentUserContextQuery, Result<UserContext>>
{
    public ValueTask<Result<UserContext>> Handle(GetCurrentUserContextQuery query, CancellationToken cancellationToken)
    {
        var userContext = currentUserAccessor.GetCurrentUser();
        if (userContext == null) return ValueTask.FromResult(Result<UserContext>.NotFound(""));
        return ValueTask.FromResult(Result.Success(userContext));
    }
}