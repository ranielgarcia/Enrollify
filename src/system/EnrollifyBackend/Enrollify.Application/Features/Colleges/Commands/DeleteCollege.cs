using Enrollify.Application.Features.Buildings.Queries;

namespace Enrollify.Application.Features.Colleges.Commands;

public static class DeleteCollege
{
    public sealed record Command(CollegeId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICollegeRepository _collegeRepository;
        private readonly IMediator _mediator;

        public Handler(ICollegeRepository collegeRepository, IMediator mediator)
        {
            _collegeRepository = collegeRepository;
            _mediator = mediator;
        }
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var countBuildings = await _mediator.Send(new CountBuildingsByCollegeQuery { CollegeId = command.id}, cancellationToken);
            if (countBuildings.Value > 0)
            {
                return Result.Invalid(new ValidationError($"This college cannot be deleted because it has {countBuildings.Value} building(s) associated with it. \n Please reassign or remove these buildings from this college before deleting."));
            }
            // TODO: Implement this:
            //var courses = await _courseRepository.GetAllByCollege(command.id, cancellationToken);
            //if (courses.Count > 0)
            //{
            //    return Result.Invalid(new ValidationError($"This college cannot be deleted because it has {courses.Count} course(s) associated with it. \n Please reassign or remove these courses from this college before deleting."));
            //}
            return await _collegeRepository.Delete(command.id, cancellationToken);
        }
    }
}
