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
using FluentValidation;
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
        private readonly IValidator<Command> _validator;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<Course> courseReadRepository,
            IReadRepository<AcademicYear> academicYearReadRepository,
            IReadRepository<ClassSection> classSectionReadRepository,
            IReadRepository<Curriculum> curriculumReadRepository,
            IClassSectionRepository classSectionRepository,
            IValidator<Command> validator,
            IMediator mediator,
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _courseReadRepository = courseReadRepository;
            _academicYearReadRepository = academicYearReadRepository;
            _classSectionReadRepository = classSectionReadRepository;
            _curriculumReadRepository = curriculumReadRepository;
            _classSectionRepository = classSectionRepository;
            _validator = validator;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async ValueTask<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new ValidationError(e.ErrorMessage))
                    .ToArray();
                return Result.Invalid(errors);
            }

            // Get validated entities (we know they exist because of validation)
            var course = await _courseReadRepository.GetByIdAsync(command.courseId, cancellationToken);
            var academicYear = await _academicYearReadRepository
                .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.academicTermId), cancellationToken);
            var academicTerm = academicYear!.AcademicTerms.First(at => at.Id == command.academicTermId);

            // Generate section code
            var sectionCode = await GetSectionCode(command, cancellationToken);

            // Get curriculum for this class section
            var curriculumResult = await GetCurriculum(
                command.courseId,
                command.yearLevel,
                academicTerm.TermNumber,
                cancellationToken);

            if (!curriculumResult.IsSuccess)
            {
                return Result.Invalid(curriculumResult.ValidationErrors);
            }

            var curriculum = curriculumResult.Value;

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
                var createNewClassSectionResult = await _classSectionRepository.Create(newClassSection, cancellationToken);
                if (!createNewClassSectionResult.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Error("Unable to create new class section");
                }

                var latestActiveCurriculum = await GetCurriculum(course.Id, command.yearLevel, academicTerm.TermNumber, cancellationToken);
                if (!latestActiveCurriculum.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Invalid(new ValidationError($"Unable to find the latest active curriculum for course {course.Name}"));
                }

                var curriculumSubjectsForYearLevelAndTerm = latestActiveCurriculum.Value.GetSubjectsByYearAndTerm(command.yearLevel, academicTerm.TermNumber);

                foreach (var curSubject in curriculumSubjectsForYearLevelAndTerm)
                {
                    await _mediator.Send(new CreateClassSectionSubjectOffering.Command(createNewClassSectionResult.Value, curSubject.SubjectId), cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                // Transaction will be rolled back automatically on dispose if not committed
                throw;
            }

            return Result.Success();
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
                .FirstOrDefaultAsync(new GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTerm(courseId, yearLevel, termNumber), ct);

            if (curriculum == null)
            {
                _logger.LogWarning("Curriculum for course with an id of {CourseId} and year level of {YearLevel} does not exist.", courseId, yearLevel);
                return Result.Invalid(new ValidationError("No active curriculum found for the specified course and year level."));
            }

            return Result.Success(curriculum);
        }

    }
}
