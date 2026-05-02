using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Core;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class AcademicYearAndTermRepository : IAcademicYearAndTermRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<AcademicYearAndTermRepository> _logger;

    public AcademicYearAndTermRepository(EnrollifyDbContext dbContext, ILogger<AcademicYearAndTermRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }


    public async Task<Result<AcademicYearId>> Create(AcademicYear academicYear, CancellationToken cancellationToken)
    {
        try
        {
            var existingAcademicYear = await _dbContext.AcademicYears
                .FirstOrDefaultAsync(ay =>
                    ay.StartDate.Value < academicYear.EndDate.Value
                    && ay.EndDate.Value > academicYear.StartDate.Value,
                cancellationToken);

            if (existingAcademicYear is not null)
            {
                _logger.LogWarning(
                    "Academic year creation conflict: proposed range {Start}-{End} overlaps with existing year {ExistingId}.",
                    academicYear.StartDate.Value, academicYear.EndDate.Value, existingAcademicYear.Id);
                return Result.Conflict("The specified academic year overlaps with an existing academic year. Please choose a different date range.");
            }

            await _dbContext.AcademicYears.AddAsync(academicYear, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(academicYear.Id);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogError(ex, "A unique constraint violation occurred while creating an academic year.");
            return Result.Conflict("The specified academic year overlaps with an existing academic year. Please choose a different date range.");
        }
        catch (DbUpdateException ex) when (IsCheckAcademicYearInvalidConstraintViolation(ex))
        {
            _logger.LogError(ex, "A check constraint violation occurred while creating an academic year. Year cannot be earlier than 2000.");
            return Result.Conflict("The specified academic year is invalid. Year cannot be earlier than 2000.");
        }
    }

    public async Task<Result<AcademicYearId>> Update(AcademicYear academicYear, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _dbContext.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Id == academicYear.Id, cancellationToken);

            if (existing is null)
            {
                return Result.NotFound("The specified academic year was not found.");
            }

            var overlaps = await _dbContext.AcademicYears
                .AnyAsync(ay =>
                    ay.Id != academicYear.Id
                    && ay.StartDate.Value < academicYear.EndDate.Value
                    && ay.EndDate.Value > academicYear.StartDate.Value,
                cancellationToken);

            if (overlaps)
            {
                return Result.Conflict("The specified academic year overlaps with an existing academic year. Please choose a different date range.");
            }

            existing.UpdateStartAndEndYear(academicYear.StartDate, academicYear.EndDate);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(existing.Id);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogError(ex, "A unique constraint violation occurred while updating an academic year.");
            return Result.Conflict("The specified academic year overlaps with an existing academic year. Please choose a different date range.");
        }
        catch (DbUpdateException ex) when (IsCheckAcademicYearInvalidConstraintViolation(ex))
        {
            _logger.LogError(ex, "A check constraint violation occurred while updating an academic year. Year cannot be earlier than 2000.");
            return Result.Conflict("The specified academic year is invalid. Year cannot be earlier than 2000.");
        }
    }

    public async Task<Result> Delete(AcademicYearId id, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _dbContext.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Id == id, cancellationToken);

            if (existing is null)
            {
                return Result.NotFound("The specified academic year was not found.");
            }

            _dbContext.AcademicYears.Remove(existing);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database error occurred while deleting academic year {AcademicYearId}.", id);
            return Result.Error("An error occurred while deleting the academic year.");
        }
    }


    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("UQ_AcademicYears_StartEnd") == true;
    }

    private static bool IsCheckAcademicYearInvalidConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("CHK_AcademicYears_Valid") == true;
    }

}
