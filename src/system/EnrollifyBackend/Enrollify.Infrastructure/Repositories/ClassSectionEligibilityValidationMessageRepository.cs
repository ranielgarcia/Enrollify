using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionEligibilityValidationMessageRepository
  : IClassSectionEligibilityValidationMessageRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly ILogger<ClassSectionEligibilityValidationMessageRepository> _logger;

  public ClassSectionEligibilityValidationMessageRepository(
    EnrollifyDbContext dbContext,
    ILogger<ClassSectionEligibilityValidationMessageRepository> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionEnrollmentEligibilityValidationMessage> messages,
    CancellationToken cancellationToken = default)
  {
    int deletedRows = await _dbContext.ClassSectionEnrollmentEligibilityValidationMessages
      .Where(m => m.ClassSectionId == classSectionId)
      .ExecuteDeleteAsync(cancellationToken);

    // Insert fresh messages
    var freshMessages = messages.ToList();
    if (freshMessages.Count > 0)
      await _dbContext.ClassSectionEnrollmentEligibilityValidationMessages
        .AddRangeAsync(freshMessages, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogDebug(
      "Replaced eligibility messages for ClassSection {ClassSectionId}: removed {RemovedCount}, inserted {InsertedCount}",
      classSectionId.Value, deletedRows, freshMessages.Count);
  }
}

