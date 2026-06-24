using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionSubjectOfferingRepository : IClassSectionSubjectOfferingRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly ILogger<ClassSectionSubjectOfferingRepository> _logger;

  public ClassSectionSubjectOfferingRepository(
    EnrollifyDbContext dbContext,
    ILogger<ClassSectionSubjectOfferingRepository> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task<Result<ClassSectionSubjectOfferingId>> Create(
    ClassSectionSubjectOffering newClassSectionSubjectOffering, CancellationToken cancellationToken)
  {
    try
    {
      await _dbContext.ClassSectionSubjectOfferings.AddAsync(newClassSectionSubjectOffering, cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success(newClassSectionSubjectOffering.Id);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating new class section subject offering {@ClassSectionSubjectOffering}",
        newClassSectionSubjectOffering);
      return Result.Error("Unable to create the new class section subject offering due to internal error");
    }
  }

  public async Task<Result> Delete(ClassSectionSubjectOffering classSectionSubjectOffering,
    CancellationToken cancellationToken)
  {
    try
    {
      _dbContext.ClassSectionSubjectOfferings.Remove(classSectionSubjectOffering);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting class section subject offering {@ClassSectionSubjectOfferingId}",
        classSectionSubjectOffering.Id.Value);
      return Result.Error("Unable to delete class section subject offering due to internal error");
    }
  }

  public async Task<Dictionary<ClassSectionId, int>> GetSubjectOfferingsCountPerClassSection(
    List<ClassSectionId> classSectionIds, CancellationToken cancellationToken)
  {
    Dictionary<ClassSectionId, int> counts = await _dbContext.ClassSectionSubjectOfferings
      .Where(x => classSectionIds.Contains(x.ClassSectionId))
      .GroupBy(x => x.ClassSectionId)
      .Select(g => new { ClassSectionId = g.Key, Count = g.Count() })
      .ToDictionaryAsync(x => x.ClassSectionId, x => x.Count, cancellationToken);

    return counts;
  }

  public async Task<Result<ClassSectionSubjectOfferingId>> Update(
    ClassSectionSubjectOffering updatedClassSectionSubjectOffering, CancellationToken cancellationToken)
  {
    try
    {
      _dbContext.ClassSectionSubjectOfferings.Update(updatedClassSectionSubjectOffering);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success(updatedClassSectionSubjectOffering.Id);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating class section subject offering {@ClassSectionSubjectOffering}",
        updatedClassSectionSubjectOffering);
      return Result.Error("Unable to update the class section subject offering due to internal error");
    }
  }
}
