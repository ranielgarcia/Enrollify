using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.Courses.DTOs;

namespace Enrollify.Application.Features.Courses.Queries;

public class ListCoursesQuery : IRequest<Result<List<CourseDto>>>
{
}

public class ListCoursesQueryHandler (IReadRepository<Course> courseReadRepository) : IRequestHandler<ListCoursesQuery, Result<List<CourseDto>>>
{

    public async Task<Result<List<CourseDto>>> Handle(ListCoursesQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListCoursesWithAllNavigationSpec();
        var courses = await courseReadRepository.ListAsync(spec, cancellationToken);

        var toReturn = courses.
            Select(CourseDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
