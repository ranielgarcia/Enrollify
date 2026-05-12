using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Extensions;
using Enrollify.Application.Features.ClassSections.Specifications;
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
        private readonly IReadRepository<Teacher> _teacherReadRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;
        private readonly IReadRepository<Curriculum> _curriculumReadRepository;
        private readonly IClassSectionRepository _classSectionRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<Course> courseReadRepository,
            IReadRepository<AcademicYear> academicYearReadRepository,
            IReadRepository<Teacher> teacherReadRepository,
            IReadRepository<ClassSection> classSectionReadRepository,
            IReadRepository<Curriculum> curriculumReadRepository,
            IClassSectionRepository classSectionRepository,
            ILogger<Handler> logger)
        {
            _courseReadRepository = courseReadRepository;
            _academicYearReadRepository = academicYearReadRepository;
            _teacherReadRepository = teacherReadRepository;
            _classSectionReadRepository = classSectionReadRepository;
            _curriculumReadRepository = curriculumReadRepository;
            _classSectionRepository = classSectionRepository;
            _logger = logger;
        }
        public async ValueTask<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var course = await _courseReadRepository.GetByIdAsync(command.courseId, cancellationToken);
            if (course == null)
            {
                _logger.LogWarning("Course with an id of {CourseId} does not exists.", command.courseId);
                return Result.Invalid(new ValidationError($"Course not found."));
            }

            var academicYear = await _academicYearReadRepository
                .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.academicTermId), cancellationToken);
            if (academicYear == null)
            {
                _logger.LogWarning("Academic year with an academic term id of {AcademicTermId} does not exists.", command.academicTermId);
                return Result.Invalid(new ValidationError($"Academic term not found."));
            }
            var academicTerm = academicYear.AcademicTerms.FirstOrDefault(at => at.Id == command.academicTermId);
            if (academicTerm == null)
            {
                _logger.LogWarning("Academic term with an id of {AcademicTermId} does not exists in the academic year with an id of {AcademicYearId}.", command.academicTermId, academicYear.Id);
                return Result.Invalid(new ValidationError($"Academic term not found in the specified academic year."));
            }

            var adviser = await _teacherReadRepository.GetByIdAsync(command.adviserId, cancellationToken);
            if (adviser == null)
            {

                _logger.LogWarning("Adviser with an id of {TeacherId} does not exists.", command.adviserId);
                return Result.Invalid(new ValidationError($"Adviser not found."));
            }

            var classSectionCode = await GetSectionCode(command, cancellationToken);

            var currentCurriculum = await GetCurriculum(command.courseId, command.yearLevel, academicTerm.TermNumber, cancellationToken);


            var newClassSection = new ClassSection(new ClassSectionForCreation
            {
                Name = $"{course.Name}-{command.yearLevel}{classSectionCode}",
                YearLevel = command.yearLevel,
                CourseId = command.courseId,
                AcademicTermId = command.academicTermId,
                AdviserId = command.adviserId,
                StudentCapacity = command.studentCapacity,
                SectionCode = classSectionCode,
            });

            var result = await _classSectionRepository.Create(newClassSection, cancellationToken);

            return result;
        }


        private async Task<SectionCode> GetSectionCode(Command command, CancellationToken ct)
        {

            var existingClassSections = await _classSectionReadRepository.ListAsync(
                new GetExistingClassSectionsByCourseYearLevelAndTerm(command.yearLevel, command.courseId, command.academicTermId), ct);
            var lastExistingClassSectionCode = existingClassSections?.OrderBy(cs => cs.SectionCode).FirstOrDefault()?.SectionCode;

            return lastExistingClassSectionCode.GetNextSectionCode();
        }


        private async Task<Curriculum> GetCurriculum(CourseId courseId, YearLevel yearLevel, TermNumber termNumber, CancellationToken ct)
        {
            var curriculum = await _curriculumReadRepository
                .FirstOrDefaultAsync(new GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTerm(courseId, yearLevel, termNumber), ct);
            if (curriculum == null)
            {
                _logger.LogWarning("Curriculum for course with an id of {CourseId} and year level of {YearLevel} does not exists.", courseId, yearLevel);
                throw new Exception($"Curriculum not found for course with an id of {courseId} and year level of {yearLevel}.");
            }
            return curriculum;
        }

    }
}
