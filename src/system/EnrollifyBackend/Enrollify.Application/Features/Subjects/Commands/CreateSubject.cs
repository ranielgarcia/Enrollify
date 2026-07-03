using Enrollify.Application.Features.Subjects;
using Enrollify.Core.Aggregates.SubjectAggregate.Models;
using Enrollify.Core.Constants.AcademicBuiltInData;

namespace Enrollify.Application.Features.Subjects.Commands;

public static class CreateSubject
{
    public sealed record Command(SubjectForCreation subject) : IRequest<Result<SubjectId>>;

    public sealed class Handler : IRequestHandler<Command, Result<SubjectId>>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;

        public Handler(ISubjectRepository subjectRepository,
            IReadRepository<Course> courseReadRepository,
            IReadRepository<RoomType> roomTypeReadRepository)
        {
            _subjectRepository = subjectRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
        }
        public async Task<Result<SubjectId>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (command.subject.Code == BuiltInSubjectsEnum.ElectivePlaceholder.Code)
                return Result.Invalid(new ValidationError { ErrorMessage = $"The subject code '{BuiltInSubjectsEnum.ElectivePlaceholder.Code}' is reserved and cannot be used." });

            var preferRoomType = await _roomTypeReadRepository.GetByIdAsync(command.subject.PreferRoomTypeId, cancellationToken);
            if (preferRoomType == null) return Result.Invalid(new ValidationError { ErrorMessage = $"Room type with an ID of {command.subject.PreferRoomTypeId} not found." });

            var subject = new Subject(command.subject);
            var result = await _subjectRepository.Create(subject, cancellationToken);
            return result;
        }
    }
}
