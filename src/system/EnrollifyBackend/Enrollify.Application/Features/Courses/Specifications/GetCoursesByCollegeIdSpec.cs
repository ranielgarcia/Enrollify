using Ardalis.Specification;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.Courses.Specifications;

public class GetCoursesByCollegeIdSpec : Specification<Course>
{
  public GetCoursesByCollegeIdSpec(CollegeId collegeId)
  {
    Query.Where(x => x.CollegeId == collegeId);
  }
}
