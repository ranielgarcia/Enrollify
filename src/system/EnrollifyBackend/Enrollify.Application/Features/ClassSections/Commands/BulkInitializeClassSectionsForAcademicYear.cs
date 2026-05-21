using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Extensions;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Features.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSections.Commands;

public static class BulkInitializeClassSectionsForAcademicYear
{
    public sealed record Payload(CourseId courseId, int numberOfSections);

    public sealed record Command(AcademicTermId academicTermId, YearLevel yearLevel, List<Payload> requestPayload) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IReadRepository<AcademicYear> _academicYearRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;
        private readonly IReadRepository<CourseCurriculumAssignment> _courseCurriculumAssignmentsReadRepository;
        private readonly IClassSectionRepository _classSectionRepository;
        private readonly IClassSectionSubjectOfferingRepository _classSectionSubjectOfferingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<AcademicYear> academicYearRepository,
            IReadRepository<ClassSection> classSectionReadRepository,
            IReadRepository<CourseCurriculumAssignment> courseCurriculumAssignmentsReadRepository,
            IClassSectionRepository classSectionRepository,
            IClassSectionSubjectOfferingRepository classSectionSubjectOfferingRepository,
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _academicYearRepository = academicYearRepository;
            _classSectionReadRepository = classSectionReadRepository;
            _courseCurriculumAssignmentsReadRepository = courseCurriculumAssignmentsReadRepository;
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

            var courseCurriculumAssignments = await _courseCurriculumAssignmentsReadRepository.ListAsync(new GetAllCourseCurriculumAssignmentsByAcademicYearIdSpec(academicYear.Id), cancellationToken);
            var courseCurriculumAssignmentsByCourseId = courseCurriculumAssignments.ToDictionary(x => x.CourseId, x => x);

            var payloadCourseIds = command.requestPayload.Select(p => p.courseId).ToList();
            var existingSections = await _classSectionReadRepository.ListAsync(
                new GetExistingClassSectionsByCourseYearLevelAndTerm(
                    new List<YearLevel> { command.yearLevel },
                    payloadCourseIds,
                    new List<AcademicTermId> { command.academicTermId }),
                cancellationToken);
            var existingSectionsByCourseId = existingSections.GroupBy(s => s.CourseId).ToDictionary(g => g.Key, g => g.ToList());

            // H5: Pre-validate that every course has subjects for the given year level and term before opening the transaction.
            // Fails fast without any DB writes if any course's curriculum has no subjects for this year/term.
            foreach (var payload in command.requestPayload)
            {
                var assignment = courseCurriculumAssignmentsByCourseId[payload.courseId];
                if (!assignment.Curriculum!.GetSubjectsByYearAndTerm(command.yearLevel, academicTerm.TermNumber).Any())
                {
                    _logger.LogWarning(
                        "No curriculum subjects found for course {CourseId}, year level {YearLevel}, term {TermNumber}",
                        payload.courseId, command.yearLevel, academicTerm.TermNumber);
                    return Result.Error($"No curriculum subjects found for course '{assignment.Course!.Code.Value}', year level {command.yearLevel}, term {academicTerm.TermNumber}.");
                }
            }

            // Begin transaction to ensure all database operations succeed or fail together
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var totalSectionsCreated = 0;

                foreach (var payload in command.requestPayload)
                {
                    // H3: Direct property access — validator guarantees assignment exists;
                    // dictionary indexer throws rather than returning null, so ?. operators are redundant.
                    var courseCurriculumAssignment = courseCurriculumAssignmentsByCourseId[payload.courseId];
                    var curriculum = courseCurriculumAssignment.Curriculum!;
                    var course = courseCurriculumAssignment.Course!;

                    // H4: Materialize once so .Count is a cheap property read and the inner foreach
                    // doesn't re-evaluate the IEnumerable on every iteration.
                    var curriculumSubjects = curriculum
                        .GetSubjectsByYearAndTerm(command.yearLevel, academicTerm.TermNumber)
                        .ToList();

                    // Get existing sections to determine starting section code
                    var existingClassSections = existingSectionsByCourseId.GetValueOrDefault(payload.courseId);
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
                            newClassSection.Name, classSectionId, curriculumSubjects.Count);
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
