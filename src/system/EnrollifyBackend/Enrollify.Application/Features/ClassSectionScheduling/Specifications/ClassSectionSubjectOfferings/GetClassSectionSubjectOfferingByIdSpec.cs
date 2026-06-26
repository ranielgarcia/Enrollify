namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class GetClassSectionSubjectOfferingByIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingByIdSpec(ClassSectionSubjectOfferingId id)
  {
    Query.Where(c => c.Id == id);
  }
}
