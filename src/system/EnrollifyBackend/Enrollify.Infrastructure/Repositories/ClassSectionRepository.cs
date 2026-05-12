using Ardalis.Result;
using Enrollify.Application.Features.ClassSections;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionRepository : IClassSectionRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<ClassSectionRepository> _logger;

    public ClassSectionRepository(EnrollifyDbContext dbContext, ILogger<ClassSectionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<ClassSectionId>> Create(ClassSection newClassSection, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.ClassSections.AddAsync(newClassSection, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newClassSection.Id);
        }
        catch (DbUpdateException ex) when (IsInvalidYearLevel(ex))
        {
            _logger.LogError(ex, "Invalid year level {YearLevel}, it must be between 1 and 6", newClassSection.YearLevel);
            return Result.Conflict($"The year level '{newClassSection.YearLevel}' is invalid. It must be between 1 and 6.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new class section {@ClassSection}", newClassSection);
            return Result.Error("Unable to create the new class section due to internal error");
        }
    }

    public async Task<Result> Delete(ClassSectionId classSectionId, CancellationToken cancellationToken)
    {
        try
        {
            var classSection = await _dbContext.ClassSections.FirstOrDefaultAsync(cs => cs.Id == classSectionId, cancellationToken);

            if (classSection == null)
            {
                return Result.NotFound($"Class Section with an ID of {classSectionId.Value} was not found.");
            }
            _dbContext.ClassSections.Remove(classSection);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete class section with ID: {ClassSectionId} due to foreign key constraint", classSectionId.Value);
            return Result.Conflict("Cannot delete this class section because it is currently in use by one or more entities. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class section with ID: {ClassSectionId}", classSectionId.Value);
            return Result.Error($"Unable to delete the class section with ID {classSectionId.Value} due to internal error");
        }
    }

    public async Task<Result<ClassSectionId>> Update(ClassSection updatedClassSection, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.ClassSections.Update(updatedClassSection);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(updatedClassSection.Id);
        }
        catch (DbUpdateException ex) when (IsInvalidYearLevel(ex))
        {
            _logger.LogError(ex, "Invalid year level {YearLevel}, it must be between 1 and 6", updatedClassSection.YearLevel.Value);
            return Result.Conflict($"The year level '{updatedClassSection.YearLevel}' is invalid. It must be between 1 and 6.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class section with ID: {ClassSectionId}", updatedClassSection.Id.Value);
            return Result.Error($"Unable to update the class section with ID {updatedClassSection.Id.Value} due to internal error");
        }
    }

    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_ClassSection") == true;
    }

    private bool IsInvalidYearLevel(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("CHK_ClassSections_YearLevel_Valid") == true;
    }
}
