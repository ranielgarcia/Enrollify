using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Extensions;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Features.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSections.Commands;

public static class BulkInitializeClassSectionsForAcademicYear
{
    public sealed record Payload(
        CourseId courseId,
        CurriculumId curriculumId,
        int numberOfSections);

    public sealed record Command(
        AcademicTermId academicTermId,
        YearLevel yearLevel,
        List<Payload> requestPayload
        ) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IReadRepository<Course> _courseRepository;
        private readonly IReadRepository<AcademicYear> _academicYearRepository;
        private readonly IReadRepository<Curriculum> _curriculumRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;
        private readonly IClassSectionRepository _classSectionRepository;
        private readonly IClassSectionSubjectOfferingRepository _classSectionSubjectOfferingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<Course> courseRepository,
            IReadRepository<AcademicYear> academicYearRepository,
            IReadRepository<Curriculum> curriculumRepository,
            IReadRepository<ClassSection> classSectionReadRepository,
            IClassSectionRepository classSectionRepository,
            IClassSectionSubjectOfferingRepository classSectionSubjectOfferingRepository,
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _courseRepository = courseRepository;
            _academicYearRepository = academicYearRepository;
            _curriculumRepository = curriculumRepository;
            _classSectionReadRepository = classSectionReadRepository;
            _classSectionRepository = classSectionRepository;
            _classSectionSubjectOfferingRepository = classSectionSubjectOfferingRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Get academic year and term (validated by validator)
            var academicYear = await _academicYearRepository
                .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.academicTermId), cancellationToken);
            var academicTerm = academicYear!.AcademicTerms.First(at => at.Id == command.academicTermId);

            // Get all courses (validated by validator)
            var courseIds = command.requestPayload.Select(p => p.courseId).Distinct().ToList();
            var courses = await _courseRepository.ListAsync(new BulkGetMinimumCoursesByIdsSpec(courseIds), cancellationToken);
            var courseById = courses.ToDictionary(c => c.Id);

            // Get all curricula (validated by validator)
            var curriculumIds = command.requestPayload
                .Select(p => p.curriculumId)
                .Where(id => id != CurriculumId.From(0))
                .Distinct()
                .ToList();
            var curricula = curriculumIds.Count > 0
                ? await _curriculumRepository.ListAsync(new BulkGetCurriculumWithSubjectsByIdsSpec(curriculumIds), cancellationToken)
                : new List<Curriculum>();
            var curriculumById = curricula.ToDictionary(c => c.Id);

            // Begin transaction to ensure all database operations succeed or fail together
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var totalSectionsCreated = 0;

                foreach (var payload in command.requestPayload)
                {
                    var course = courseById[payload.courseId];
                    var curriculum = curriculumById[payload.curriculumId];

                    // Get subjects that should be offered for this class section
                    var curriculumSubjects = curriculum.GetSubjectsByYearAndTerm(command.yearLevel, academicTerm.TermNumber);
                    if (!curriculumSubjects.Any())
                    {
                        _logger.LogWarning(
                            "No curriculum subjects found for course {CourseId}, year level {YearLevel}, term {TermNumber}",
                            payload.courseId, command.yearLevel, academicTerm.TermNumber);
                        return Result.Error($"No curriculum subjects found for course '{course.Code.Value}', year level {command.yearLevel}, term {academicTerm.TermNumber}.");
                    }

                    // Get existing sections to determine starting section code
                    var existingClassSections = await _classSectionReadRepository.ListAsync(
                        new GetExistingClassSectionsByCourseYearLevelAndTerm(command.yearLevel, payload.courseId, command.academicTermId),
                        cancellationToken);
                    var lastExistingClassSectionCode = existingClassSections?
                        .OrderByDescending(cs => cs.SectionCode)
                        .FirstOrDefault()?
                        .SectionCode;

                    // Create the requested number of sections
                    for (var i = 0; i < payload.numberOfSections; i++)
                    {
                        // Generate next section code (A, B, C, etc.)
                        var sectionCode = lastExistingClassSectionCode.GetNextSectionCode();
                        lastExistingClassSectionCode = sectionCode;

                        // Create class section with auto-generated name
                        var newClassSection = new ClassSection(new ClassSectionForCreation
                        {
                            Name = $"{course.Code.Value}-{command.yearLevel}{sectionCode}",
                            YearLevel = command.yearLevel,
                            CourseId = payload.courseId,
                            CurriculumId = curriculum.Id,
                            AcademicTermId = command.academicTermId,
                            AdviserId = null, // No adviser assigned during bulk initialization
                            SectionCode = sectionCode,
                        });

                        var createResult = await _classSectionRepository.Create(newClassSection, cancellationToken);
                        if (!createResult.IsSuccess)
                        {
                            _logger.LogError("Failed to create class section: {Errors}", string.Join(", ", createResult.Errors));
                            return Result.Error($"Unable to create class section for course '{course.Code.Value}'.");
                        }

                        var classSectionId = createResult.Value;

                        // Create subject offerings for this section
                        foreach (var curriculumSubject in curriculumSubjects)
                        {
                            var offeringResult = await _classSectionSubjectOfferingRepository.Create(
                                new ClassSectionSubjectOffering(classSectionId, curriculumSubject.SubjectId),
                                cancellationToken);

                            if (!offeringResult.IsSuccess)
                            {
                                _logger.LogError(
                                    "Failed to create subject offering for ClassSection {ClassSectionId}, Subject {SubjectId}: {Errors}",
                                    classSectionId, curriculumSubject.SubjectId, string.Join(", ", offeringResult.Errors));
                                return Result.Error("Unable to create one or more subject offerings.");
                            }
                        }

                        totalSectionsCreated++;
                        _logger.LogInformation(
                            "Created class section {SectionName} (ID: {ClassSectionId}) with {SubjectCount} subject offerings",
                            newClassSection.Name, classSectionId, curriculumSubjects.Count());
                    }
                }

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully bulk initialized {TotalSections} class sections for term {TermId}, year level {YearLevel}",
                    totalSectionsCreated, command.academicTermId, command.yearLevel);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk class section initialization");
                // Transaction will rollback automatically on dispose
                return Result.Error("An unexpected error occurred while initializing class sections.");
            }
        }
    }

}
