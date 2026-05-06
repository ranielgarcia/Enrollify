namespace Enrollify.WebAPI.Features.AcademicYearAndTerm.Models;

/// <summary>Shared request model for a single academic term used by both Initiate and Update endpoints.</summary>
public class AcademicTermRequest
{
    public int TermNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
