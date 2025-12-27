using Ardalis.Result;
using Enrollify.Application.Courses;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<CourseRepository> _logger;

    public CourseRepository(EnrollifyDbContext dbContext, ILogger<CourseRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<CourseId>> Create(Course newCourse, CancellationToken cancellationToken)
    {
        try 
        {
            await _dbContext.Courses.AddAsync(newCourse, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newCourse.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCodeInACourseException(ex))
        {
            _logger.LogError(ex, "Duplicate course code: {courseCode}", newCourse.Code);
            return Result.Conflict($"The course code '{newCourse.Code}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsDuplicateNameInACourseException(ex))
        {
            _logger.LogError(ex, "Duplicate course name: {courseName}", newCourse.Name);
            return Result.Conflict($"The course name '{newCourse.Name}' is already in use. Please choose a different name.");
        }
    }

    public async Task<Result> Delete(CourseId id, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (course == null) return Result.NotFound($"Course with ID {id.Value} not found");

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete course with ID: {courseId} due to foreign key constraint", id.Value);
            return Result.Conflict("Cannot delete this course because it is currently in use by one or more courses. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course with ID: {courseId}", id.Value);
            return Result.Error($"Unable to delete the course with ID {id.Value} due to internal error");
        }
    }

    public async Task<Result<CourseId>> Update(Course newCourse, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Courses.Update(newCourse);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newCourse.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCodeInACourseException(ex))
        {
            _logger.LogError(ex, "Duplicate course code: {courseCode}", newCourse.Code);
            return Result.Conflict($"The course code '{newCourse.Code}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsDuplicateNameInACourseException(ex))
        {
            _logger.LogError(ex, "Duplicate course name: {courseName}", newCourse.Name);
            return Result.Conflict($"The course name '{newCourse.Name}' is already in use. Please choose a different name.");
        }
    }

    private bool IsDuplicateCodeInACourseException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true &&
               ex.InnerException?.Message.Contains("UIdx_Course_Code_College_IsActive") == true;
    }

    private bool IsDuplicateNameInACourseException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true &&
               ex.InnerException?.Message.Contains("UIdx_Course_Name_College_IsActive") == true;
    }

    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_course") == true;
    }
}
