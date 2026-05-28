using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class GetClassSectionsByCourseIdSpec : Specification<ClassSection>
{
    public GetClassSectionsByCourseIdSpec(CourseId courseId)
    {
        Query.Where(cs => cs.CourseId == courseId && cs.IsActive);
    }
}
