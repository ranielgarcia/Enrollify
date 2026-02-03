using Ardalis.Result;
using Enrollify.Application.Curriculums.Specifications;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;
using static Enrollify.Application.Curriculums.Features.SaveCurriculumContent;
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

    public sealed record Command(CurriculumId CurriculumId, Dictionary<Year, Dictionary<Semester, SubjectInCurriculum[]>> SubjectsGrid) : ICommand<Result<CurriculumId>>;

    public sealed class Handler : ICommandHandler<Command, Result<CurriculumId>>
    {
        private readonly IReadRepository<Curriculum> _readRepository;
        private readonly IReadRepository<Subject> _subjectReadRepository;

        public Handler(IReadRepository<Curriculum> readRepository, IReadRepository<Subject> subjectReadRepository)
        {
            _readRepository = readRepository;
            _subjectReadRepository = subjectReadRepository;
        }

        public async ValueTask<Result<CurriculumId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var spec = new GetCurriculumWithSubjectsByIdSpec(command.CurriculumId);
            var curriculum = await _readRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (curriculum == null)
            {
                return Result.Invalid(new ValidationError($"Curriculum with an id of {command.CurriculumId.Value} not found"));
            }

            var allSubjectCodes = command.SubjectsGrid.Values
                .SelectMany(semesters => semesters.Values)
                .SelectMany(subjects => subjects)
                .SelectMany(subject => subject.Prerequisites.Prepend(subject.Code))
                .Distinct()
                .ToList();

            var allSubjects = await _subjectReadRepository.ListAsync(new ListSubjectsByCodesSpec(allSubjectCodes), cancellationToken);

            // Track CurriculumSubject by SubjectCode using object references (not IDs)
            var curriculumSubjectsLookup = new Dictionary<SubjectCode, CurriculumSubject>();

            // First pass: Add all subjects to the curriculum
            foreach (var year in command.SubjectsGrid.OrderBy(kvp => kvp.Key))
            {
                foreach (var semester in year.Value.OrderBy(kvp => kvp.Key))
                {
                    foreach (var subjectInCurriculum in semester.Value)
                    {
                        var subject = allSubjects.FirstOrDefault(s => s.Code == subjectInCurriculum.Code);
                        if (subject == null)
                        {
                            throw new InvalidOperationException($"Subject with code {subjectInCurriculum.Code.Value} not found");
                        }

                        var curriculumSubject = curriculum.AddSubject(subject.Id, year.Key, semester.Key, isElective: false, electiveGroupName: null);
                        if (curriculumSubject != null)
                        {
                            curriculumSubjectsLookup[subjectInCurriculum.Code] = curriculumSubject;
                        }
                    }
                }
            }

            // Second pass: Add prerequisites using object references
            foreach (var year in command.SubjectsGrid)
            {
                foreach (var semester in year.Value)
                {
                    foreach (var subjectInCurriculum in semester.Value)
                    {
                        if (subjectInCurriculum.Prerequisites.Length == 0)
                            continue;

                        if (!curriculumSubjectsLookup.TryGetValue(subjectInCurriculum.Code, out var curriculumSubject))
                            continue;

                        foreach (var prerequisiteCode in subjectInCurriculum.Prerequisites)
                        {
                            if (!curriculumSubjectsLookup.TryGetValue(prerequisiteCode, out var prerequisiteCurriculumSubject))
                            {
                                throw new InvalidOperationException($"Prerequisite with code {prerequisiteCode.Value} not found in the curriculum");
                            }

                            // TODO: Replace with actual user ID from context
                            curriculumSubject.AddPrerequisite(prerequisiteCurriculumSubject, minimumGrade: null, addedBy: curriculum.CreatedBy);
                        }
                    }
                }
            }

            return Result.Success(command.CurriculumId);
        }
    }
}
