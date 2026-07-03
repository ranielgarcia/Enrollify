using Enrollify.Application.Features.AcademicYearAndTerm.Models;

namespace Enrollify.Application.Features.AcademicYearAndTerm;

public interface IAcademicYearAndTermRepository
{
    Task<Result<AcademicYear>> Create(AcademicYear academicYear, CancellationToken cancellationToken);
    Task<Result<AcademicYear>> Update(AcademicYear academicYear, CancellationToken cancellationToken);
    Task<Result> Delete(AcademicYear academicYear, CancellationToken cancellationToken);
}
