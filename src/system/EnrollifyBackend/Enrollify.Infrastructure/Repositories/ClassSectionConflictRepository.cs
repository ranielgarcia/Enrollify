using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionConflictAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionConflictRepository : IClassSectionConflictRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly ILogger<ClassSectionConflictRepository> _logger;

  public ClassSectionConflictRepository(EnrollifyDbContext dbContext, ILogger<ClassSectionConflictRepository> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task ReplaceAllForSectionAsync(ClassSectionId classSectionId, IEnumerable<ClassSectionConflict> conflicts, CancellationToken cancellationToken = default)
  {
    int deletedRows = await _dbContext.ClassSectionConflicts
      .Where(c => c.ClassSectionId == classSectionId)
      .ExecuteDeleteAsync(cancellationToken);

    // Insert fresh conflicts
    var freshConflicts = conflicts.ToList();
    if (freshConflicts.Count > 0)
      await _dbContext.ClassSectionConflicts
        .AddRangeAsync(freshConflicts, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogDebug(
      "Replaced conflicts for ClassSection {ClassSectionId}: removed {RemovedCount}, inserted {InsertedCount}",
      classSectionId.Value, deletedRows, freshConflicts.Count);
  }
}
