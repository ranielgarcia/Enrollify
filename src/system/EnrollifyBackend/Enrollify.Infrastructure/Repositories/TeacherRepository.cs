using Ardalis.Result;
using Enrollify.Application.Teachers;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<TeacherRepository> _logger;

    public TeacherRepository(EnrollifyDbContext dbContext, ILogger<TeacherRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<TeacherId>> Create(Teacher newTeacher, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Teachers.AddAsync(newTeacher, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newTeacher.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailException(ex))
        {
            _logger.LogError(ex, "Duplicate email: {Email}", newTeacher.Email);
            return Result.Conflict("Unable to create teacher with the provided email. Please use a different email.");
        }
    }

    public async Task<Result> Delete(TeacherId id, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
            if (teacher == null)
            {
                return Result.NotFound($"Teacher with ID {id.Value} not found");
            }

            _dbContext.Teachers.Remove(teacher);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher with ID: {TeacherId}", id.Value);
            return Result.Error($"Unable to delete the teacher with ID {id.Value} due to internal error");
        }
    }

    public async Task<Result<TeacherId>> Update(Teacher updatedTeacher, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Teachers.Update(updatedTeacher);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(updatedTeacher.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailException(ex))
        {
            _logger.LogError(ex, "Duplicate email: {Email}", updatedTeacher.Email);
            return Result.Conflict("Unable to create teacher with the provided email. Please use a different email.");
        }
    }

    private bool IsDuplicateEmailException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("UQ_Teachers_Email") == true;
    }

}
