using Ardalis.Result;
using Enrollify.Application.Features.CourseCurriculumAssignments;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Infrastructure.Data;
using Enrollify.SharedKernel;

namespace Enrollify.Infrastructure.Repositories;

public class CourseCurriculumAssignmentRepository : ICourseCurriculumAssignmentRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseCurriculumAssignmentRepository> _logger;

    public CourseCurriculumAssignmentRepository(
        EnrollifyDbContext dbContext,
        IUnitOfWork unitOfWork,
        ILogger<CourseCurriculumAssignmentRepository> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<Result> BulkCreate(List<CourseCurriculumAssignment> courseCurriculumAssignments, CancellationToken ct)
    {
        // Begin transaction to ensure all database operations succeed or fail together
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            _dbContext.CourseCurriculumAssignments.AddRange(courseCurriculumAssignments);
            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unable to bulk create Course-Curriculum assignments.");
            // Transaction will rollback automatically on dispose
            return Result.Error("An unexecpted error occured while creating Course-Curriculum assignments.");
        }
    }
}
