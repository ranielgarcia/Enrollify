using Ardalis.Result;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public static class UpdateSubject
{
    public class Command : ICommand<Result<SubjectId>>
    {
        public SubjectId Id { get; set; }
        public SubjectCode Code { get; set; }
        public string Title { get; set; } = null!;
        public decimal Units { get; set; }
        public string Description { get; set; } = null!;
        public CourseId CourseId { get; set; }
        public RoomTypeId PreferRoomTypeId { get; set; }

        public List<SubjectId> Prerequisites { get; set; } = new List<SubjectId>();
    }


    public sealed class Handler : ICommandHandler<Command, Result<SubjectId>>
    {
        private readonly IReadRepository<Subject> _subjectReadRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;

        public Handler(IReadRepository<Subject> subjectReadRepository,
            ISubjectRepository subjectRepository,
            IReadRepository<Course> courseReadRepository,
            IReadRepository<RoomType> roomTypeReadRepository)
        {
            _subjectReadRepository = subjectReadRepository;
            _subjectRepository = subjectRepository;
            _courseReadRepository = courseReadRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
        }

        public async ValueTask<Result<SubjectId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await _subjectReadRepository.GetByIdAsync(command.Id, cancellationToken);
            if (existing == null)
            {
                return Result.NotFound($"Subject with an ID of {command.Id} not found.");
            }

            var course = await _courseReadRepository.GetByIdAsync(command.CourseId, cancellationToken);
            if (course is null)
            {
                return Result.Invalid(new ValidationError { ErrorMessage = $"Course with an ID of {command.CourseId} not found." });
            }

            var preferRoomType = await _roomTypeReadRepository.GetByIdAsync(command.PreferRoomTypeId, cancellationToken);
            if (preferRoomType == null) return Result.Invalid(new ValidationError { ErrorMessage = $"Room type with an ID of {command.PreferRoomTypeId} not found." });

            if (command.Prerequisites.Count > 0)
            {
                var uniqueSubjectIds = command.Prerequisites.Distinct().ToList();
                var prerequisiteSubjects = await _subjectRepository.GetSubjectsById(uniqueSubjectIds, cancellationToken);

                if (prerequisiteSubjects.Count != uniqueSubjectIds.Count)
                {
                    var foundIds = prerequisiteSubjects.Select(s => s.Id).ToHashSet();
                    var notFoundIds = uniqueSubjectIds.Where(id => !foundIds.Contains(id)).ToList();
                    return Result.NotFound($"Prerequisite Subjects with IDs {string.Join(", ", notFoundIds)} not found.");
                }
            }

            existing
                .UpdateCode(command.Code)
                .UpdateTitle(command.Title)
                .UpdateDescription(command.Description)
                .UpdateUnits(command.Units)
                .UpdateCourse(command.CourseId)
                .UpdatePreferRoomType(command.PreferRoomTypeId);

            await _subjectRepository.Update(existing, cancellationToken);

            return Result.Success(existing.Id);
        }
    }
}
