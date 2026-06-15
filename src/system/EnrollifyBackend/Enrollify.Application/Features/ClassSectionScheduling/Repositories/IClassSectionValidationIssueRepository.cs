using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionValidationIssueRepository
{
  Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> validationIssues,
    CancellationToken cancellationToken = default);
}
