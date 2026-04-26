using Ardalis.Result;
using Enrollify.Application.Features.SubjectEquivalences;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class SubjectEquivalenceGroupRepository : ISubjectEquivalenceGroupRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<SubjectEquivalenceGroupRepository> _logger;

    public SubjectEquivalenceGroupRepository(EnrollifyDbContext dbContext, ILogger<SubjectEquivalenceGroupRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<SubjectEquivalenceGroupId>> AddNewSubjectEquivalenceGroup(SubjectEquivalenceGroup newGroup, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SubjectEquivalenceGroups.AddAsync(newGroup, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newGroup.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateNameException(ex))
        {
            _logger.LogError(ex, "Duplicate subject equivalence group name: {SubjectEquivalenceGroupName}", newGroup.Name);
            return Result.Conflict($"The subject equivalence group name '{newGroup.Name}' is already in use. Please choose a different name.");
        }
    }

    public async Task<Result<SubjectEquivalenceGroupId>> UpdateSubjectEquivalenceGroup(SubjectEquivalenceGroup updatedGroup, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.SubjectEquivalenceGroups.Update(updatedGroup);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(updatedGroup.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateNameException(ex))
        {
            _logger.LogError(ex, "Duplicate subject equivalence group name: {SubjectEquivalenceGroupName}", updatedGroup.Name);
            return Result.Conflict($"The subject equivalence group name '{updatedGroup.Name}' is already in use. Please choose a different name.");
        }
    }

    public async Task<Result> Delete(SubjectEquivalenceGroupId id, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _dbContext.SubjectEquivalenceGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
            if (group == null)
            {
                return Result.NotFound($"Subject equivalence group with an id of {id.Value} not found");
            }

            _dbContext.SubjectEquivalenceGroups.Remove(group);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject equivalence group with an of ID: {SubjectEquivalenceGroupId}", id.Value);
            return Result.Error($"Unable to delete the subject equivalence group with ID {id.Value} due to internal error");
        }
    }

    private bool IsDuplicateNameException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("UQ_SubjectEquivalenceGroups_Name") == true;
    }
}
