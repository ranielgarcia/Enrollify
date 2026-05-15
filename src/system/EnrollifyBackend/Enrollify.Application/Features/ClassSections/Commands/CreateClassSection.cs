using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Extensions;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSections.Commands;

public static class CreateClassSection
{
    public sealed record Command(
        YearLevel yearLevel,
        CourseId courseId,
        AcademicTermId academicTermId,
        TeacherId adviserId,
        int studentCapacity) : ICommand<Result<ClassSectionId>>;

    public sealed class Handler : ICommandHandler<Command, Result<ClassSectionId>>
    {
        private readonly IReadRepository<Course> _courseReadRepository;
        private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;
        private readonly IReadRepository<Curriculum> _curriculumReadRepository;
        private readonly IClassSectionRepository _classSectionRepository;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<Course> courseReadRepository,
            IReadRepository<AcademicYear> academicYearReadRepository,
            IReadRepository<ClassSection> classSectionReadRepository,
            IReadRepository<Curriculum> curriculumReadRepository,
            IClassSectionRepository classSectionRepository,
            IMediator mediator,
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _courseReadRepository = courseReadRepository;
            _academicYearReadRepository = academicYearReadRepository;
            _classSectionReadRepository = classSectionReadRepository;
            _curriculumReadRepository = curriculumReadRepository;
            _classSectionRepository = classSectionRepository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async ValueTask<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Get validated entities (we know they exist because of validation)
            var course = await _courseReadRepository.GetByIdAsync(command.courseId, cancellationToken);
            var academicYear = await _academicYearReadRepository
                .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.academicTermId), cancellationToken);
            var academicTerm = academicYear!.AcademicTerms.First(at => at.Id == command.academicTermId);

            // Generate section code
            var sectionCode = await GetSectionCode(command, cancellationToken);

            // Get curriculum for this class section
            var latestActiveCurriculum = await GetCurriculum(
                command.courseId,
                command.yearLevel,
                academicTerm.TermNumber,
                cancellationToken);

            if (!latestActiveCurriculum.IsSuccess || latestActiveCurriculum.Value == null)
            {
                return Result.Invalid(latestActiveCurriculum.ValidationErrors);
            }

            var curriculum = latestActiveCurriculum.Value;

            // Get subjects that should be offered for this class section
            var curriculumSubjects = curriculum.GetSubjectsByYearAndTerm(command.yearLevel, academicTerm.TermNumber);
            if (!curriculumSubjects.Any())
            {
                _logger.LogWarning("No curriculum subjects found for course with an id of {CourseId}, year level of {YearLevel}, and term number of {TermNumber}.", command.courseId, command.yearLevel, academicTerm.TermNumber);
                return Result.Error("No curriculum subjects found for the specified course, year level, and term.");
            }

            // Begin transaction to ensure all database operations succeed or fail together
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create the class section
                var newClassSection = new ClassSection(new ClassSectionForCreation
                {
                    Name = $"{course!.Code.Value}-{command.yearLevel}{sectionCode}",
                    YearLevel = command.yearLevel,
                    CourseId = command.courseId,
                    AcademicTermId = command.academicTermId,
                    AdviserId = command.adviserId,
                    SectionCode = sectionCode,
                });
                var createResult = await _classSectionRepository.Create(newClassSection, cancellationToken);
                if (!createResult.IsSuccess)
                {
                    _logger.LogError("Failed to create class section: {Errors}", string.Join(", ", createResult.Errors));
                    return Result.Error("Unable to create the class section.");
                }

                var classSectionId = createResult.Value;

                foreach (var curriculumSubject in curriculumSubjects)
                {
                    var offeringResult = await _mediator.Send(
                        new CreateClassSectionSubjectOffering.Command(classSectionId, curriculumSubject.SubjectId),
                        cancellationToken);
                    if (!offeringResult.IsSuccess)
                    {
                        _logger.LogError(
                            "Failed to create subject offering for ClassSection {ClassSectionId}, Subject {SubjectId}: {Errors}",
                            classSectionId,
                            curriculumSubject.SubjectId,
                            string.Join(", ", offeringResult.Errors));
                        return Result.Error("Unable to create one or more subject offerings.");
                    }
                }

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully created class section {ClassSectionId} with {SubjectCount} subject offerings",
                    classSectionId,
                    curriculumSubjects.Count());

                return Result.Success(createResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class section and subject offerings");
                // Transaction will rollback automatically on dispose
                return Result.Error("An unexpected error occurred while creating the class section.");
            }
        }

        private async Task<SectionCode> GetSectionCode(Command command, CancellationToken ct)
        {

            var existingClassSections = await _classSectionReadRepository.ListAsync(
                new GetExistingClassSectionsByCourseYearLevelAndTerm(command.yearLevel, command.courseId, command.academicTermId), ct);
            var lastExistingClassSectionCode = existingClassSections?.OrderByDescending(cs => cs.SectionCode).FirstOrDefault()?.SectionCode;

            return lastExistingClassSectionCode.GetNextSectionCode();
        }

        private async Task<Result<Curriculum>> GetCurriculum(CourseId courseId, YearLevel yearLevel, TermNumber termNumber, CancellationToken ct)
        {
            var curriculum = await _curriculumReadRepository
                .FirstOrDefaultAsync(new GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTermSpec(courseId, yearLevel, termNumber), ct);

            if (curriculum == null)
            {
                _logger.LogWarning("Curriculum for course with an id of {CourseId} and year level of {YearLevel} does not exist.", courseId, yearLevel);
                return Result.Invalid(new ValidationError("No active curriculum found for the specified course and year level."));
            }

            return Result.Success(curriculum);
        }

    }
}
