using Enrollify.Application.Subjects.DTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Curriculums.DTOs;

public class CurriculumSubjectDTO
{
    public CurriculumSubjectId Id { get; set; }
    public CurriculumId CurriculumId { get; set; }
    public SubjectId SubjectId { get; set; }

    /// <summary>
    /// Which year this subject is typically taken (1-6)
    /// </summary>
    public int YearLevel { get; set; }

    /// <summary>
    /// Which semester (1 = First, 2 = Second, 3 = Summer)
    /// </summary>
    public int TermNumber { get; set; }

    /// <summary>
    /// Whether this is an elective slot
    /// </summary>
    public bool IsElective { get; set; }

    /// <summary>
    /// Group name for electives (e.g., 'Major Elective', 'Free Elective')
    /// </summary>
    public string? ElectiveGroupName { get; set; }

    public SubjectDTO? Subject { get; set; }

    public IReadOnlyCollection<CurriculumSubjectPrerequisiteDTO> Prerequisites { get; set; } = [];

    public static CurriculumSubjectDTO FromEntity(CurriculumSubject curriculumSubject)
    {
        return new CurriculumSubjectDTO
        {
            Id = curriculumSubject.Id,
            CurriculumId = curriculumSubject.CurriculumId,
            SubjectId = curriculumSubject.SubjectId,
            YearLevel = curriculumSubject.YearLevel,
            TermNumber = curriculumSubject.TermNumber,
            IsElective = curriculumSubject.IsElective,
            ElectiveGroupName = curriculumSubject.ElectiveGroupName,
            Subject = curriculumSubject.Subject != null ? SubjectDTO.FromEntity(curriculumSubject.Subject) : null,
            Prerequisites = curriculumSubject.Prerequisites
                .Where(p => p.IsActive)
                .Select(CurriculumSubjectPrerequisiteDTO.FromEntity)
                .ToList(),
        };
    }
}
