namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class GetClassSectionSubjectOfferingsBySubjectIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsBySubjectIdSpec(SubjectId subjectId)
  {
    Query.Where(o => o.SubjectId == subjectId && o.IsActive);
  }
}
