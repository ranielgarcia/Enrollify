using Ardalis.Result;
using Enrollify.Application.Features.Subjects;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<SubjectRepository> _logger;

    public SubjectRepository(EnrollifyDbContext dbContext, ILogger<SubjectRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<SubjectId>> Create(Subject newSubject, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Subjects.AddAsync(newSubject, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newSubject.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCodeException(ex))
        {
            _logger.LogError(ex, "Duplicate subject code: {SubjectCode}", newSubject.Code);
            return Result.Conflict($"The subject code '{newSubject.Code}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsSubjectsUnitsInvalid(ex))
        {
            _logger.LogError(ex, "Invalid subject units: {SubjectCode}", newSubject.Code);
            return Result.Conflict($"The subject unit must be greater than 0 and less than 12 (exclusive).");
        }
    }

    public async Task<Result> Delete(SubjectId id, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (subject == null)
            {
                return Result.NotFound($"Subject with ID {id.Value} not found.");
            }

            _dbContext.Subjects.Remove(subject);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete subject with ID: {SubjectId} due to foreign key constraint", id.Value);
            return Result.Conflict("Cannot delete this subject because it is currently in use. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject with ID: {SubjectId}", id.Value);
            return Result.Error($"Unable to delete the subject with ID {id.Value} due to internal error");
        }
    }

    public async Task<Result<SubjectId>> Update(Subject newSubject, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Subjects.Update(newSubject);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newSubject.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCodeException(ex))
        {
            _logger.LogError(ex, "Duplicate subject code: {SubjectCode}", newSubject.Code);
            return Result.Conflict($"The subject code '{newSubject.Code}' already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsSubjectsUnitsInvalid(ex))
        {
            _logger.LogError(ex, "Invalid subject units: {SubjectCode}", newSubject.Code);
            return Result.Conflict($"The subject unit must be greater than 0 and less than 12 (exclusive).");
        }
    }

    private bool IsDuplicateCodeException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("UQ_Subjects_Code") == true;
    }

    private bool IsSubjectsUnitsInvalid(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("CHK_Subjects_Units_Valid") == true;
    }

    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_Subject") == true;
    }
}
