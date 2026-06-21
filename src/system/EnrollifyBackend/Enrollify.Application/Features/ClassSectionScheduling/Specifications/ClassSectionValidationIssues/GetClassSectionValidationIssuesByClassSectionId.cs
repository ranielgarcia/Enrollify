using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionValidationIssues;

public class GetClassSectionValidationIssuesByClassSectionId : Specification<ClassSectionValidationIssue>
{
  public GetClassSectionValidationIssuesByClassSectionId(ClassSectionId classSectionId)
  {
    Query.Where(x => x.ClassSectionId == classSectionId);
  }
}
