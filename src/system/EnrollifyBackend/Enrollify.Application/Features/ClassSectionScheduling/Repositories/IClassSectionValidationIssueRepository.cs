using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionValidationIssueRepository
{
  Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> validationIssues,
    CancellationToken cancellationToken);

  /// <summary>
  /// Check if the class section has any errors severity validation issues. Used for eligibility checks before opening for enrollment
  /// and for displaying validation status in section details.
  /// </summary>
  /// <param name="classSectionId">Class Section Id</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<bool> HasValidationErrorsAsync(ClassSectionId classSectionId, CancellationToken cancellationToken);
}
