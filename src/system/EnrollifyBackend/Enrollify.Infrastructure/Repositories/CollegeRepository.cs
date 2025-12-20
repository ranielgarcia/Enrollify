using Ardalis.Result;
using Enrollify.Application.Colleges;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class CollegeRepository : ICollegeRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<CollegeRepository> _logger;

    public CollegeRepository(EnrollifyDbContext dbContext, ILogger<CollegeRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<CollegeId>> Create(College newCollege, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Colleges.AddAsync(newCollege, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newCollege.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCollegeCodeException(ex))
        {
            _logger.LogError(ex, "Duplicate college code: {CollegeCode}", newCollege.Code.Value);
            return Result.Conflict($"The college code '{newCollege.Code.Value}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsDuplicateCollegeNameException(ex))
        {
            _logger.LogError(ex, "Duplicate college name: {CollegeName}", newCollege.Name);
            return Result.Conflict($"The college name '{newCollege.Name}' is already in use. Please choose a different name.");
        }
    }

    public async Task<Result> Delete(CollegeId collegeId, CancellationToken cancellationToken)
    {
        try
        {
            var college = await _dbContext.Colleges.FirstOrDefaultAsync(c => c.Id == collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an ID of {collegeId.Value} was not found.");
            }

            _dbContext.Colleges.Remove(college);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete college with ID: {CollegeId} due to foreign key constraint", collegeId.Value);
            return Result.Conflict("Cannot delete this college because it is currently in use by one or more rooms or courses. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting college with ID: {CollegeId}", collegeId.Value);
            return Result.Error($"Unable to delete the college with ID {collegeId.Value} due to internal error");
        }
    }

    public async Task<College?> GetById(CollegeId collegeId, CancellationToken cancellationToken)
    {
        var college = await _dbContext.Colleges
            .FirstOrDefaultAsync(c => c.Id == collegeId, cancellationToken);
        return college;
    }

    public async Task<List<College>> ListColleges(CancellationToken cancellationToken = default)
    {
        var colleges = await _dbContext.Colleges
            .Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return colleges;
    }

    public async Task<Result<CollegeId>> Update(College updatedCollege, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Colleges.Update(updatedCollege);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(updatedCollege.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCollegeCodeException(ex))
        {
            _logger.LogError(ex, "Duplicate college code: {CollegeCode}", updatedCollege.Code.Value);
            return Result.Conflict($"The college code '{updatedCollege.Code.Value}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsDuplicateCollegeNameException(ex))
        {
            _logger.LogError(ex, "Duplicate college name: {CollegeName}", updatedCollege.Name);
            return Result.Conflict($"The college name '{updatedCollege.Name}' is already in use. Please choose a different name.");
        }
    }


    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_College") == true;
    }

    private bool IsDuplicateCollegeCodeException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true ||
               ex.InnerException?.Message.Contains("UQ__College__") == true ||
               ex.InnerException?.Message.Contains("Code") == true;
    }

    private bool IsDuplicateCollegeNameException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true ||
               ex.InnerException?.Message.Contains("UQ__College__") == true ||
               ex.InnerException?.Message.Contains("Name") == true;
    }
}
