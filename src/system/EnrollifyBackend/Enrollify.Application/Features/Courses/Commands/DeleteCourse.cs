namespace Enrollify.Application.Features.Courses.Commands;

public static class DeleteCourse
{
    public sealed record Command(CourseId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;

        public Handler(
            ICourseRepository courseRepository,
            IReadRepository<ClassSection> classSectionReadRepository)
        {
            _courseRepository = courseRepository;
            _classSectionReadRepository = classSectionReadRepository;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var sections = await _classSectionReadRepository.ListAsync(
                new GetClassSectionsByCourseIdSpec(command.id), cancellationToken);
            if (sections.Count > 0)
                return Result.Forbidden(
                    $"Cannot delete course — {sections.Count} class section(s) reference it.");

            return await _courseRepository.Delete(command.id, cancellationToken);
        }
    }
}
