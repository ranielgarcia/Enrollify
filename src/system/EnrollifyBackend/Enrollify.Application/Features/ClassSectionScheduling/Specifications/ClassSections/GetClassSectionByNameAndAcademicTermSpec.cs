namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionByNameAndAcademicTermSpec : Specification<ClassSection>
{
    public GetClassSectionByNameAndAcademicTermSpec(string name, AcademicTermId academicTermId)
    {
        Query.Where(cs => cs.Name == name && cs.AcademicTermId == academicTermId);
    }
}
