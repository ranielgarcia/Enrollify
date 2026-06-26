namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionsByAcademicTermIdsSpec : Specification<ClassSection>
{
    public GetClassSectionsByAcademicTermIdsSpec(IEnumerable<AcademicTermId> termIds)
    {
        Query.Where(cs => termIds.Contains(cs.AcademicTermId));
    }
}
