using Ardalis.Result;
using Enrollify.Application.Courses.DTOs;
using Enrollify.Application.Courses.Specifications;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Courses.Features;

public class ListCoursesQuery : IQuery<Result<List<CourseDTO>>>
{
}

public class ListCoursesQueryHandler (IReadRepository<Course> courseReadRepository) : IQueryHandler<ListCoursesQuery, Result<List<CourseDTO>>>
{

    public async ValueTask<Result<List<CourseDTO>>> Handle(ListCoursesQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListCoursesWithAllNavigationSpec();
        var courses = await courseReadRepository.ListAsync(spec, cancellationToken);

        var toReturn = courses.
            Select(CourseDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}