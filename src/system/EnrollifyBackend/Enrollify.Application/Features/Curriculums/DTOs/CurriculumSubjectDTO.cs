using Enrollify.Application.SharedDTOs;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class CurriculumSubjectDto
{
    public CurriculumSubjectId Id { get; set; }
    public SubjectId SubjectId { get; set; }

    /// <summary>
    /// Which year this subject is typically taken (1-6)
    /// </summary>
    public YearLevel YearLevel { get; set; }

    /// <summary>
    /// Which semester (1 = First, 2 = Second, 3 = Summer)
    /// </summary>
    public TermNumber TermNumber { get; set; }

    /// <summary>
    /// Whether this is an elective slot
    /// </summary>
    public bool IsElective { get; set; }

    public decimal? UnitsOverride { get; set; }

    /// <summary>
    /// Group name for electives (e.g., 'Major Elective', 'Free Elective')
    /// </summary>
    public string? ElectiveGroupName { get; set; }

    public SubjectSummaryDto? Subject { get; set; }

    public IReadOnlyCollection<CurriculumSubjectPrerequisiteDto> Prerequisites { get; set; } = [];
}
