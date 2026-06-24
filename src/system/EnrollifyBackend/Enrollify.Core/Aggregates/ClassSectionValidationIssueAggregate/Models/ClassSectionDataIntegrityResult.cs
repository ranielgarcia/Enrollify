using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;

public class ClassSectionDataIntegrityResult
{
  public required ClassSectionId ClassSectionId { get; set; }
  public ClassSectionSubjectOfferingId? OfferingId { get; set; }

  public required ClassSectionValidationIssueTypeEnum Type { get; set; }
  public string Message { get; init; } = string.Empty;
}
