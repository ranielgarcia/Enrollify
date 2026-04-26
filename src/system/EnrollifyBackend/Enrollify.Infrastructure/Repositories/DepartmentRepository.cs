using Ardalis.Result;
using Enrollify.Application.Features.Departments;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(EnrollifyDbContext dbContext, ILogger<DepartmentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<DepartmentId>> Create(Department newDepartment, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Departments.AddAsync(newDepartment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newDepartment.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCodeInACollegeException(ex))
        {
            _logger.LogError(ex, "Duplicate department code: {DepartmentCode}", newDepartment.Code);
            return Result.Conflict($"The department code '{newDepartment.Code}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsDuplicateNameInACollegeException(ex))
        {
            _logger.LogError(ex, "Duplicate department name: {DepartmentName}", newDepartment.Name);
            return Result.Conflict($"The department name '{newDepartment.Name}' is already in use. Please choose a different name.");
        }
    }

    public async Task<Result> Delete(DepartmentId id, CancellationToken cancellationToken)
    {
        try
        {
            var department = await _dbContext.Departments.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
            if (department == null) return Result.NotFound($"Department with ID {id.Value} not found");

            _dbContext.Departments.Remove(department);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete department with ID: {DepartmentId} due to foreign key constraint", id.Value);
            return Result.Conflict("Cannot delete this department because it is currently in use by one or more courses. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department with ID: {DepartmentId}", id.Value);
            return Result.Error($"Unable to delete the department with ID {id.Value} due to internal error");
        }
    }

    public async Task<Result<DepartmentId>> Update(Department newDepartment, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Departments.Update(newDepartment);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newDepartment.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateCodeInACollegeException(ex))
        {
            _logger.LogError(ex, "Duplicate department code: {DepartmentCode}", newDepartment.Code);
            return Result.Conflict($"The department code '{newDepartment.Code}' is already in use. Please choose a different code.");
        }
        catch (DbUpdateException ex) when (IsDuplicateNameInACollegeException(ex))
        {
            _logger.LogError(ex, "Duplicate department name: {DepartmentName}", newDepartment.Name);
            return Result.Conflict($"The department name '{newDepartment.Name}' is already in use. Please choose a different name.");
        }
    }

    private static bool IsDuplicateCodeInACollegeException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true &&
               ex.InnerException?.Message.Contains("UIdx_Departments_Code_College_IsActive") == true;
    }

    private static bool IsDuplicateNameInACollegeException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true &&
               ex.InnerException?.Message.Contains("UIdx_Departments_Name_College_IsActive") == true;
    }


    private static bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_Department") == true;
    }
}
