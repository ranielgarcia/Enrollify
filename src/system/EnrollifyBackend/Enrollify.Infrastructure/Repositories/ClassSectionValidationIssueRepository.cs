using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionValidationIssueRepository : IClassSectionValidationIssueRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly ILogger<ClassSectionValidationIssueRepository> _logger;

  public ClassSectionValidationIssueRepository(EnrollifyDbContext dbContext,
    ILogger<ClassSectionValidationIssueRepository> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task ReplaceAllForSectionAsync(ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> validationIssues, CancellationToken cancellationToken = default)
  {
    try
    {
      int deletedRows = await _dbContext.ClassSectionValidationIssues
        .Where(c => c.ClassSectionId == classSectionId)
        .ExecuteDeleteAsync(cancellationToken);

      // Insert fresh validationIssues
      var freshIssues = validationIssues.ToList();
      if (freshIssues.Count > 0)
        await _dbContext.ClassSectionValidationIssues
          .AddRangeAsync(freshIssues, cancellationToken);

      await _dbContext.SaveChangesAsync(cancellationToken);

      _logger.LogDebug(
        "Replaced validation issues for ClassSection {ClassSectionId}: removed {RemovedCount}, inserted {InsertedCount}",
        classSectionId.Value, deletedRows, freshIssues.Count);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error replacing validation issues for ClassSection {ClassSectionId}", classSectionId.Value);
      throw;
    }
  }

  public async Task ReplaceAllForSectionsAsync(List<ClassSectionId> classSectionIds,
    IEnumerable<ClassSectionValidationIssue> validationIssues, CancellationToken cancellationToken = default)
  {
    try
    {
      int deletedRows = await _dbContext.ClassSectionValidationIssues
        .Where(c => classSectionIds.Contains(c.ClassSectionId))
        .ExecuteDeleteAsync(cancellationToken);

      // Insert fresh validationIssues
      var freshIssues = validationIssues.ToList();
      if (freshIssues.Count > 0)
        await _dbContext.ClassSectionValidationIssues
          .AddRangeAsync(freshIssues, cancellationToken);

      await _dbContext.SaveChangesAsync(cancellationToken);

      _logger.LogDebug(
        "Replaced validation issues for ClassSections {@ClassSectionIds}: removed {RemovedCount}, inserted {InsertedCount}",
        classSectionIds.Select(x => x.Value), deletedRows, freshIssues.Count);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error replacing validation issues for ClassSections {ClassSectionIds}",
        classSectionIds.Select(x => x.Value));
      throw;
    }
  }
}
