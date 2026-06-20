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

  public async Task<bool> HasValidationErrorsAsync(ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
