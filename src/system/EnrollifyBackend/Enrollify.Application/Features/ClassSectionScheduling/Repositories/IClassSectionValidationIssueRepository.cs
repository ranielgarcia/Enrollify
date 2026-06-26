using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionValidationIssueRepository
{
  Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> validationIssues,
    CancellationToken cancellationToken);

  Task ReplaceAllSpecificIssuesForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> validationIssues,
    IEnumerable<ClassSectionValidationIssueTypeEnum> issueTypes,
    CancellationToken cancellationToken);
}
