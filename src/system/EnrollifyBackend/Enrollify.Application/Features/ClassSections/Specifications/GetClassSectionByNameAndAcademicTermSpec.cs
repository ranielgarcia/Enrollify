using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class GetClassSectionByNameAndAcademicTermSpec : Specification<ClassSection>
{
    public GetClassSectionByNameAndAcademicTermSpec(string name, AcademicTermId academicTermId)
    {
        Query.Where(cs => cs.Name == name && cs.AcademicTermId == academicTermId);
    }
}
