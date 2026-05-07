namespace Enrollify.Application.Features.AcademicYearAndTerm.DTOs;

public class AcademicYearTimelineDto
{
    public List<AcademicYearDto> Previous { get; set; } = [];
    public AcademicYearDto? Current { get; set; }
    public List<AcademicYearDto> Future { get; set; } = [];
}
