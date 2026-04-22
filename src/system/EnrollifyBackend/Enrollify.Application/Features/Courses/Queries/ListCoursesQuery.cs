using Ardalis.Result;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.Courses.DTOs;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Courses.Queries;

public class ListCoursesQuery : IQuery<Result<List<CourseDto>>>
{
}

public class ListCoursesQueryHandler (IReadRepository<Course> courseReadRepository) : IQueryHandler<ListCoursesQuery, Result<List<CourseDto>>>
{

    public async ValueTask<Result<List<CourseDto>>> Handle(ListCoursesQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListCoursesWithAllNavigationSpec();
        var courses = await courseReadRepository.ListAsync(spec, cancellationToken);

        var toReturn = courses.
            Select(CourseDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
