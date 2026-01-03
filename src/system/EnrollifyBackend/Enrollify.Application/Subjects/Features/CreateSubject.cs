using Ardalis.Result;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public static class CreateSubject
{
    public sealed record Command(SubjectForCreation subject) : ICommand<Result<SubjectId>>;

    public sealed class Handler : ICommandHandler<Command, Result<SubjectId>>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;

        public Handler(ISubjectRepository subjectRepository,
            IReadRepository<Course> courseReadRepository,
            IReadRepository<RoomType> roomTypeReadRepository)
        {
            _subjectRepository = subjectRepository;
            _courseReadRepository = courseReadRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
        }
        public async ValueTask<Result<SubjectId>> Handle(Command command, CancellationToken cancellationToken)
        {            
            var preferRoomType = await _roomTypeReadRepository.GetByIdAsync(command.subject.PreferRoomTypeId, cancellationToken);
            if (preferRoomType == null) return Result.Invalid(new ValidationError { ErrorMessage = $"Room type with an ID of {command.subject.PreferRoomTypeId} not found." });

            var subject = new Subject(command.subject);
            var result = await _subjectRepository.Create(subject, cancellationToken);
            return result;
        }
    }
}
