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
            _dbContext.Update(academicYear);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(academicYear.Id);
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

    public async Task<Result> Delete(AcademicYear academicYear, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.AcademicYears.Remove(academicYear);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database error occurred while deleting academic year {AcademicYearId}.", academicYear.Id.Value);
            return Result.Error("An error occurred while deleting the academic year.");
        }
    }

    public async Task<AcademicYear?> GetActiveAcademicYearAsync(CancellationToken cancellation)
    {
        var today = DateTime.UtcNow.Date;
        return await _dbContext.AcademicYears
            .Include(ay => ay.AcademicTerms)
            .FirstOrDefaultAsync(ay => ay.StartDate.Value <= today && ay.EndDate.Value >= today, cancellation);
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
