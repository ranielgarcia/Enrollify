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
using MediatR;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Enrollify.Application.Features.ClassSections.Commands;

public static class BulkInitializeClassSectionsForAcademicYear
{
    public sealed record TargetCourse(CourseId CourseId, int NumberOfSections);

    public sealed record Command(AcademicTermId AcademicTermId, YearLevel YearLevel, List<TargetCourse> TargetCourses) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
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


        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var targetCourseIds = command.TargetCourses.Select(p => p.CourseId).ToList();

            // Get academic year and term (validated by validator)
            var academicYear = await _academicYearRepository
                .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.AcademicTermId), cancellationToken);
            var academicTerm = academicYear!.AcademicTerms.First(at => at.Id == command.AcademicTermId);

            var intendedCohortEntryYear = GetCohortEntryYear(academicYear, command.YearLevel);

            var getCohortCourseCurriculumResult = await GetCohortCourseCurriculumAssignments(targetCourseIds, intendedCohortEntryYear, cancellationToken);
            if (!getCohortCourseCurriculumResult.IsSuccess)
            {
                return Result.Error(string.Join("; ", getCohortCourseCurriculumResult.Errors));
            }

            var (cohortAcademicYear, cohortCourseCurriculumAssignments) = getCohortCourseCurriculumResult.Value;

            var courseCurriculumAssignmentsByCourseId = cohortCourseCurriculumAssignments.ToDictionary(x => x.CourseId, x => x);

            var existingSections = await _classSectionReadRepository.ListAsync(
                new GetExistingClassSectionsByCourseYearLevelAndTerm(
                    new List<YearLevel> { command.YearLevel },
                    targetCourseIds,
                    new List<AcademicTermId> { command.AcademicTermId }),
                cancellationToken);
            var existingSectionsByCourseId = existingSections.GroupBy(s => s.CourseId).ToDictionary(g => g.Key, g => g.ToList());

            // H5: Pre-validate that every course has subjects for the given year level and term before opening the transaction.
            // Fails fast without any DB writes if any course's curriculum has no subjects for this year/term.
            foreach (var targetCourse in command.TargetCourses)
            {
                var assignment = courseCurriculumAssignmentsByCourseId[targetCourse.CourseId];
                if (!assignment.Curriculum!.GetSubjectsByYearAndTerm(command.YearLevel, academicTerm.TermNumber).Any())
                {
                    _logger.LogWarning(
                        "No curriculum subjects found for course {CourseId}, year level {YearLevel}, term {TermNumber}",
                        targetCourse.CourseId, command.YearLevel, academicTerm.TermNumber);
                    return Result.Error($"No curriculum subjects found for course '{assignment.Course!.Code.Value}', year level {command.YearLevel}, term {academicTerm.TermNumber}.");
                }
            }

            // Begin transaction to ensure all database operations succeed or fail together
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var totalSectionsCreated = 0;

                foreach (var targetCourse in command.TargetCourses)
                {
                    // H3: Direct property access — validator guarantees assignment exists;
                    // dictionary indexer throws rather than returning null, so ?. operators are redundant.
                    var courseCurriculumAssignment = courseCurriculumAssignmentsByCourseId[targetCourse.CourseId];
                    var curriculum = courseCurriculumAssignment.Curriculum!;
                    var course = courseCurriculumAssignment.Course!;

                    // H4: Materialize once so .Count is a cheap property read and the inner foreach
                    // doesn't re-evaluate the IEnumerable on every iteration.
                    var curriculumSubjects = curriculum
                        .GetSubjectsByYearAndTerm(command.YearLevel, academicTerm.TermNumber)
                        .ToList();

                    // Get existing sections to determine starting section code
                    var existingClassSections = existingSectionsByCourseId.GetValueOrDefault(targetCourse.CourseId);
                    var lastExistingClassSectionCode = existingClassSections?
                        .OrderByDescending(cs => cs.SectionCode)
                        .FirstOrDefault()?
                        .SectionCode;

                    // Create the requested number of sections
                    for (var i = 0; i < targetCourse.NumberOfSections; i++)
                    {
                        // Generate next section code (A, B, C, etc.)
                        var sectionCode = lastExistingClassSectionCode.GetNextSectionCode();
                        lastExistingClassSectionCode = sectionCode;

                        // Create class section with auto-generated name
                        var newClassSection = new ClassSection(new ClassSectionForCreation
                        {
                            Name = course.Code.Value,
                            IntendedYearLevel = command.YearLevel,
                            CourseId = targetCourse.CourseId,
                            CurriculumId = curriculum.Id,
                            AcademicTermId = command.AcademicTermId,
                            AdviserId = null, // No adviser assigned during bulk initialization
                            SectionCode = sectionCode,
                            CohortAcademicYearId = cohortAcademicYear.Id,
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
                    totalSectionsCreated, command.AcademicTermId, command.YearLevel);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk class section initialization");
                // Transaction will rollback automatically on dispose
                return Result.Error("An unexpected error occurred while initializing class sections.");
            }
        }


        //> Creating class sections for BSCS in AY 2025-2026:

        //| Year Level | Entry AY     | Lookup in `CourseCurriculumAssignments`  |
        //| ---------- | ------------ | ---------------------------------------- |
        //| Year 1     | AY 2025-2026 | (BSCS, AY 2025-2026) → Curriculum 2025-A |
        //| Year 2     | AY 2024-2025 | (BSCS, AY 2024-2025) → Curriculum 2024-A |
        //| Year 3     | AY 2023-2024 | (BSCS, AY 2023-2024) → Curriculum 2023-X |
        //| Year 4     | AY 2022-2023 | (BSCS, AY 2022-2023) → Curriculum 2023-X |

        //The entry AY derivation lives in application code: find the AcademicYear with `StartDate.Year = currentAY.StartDate.Year - (YearLevel - 1)`.

        private async Task<Result<(AcademicYear cohortAcademicYear, List<CourseCurriculumAssignment> cohortCourseCurriculumAssignments)>>
            GetCohortCourseCurriculumAssignments
            (List<CourseId> courseIds, Year cohortEntryYear, CancellationToken ct)
        {
            var academicYear = await _academicYearRepository
                .FirstOrDefaultAsync(new GetAcademicYearByStartDateYearSpec(cohortEntryYear), CancellationToken.None);

            // Limitation: Requires all historical curriculum data for the cohort's entry academic year.
            // If the academic year or curriculum assignments are missing, bulk initialization fails.
            // Admins must pre-populate all required historical data before creating sections for higher year levels.
            if (academicYear == null) return Result.Error("Academic year for the given cohort entry year not found.");

            var courseCurriculumAssignments = await _courseCurriculumAssignmentsReadRepository
                .ListAsync(new GetAllCourseCurriculumAssignmentsForCoursesByAcademicYearIdSpec(courseIds, academicYear.Id), ct);

            return Result.Success((academicYear, courseCurriculumAssignments));
        }

        private Year GetCohortEntryYear(AcademicYear classSectionAcademicYear, YearLevel yearLevel)
            => Year.From(classSectionAcademicYear.StartDate.Value.Year - (yearLevel.Value - 1));
    }

}
