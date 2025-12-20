using Enrollify.Core.Authentication;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Enrollify.Infrastructure.Data;

// Intercepts BEFORE SaveChanges/SaveChangesAsync executes to mutate tracked entities (e.g. soft delete / auditing)
public class PreSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public PreSaveChangesInterceptor(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();

        if (eventData.Context is EnrollifyDbContext ctx)
        {
            ctx.ApplySoftDelete(currentUser);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();

        if (eventData.Context is EnrollifyDbContext ctx)
        {
            ctx.ApplySoftDelete(currentUser);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
