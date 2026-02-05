using Enrollify.Application.Subjects.DTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Curriculums.DTOs;

public class CurriculumSubjectDTO
{
    public CurriculumSubjectId Id { get; set; }
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

    public SubjectSummaryDTO? Subject { get; set; }

    public IReadOnlyCollection<CurriculumSubjectPrerequisiteDTO> Prerequisites { get; set; } = [];
}
