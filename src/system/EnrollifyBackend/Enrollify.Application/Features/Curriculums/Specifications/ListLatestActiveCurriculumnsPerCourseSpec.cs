using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Models.Views;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class ListLatestActiveCurriculumnsPerCourseSpec : Specification<LatestActiveCurriculumPerCourseView>
{
    public ListLatestActiveCurriculumnsPerCourseSpec()
    {
        // Query is already configured by the view
    }
}
