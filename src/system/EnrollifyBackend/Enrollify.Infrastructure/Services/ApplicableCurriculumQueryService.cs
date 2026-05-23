using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.Services;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Services;

public class ApplicableCurriculumQueryService : IApplicableCurriculumQueryService
{
    private readonly EnrollifyDbContext _dbContext;

    public ApplicableCurriculumQueryService(EnrollifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Curriculum>> GetApplicableCurriculumsForAcademicYearStartDateAsync(
        AcademicYearStartDate academicYearStartDate,
        CancellationToken cancellationToken)
    {
        var academicYearStartYear = academicYearStartDate.Value.Year;
        var activeCurriculumStatus = CurriculumStatusEnum.Active.Value;

        return await _dbContext.Curriculums
            .FromSqlInterpolated($"""
                SELECT
                    Id,
                    CourseId,
                    EffectiveYear,
                    Version,
                    StatusId,
                    Description,
                    ApprovedDate,
                    CreatedAt,
                    CreatedBy,
                    UpdatedAt,
                    UpdatedBy,
                    DeletedAt,
                    DeletedBy,
                    IsActive
                FROM (
                    SELECT
                        c.*,
                        ROW_NUMBER() OVER (
                            PARTITION BY c.CourseId
                            ORDER BY c.EffectiveYear DESC, c.Version DESC, c.ApprovedDate DESC, c.Id DESC
                        ) AS rn
                    FROM Curriculums c
                    WHERE c.IsActive = 1
                      AND c.StatusId = {activeCurriculumStatus}
                      AND c.EffectiveYear <= {academicYearStartYear}
                ) ranked
                WHERE rn = 1
                """)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
