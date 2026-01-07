using Ardalis.Result;
using Enrollify.Application.Curriculums;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

internal class CurriculumRepository : ICurriculumRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<CurriculumRepository> _logger;

    public CurriculumRepository(EnrollifyDbContext dbContext, ILogger<CurriculumRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Curriculum>> CreateDraftCurricula(Curriculum newCurriculum, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Curriculums.AddAsync(newCurriculum, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newCurriculum);
        }
        catch (DbUpdateException ex) when (IsEffectiveYearInvalidException(ex))
        {
            _logger.LogError(ex, "Failed to create curricula due to invalid effective year: {EffectiveYear}", newCurriculum.EffectiveYear);
            return Result.Invalid(new ValidationError(
                identifier: "EffectiveYear",
                errorMessage: $"The effective year '{newCurriculum.EffectiveYear}' is invalid. Please enter a year greater than or equal to 2000."));
        }
    }

    private bool IsEffectiveYearInvalidException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("CHK_Curricula_EffectiveYear_Valid") == true;
    }
}
