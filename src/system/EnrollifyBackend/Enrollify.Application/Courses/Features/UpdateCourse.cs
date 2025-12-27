using Ardalis.Result;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Courses.Features;

public static class UpdateCourse
{
    public sealed record Command(CourseId id, CourseCode code, string name, int durationYears, string description, CollegeId collegeId, RoomTypeId preferRoomTypeId)
        : ICommand<Result<CourseId>>;

    public sealed class Handler : ICommandHandler<Command, Result<CourseId>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly IReadRepository<College> _collegeReadRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;

        public Handler(
            ICourseRepository courseRepository,
            IReadRepository<Course> courseReadRepository,
            IReadRepository<College> collegeReadRepository,
            IReadRepository<RoomType> roomTypeReadRepository)
        {
            _courseRepository = courseRepository;
            _courseReadRepository = courseReadRepository;
            _collegeReadRepository = collegeReadRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
        }

        public async ValueTask<Result<CourseId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an id of {command.collegeId} not found");
            }

            var roomType = await _roomTypeReadRepository.GetByIdAsync(command.preferRoomTypeId, cancellationToken);
            if (roomType == null)
            {
                return Result.NotFound($"RoomType with an id of {command.preferRoomTypeId} not found");
            }

            var existing = await _courseReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing == null)
            {
                return Result.NotFound($"Course with an id of {command.id} not found");
            }

            existing.UpdateCode(command.code);
            existing.UpdateName(command.name);
            existing.UpdateDurationYears(command.durationYears);
            existing.UpdateDescription(command.description);
            existing.UpdateCollegeId(command.collegeId);
            existing.UpdatePreferRoomTypeId(command.preferRoomTypeId);

            var updateResult = await _courseRepository.Update(existing, cancellationToken);
            return updateResult;
        }
    }
}
