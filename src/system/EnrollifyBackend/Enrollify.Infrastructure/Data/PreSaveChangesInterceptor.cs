using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Enrollify.Infrastructure.Data;

// Intercepts BEFORE SaveChanges/SaveChangesAsync executes to mutate tracked entities (e.g. soft delete / auditing)
public class PreSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is EnrollifyDbContext ctx)
        {
            ctx.ApplySoftDelete();
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is EnrollifyDbContext ctx)
        {
            ctx.ApplySoftDelete();
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
