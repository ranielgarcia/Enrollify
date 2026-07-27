using Ardalis.Result;
using Enrollify.Application.Features.CourseCurriculumAssignments;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class CourseCurriculumAssignmentRepository : ICourseCurriculumAssignmentRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<CourseCurriculumAssignmentRepository> _logger;

    public CourseCurriculumAssignmentRepository(
        EnrollifyDbContext dbContext,
        ILogger<CourseCurriculumAssignmentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result> BulkCreate(List<CourseCurriculumAssignment> courseCurriculumAssignments, CancellationToken ct)
    {
        try
        {
            _dbContext.CourseCurriculumAssignments.AddRange(courseCurriculumAssignments);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unable to bulk create Course-Curriculum assignments.");
            return Result.Error("An unexpected error occurred while creating Course-Curriculum assignments.");
        }
    }

    public async Task<Result> BulkUpdate (List<CourseCurriculumAssignment> courseCurriculumAssignments, CancellationToken ct)
    {
        try
        {
            _dbContext.CourseCurriculumAssignments.UpdateRange(courseCurriculumAssignments);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to bulk update Course-Curriculum assignments.");
            return Result.Error("An unexpected error occurred while updating Course-Curriculum assignments.");
        }
    }
}
