using Ardalis.Result;
using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Services;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentsForAcademicYear
{
    public record Command(AcademicYearId AcademicYearId) : ICommand<Result>;
    public class Handler : ICommandHandler<Command, Result>
    {
        private readonly ICourseCurriculumAssignmentRepository _repository;
        private readonly IReadRepository<CourseCurriculumAssignment> _readRepository;
        private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly IApplicableCurriculumQueryService _applicableCurriculumQueryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Command> _logger;

        public Handler(ICourseCurriculumAssignmentRepository repository,
            IReadRepository<CourseCurriculumAssignment> readRepository,
            IReadRepository<AcademicYear> academicYearReadRepository,
            IReadRepository<Course> courseReadRepository,
            IApplicableCurriculumQueryService applicableCurriculumQueryService,
            IUnitOfWork unitOfWork,
            ILogger<Command> logger)
        {
            _repository = repository;
            _readRepository = readRepository;
            _academicYearReadRepository = academicYearReadRepository;
            _courseReadRepository = courseReadRepository;
            _applicableCurriculumQueryService = applicableCurriculumQueryService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var academicYear = await _academicYearReadRepository.GetByIdAsync(command.AcademicYearId, cancellationToken);
            if (academicYear == null)
            {
                _logger.LogError("Unable to find the academic year with an id of {AcademicYearId}", command.AcademicYearId);
                return Result.Invalid(new ValidationError("Academic year not found."));
            }

            var courses = await _courseReadRepository.ListAsync(cancellationToken);
            var existingAssignments = await _readRepository.ListAsync(new GetAllCourseCurriculumAssignmentsByAcademicYearIdSpec(academicYear.Id), cancellationToken);

            // Get the latest active curriculum for each course that is applicable to the academic year's start date/year
            var applicableCurriculums = await _applicableCurriculumQueryService.GetApplicableCurriculumsForAcademicYearStartDateAsync(academicYear.StartDate, cancellationToken);

            var applicableCurriculumsByCourseId = applicableCurriculums.ToDictionary(c => c.CourseId, c => c);
            var existingAssignmentByCourseId = existingAssignments.ToDictionary(c => c.CourseId, c => c);

            var courseCurriculumAssignmentsToCreate = new List<CourseCurriculumAssignment>();
            var courseCurriculumAssignmentsToUpdate = new List<CourseCurriculumAssignment>();
            foreach (var course in courses)
            {
                var existing = existingAssignmentByCourseId.GetValueOrDefault(course.Id);
                var curriculum = applicableCurriculumsByCourseId.GetValueOrDefault(course.Id);

                if (curriculum == null)
                {
                    _logger.LogError("Unable to find an active curriculum for course {CourseName}", course.Name);
                    return Result.Error($"Unable to find an active curriculum for course {course.Name}");
                }

                if (existing != null && existing.CurriculumId != curriculum.Id)
                {
                    _logger.LogInformation("Updated course-curriculum assignment, From CurriculumId {PreviousCurriculumId} To {CurriculumId}",
                        existing.CurriculumId, curriculum.Id);
                    existing.UpdateCurriculum(curriculum.Id);
                    courseCurriculumAssignmentsToUpdate.Add(existing);
                }

                if (existing == null)
                {
                    var newAssignment = new CourseCurriculumAssignment(course.Id, command.AcademicYearId, curriculum.Id);
                    _logger.LogInformation("Added new course-curriculum assignment: {@newCourseCurriculumAssignment}", newAssignment);
                    courseCurriculumAssignmentsToCreate.Add(newAssignment);
                }
            }

            // Begin transaction to ensure all database operations succeed or fail together
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                await _repository.BulkCreate(courseCurriculumAssignmentsToCreate, cancellationToken);
                await _repository.BulkUpdate(courseCurriculumAssignmentsToUpdate, cancellationToken);
                await transaction.CommitAsync();

                return Result.Success();
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to sync course-curriculum assignments.");
                return Result.Error("Failed to sync course-curriculum assignments.");
            }
        }
    }
}
