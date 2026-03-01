using Ardalis.Result;
using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Specifications;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;
using Semester = int;
using Year = int;

namespace Enrollify.Application.Curriculums.Features;

public class SaveCurriculumContent
{
    public class SubjectInCurriculum
    {
        public SubjectCode Code { get; set; }
        public SubjectCode[] Prerequisites { get; set; } = [];
    }

    public sealed record Command(CurriculumId CurriculumId, Dictionary<Year, Dictionary<Semester, SubjectInCurriculum[]>> SubjectsGrid) : ICommand<Result<CurriculumDTO>>;

    public sealed class Handler : ICommandHandler<Command, Result<CurriculumDTO>>
    {
        private readonly IReadRepository<Curriculum> _readRepository;
        private readonly IReadRepository<Subject> _subjectReadRepository;
        private readonly ICurriculumRepository _curriculumRepository;

        public Handler(
            IReadRepository<Curriculum> readRepository, 
            IReadRepository<Subject> subjectReadRepository,
            ICurriculumRepository curriculumRepository)
        {
            _readRepository = readRepository;
            _subjectReadRepository = subjectReadRepository;
            _curriculumRepository = curriculumRepository;
        }

        public async ValueTask<Result<CurriculumDTO>> Handle(Command command, CancellationToken cancellationToken)
        {
            var spec = new GetCurriculumWithSubjectsByIdSpec(command.CurriculumId);
            var curriculum = await _readRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (curriculum == null)
            {
                return Result.Invalid(new ValidationError($"Curriculum with an id of {command.CurriculumId.Value} not found"));
            }

            // Collect all subject codes from the grid (including prerequisites)
            var allSubjectCodes = command.SubjectsGrid.Values
                .SelectMany(semesters => semesters.Values)
                .SelectMany(subjects => subjects)
                .SelectMany(subject => subject.Prerequisites.Prepend(subject.Code))
                .Distinct()
                .ToList();

            // Get only the main subject codes (not prerequisites) for determining what should exist
            var gridSubjectCodes = command.SubjectsGrid.Values
                .SelectMany(semesters => semesters.Values)
                .SelectMany(subjects => subjects)
                .Select(subject => subject.Code)
                .Distinct()
                .ToHashSet();

            var allSubjects = await _subjectReadRepository.ListAsync(new ListSubjectsByCodesSpec(allSubjectCodes), cancellationToken);

            // Create lookup from SubjectCode to Subject
            var subjectsByCode = allSubjects.ToDictionary(s => s.Code, s => s);

            // Validate all subject codes exist
            var missingCodes = allSubjectCodes
                .Where(code => !subjectsByCode.ContainsKey(code))
                .Select(code => code.Value)
                .ToList();

            if (missingCodes.Count > 0)
            {
                return Result.Invalid(new ValidationError($"Subjects with codes {string.Join(", ", missingCodes)} not found"));
            }

            // Validate prerequisites - build a lookup of subject code to (year, semester)
            var subjectPositions = command.SubjectsGrid
                .SelectMany(year => year.Value.SelectMany(semester => 
                    semester.Value.Select(subject => (Code: subject.Code, Year: year.Key, Semester: semester.Key))))
                .ToDictionary(x => x.Code, x => (x.Year, x.Semester));

            // Build reverse lookup: prerequisite code -> list of subjects that depend on it
            var prerequisiteDependents = command.SubjectsGrid.Values
                .SelectMany(semesters => semesters.Values)
                .SelectMany(subjects => subjects)
                .SelectMany(subject => subject.Prerequisites.Select(prereq => (Prerequisite: prereq, Dependent: subject.Code)))
                .GroupBy(x => x.Prerequisite)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Dependent).Distinct().ToList());

            // Validate each subject's prerequisites
            var prerequisiteErrors = new List<string>();
            var reportedMissingPrereqs = new HashSet<SubjectCode>();
            foreach (var year in command.SubjectsGrid)
            {
                foreach (var semester in year.Value)
                {
                    foreach (var subjectInCurriculum in semester.Value)
                    {
                        var subjectCode = subjectInCurriculum.Code;
                        var subjectYear = year.Key;
                        var subjectSemester = semester.Key;
                        var seenPrereqs = new HashSet<SubjectCode>();

                        foreach (var prereqCode in subjectInCurriculum.Prerequisites)
                        {
                            // Val: Subject cannot be its own prerequisite
                            if (prereqCode == subjectCode)
                            {
                                prerequisiteErrors.Add($"Subject '{subjectCode.Value}' cannot be its own prerequisite");
                                continue;
                            }

                            // Val: No duplicate prerequisites
                            if (!seenPrereqs.Add(prereqCode))
                            {
                                prerequisiteErrors.Add($"Duplicate prerequisite '{prereqCode.Value}' for subject '{subjectCode.Value}'");
                                continue;
                            }

                            // Val: Prerequisite must be in the grid
                            if (!subjectPositions.TryGetValue(prereqCode, out var prereqPosition))
                            {
                                if (reportedMissingPrereqs.Add(prereqCode))
                                {
                                    var dependents = prerequisiteDependents.TryGetValue(prereqCode, out var deps)
                                        ? deps.Select(d => d.Value)
                                        : [subjectCode.Value];
                                    prerequisiteErrors.Add($"Subject '{prereqCode.Value}' is not in the curriculum grid but is listed as a prerequisite for {string.Join(", ", dependents.Select(d => $"'{d}'"))}. Either add '{prereqCode.Value}' to the grid or remove it as a prerequisite.");
                                }
                                continue;
                            }

                            var prereqYear = prereqPosition.Year;
                            var prereqSemester = prereqPosition.Semester;

                            // Val: Prerequisites cannot be from future years
                            if (prereqYear > subjectYear)
                            {
                                prerequisiteErrors.Add($"Prerequisite '{prereqCode.Value}' (Year {prereqYear}, Sem {prereqSemester}) cannot be from a future year for subject '{subjectCode.Value}' (Year {subjectYear}, Sem {subjectSemester})");
                                continue;
                            }

                            // Val: Prerequisites cannot be from the same year with same or future semester
                            if (prereqYear == subjectYear && prereqSemester >= subjectSemester)
                            {
                                prerequisiteErrors.Add($"Prerequisite '{prereqCode.Value}' (Year {prereqYear}, Sem {prereqSemester}) must be from an earlier semester for subject '{subjectCode.Value}' (Year {subjectYear}, Sem {subjectSemester})");
                                continue;
                            }
                        }
                    }
                }
            }

            if (prerequisiteErrors.Count > 0)
            {
                return Result.Invalid(prerequisiteErrors.Select(e => new ValidationError(e)).ToArray());
            }

            // Get SubjectIds that should be in the curriculum
            var gridSubjectIds = gridSubjectCodes
                .Select(code => subjectsByCode[code].Id)
                .ToHashSet();

            // Remove subjects that are in the curriculum but not in the grid
            var existingSubjects = curriculum.CurriculumSubjects.Where(cs => cs.IsActive).ToList();
            foreach (var existingSubject in existingSubjects)
            {
                if (!gridSubjectIds.Contains(existingSubject.SubjectId))
                {
                    curriculum.RemoveSubject(existingSubject.SubjectId);
                }
            }

            // Track CurriculumSubject by SubjectCode
            var curriculumSubjectsLookup = new Dictionary<SubjectCode, CurriculumSubject>();

            // Add new subjects or update existing ones
            foreach (var year in command.SubjectsGrid.OrderBy(kvp => kvp.Key))
            {
                foreach (var semester in year.Value.OrderBy(kvp => kvp.Key))
                {
                    foreach (var subjectInCurriculum in semester.Value)
                    {
                        var subject = subjectsByCode[subjectInCurriculum.Code];

                        // Try to get existing curriculum subject
                        var curriculumSubject = curriculum.GetCurriculumSubject(subject.Id);

                        if (curriculumSubject != null)
                        {
                            // Update existing subject's year/semester if changed
                            curriculumSubject.UpdateYearLevel(year.Key);
                            curriculumSubject.UpdateTermNumber(semester.Key);
                            curriculumSubjectsLookup[subjectInCurriculum.Code] = curriculumSubject;
                        }
                        else
                        {
                            // Add new subject
                            curriculumSubject = curriculum.AddSubject(subject.Id, year.Key, semester.Key, isElective: false, electiveGroupName: null);
                            if (curriculumSubject != null)
                            {
                                curriculumSubjectsLookup[subjectInCurriculum.Code] = curriculumSubject;
                            }
                        }
                    }
                }
            }

            // Save the curriculum subjects first to get their database-generated IDs
            // This is required because CurriculumSubjectPrerequisite needs valid CurriculumSubjectIds
            var saveSubjectsResult = await _curriculumRepository.UpdateCurriculum(curriculum, cancellationToken);
            if (!saveSubjectsResult.IsSuccess)
            {
                return Result.Invalid(saveSubjectsResult.ValidationErrors);
            }

            // Build expected prerequisites map: SubjectCode -> Set of prerequisite SubjectCodes
            var expectedPrerequisites = command.SubjectsGrid.Values
                .SelectMany(semesters => semesters.Values)
                .SelectMany(subjects => subjects)
                .ToDictionary(
                    s => s.Code,
                    s => s.Prerequisites.ToHashSet());

            // Sync prerequisites for each subject
            foreach (var year in command.SubjectsGrid)
            {
                foreach (var semester in year.Value)
                {
                    foreach (var subjectInCurriculum in semester.Value)
                    {
                        var subject = subjectsByCode[subjectInCurriculum.Code];
                        var curriculumSubject = curriculumSubjectsLookup.TryGetValue(subjectInCurriculum.Code, out var cs)
                            ? cs
                            : curriculum.GetCurriculumSubject(subject.Id);

                        if (curriculumSubject == null)
                        {
                            return Result.Invalid(new ValidationError($"CurriculumSubject for code {subjectInCurriculum.Code.Value} was not found"));
                        }

                        var expectedPrereqCodes = expectedPrerequisites.GetValueOrDefault(subjectInCurriculum.Code) ?? [];

                        // Get expected prerequisite CurriculumSubjectIds
                        var expectedPrereqIds = new HashSet<CurriculumSubjectId>();
                        foreach (var prereqCode in expectedPrereqCodes)
                        {
                            var prereqSubject = subjectsByCode[prereqCode];
                            var prereqCurriculumSubject = curriculumSubjectsLookup.TryGetValue(prereqCode, out var pcs)
                                ? pcs
                                : curriculum.GetCurriculumSubject(prereqSubject.Id);

                            if (prereqCurriculumSubject == null)
                            {
                                return Result.Invalid(new ValidationError($"Prerequisite with code {prereqCode.Value} not found in the curriculum"));
                            }

                            expectedPrereqIds.Add(prereqCurriculumSubject.Id);
                        }

                        // Remove prerequisites that are no longer in the grid
                        var existingPrerequisites = curriculumSubject.Prerequisites.Where(p => p.IsActive).ToList();
                        foreach (var existingPrereq in existingPrerequisites)
                        {
                            if (!expectedPrereqIds.Contains(existingPrereq.PrerequisiteCurriculumSubjectId))
                            {
                                curriculumSubject.RemovePrerequisite(existingPrereq.PrerequisiteCurriculumSubjectId);
                            }
                        }

                        // Add new prerequisites
                        foreach (var prereqId in expectedPrereqIds)
                        {
                            curriculumSubject.AddPrerequisite(prereqId, minimumGrade: null, addedBy: curriculum.CreatedBy);
                        }
                    }
                }
            }

            // Save the prerequisites
            var updateResult = await _curriculumRepository.UpdateCurriculum(curriculum, cancellationToken);

            return updateResult.IsSuccess ? Result.Success(CurriculumDTO.FromEntity(curriculum)) : Result.Invalid(updateResult.ValidationErrors);
        }
    }
}
