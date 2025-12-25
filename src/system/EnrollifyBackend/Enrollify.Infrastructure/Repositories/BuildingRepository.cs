using Ardalis.Result;
using Enrollify.Application.Buildings;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class BuildingRepository : IBuildingRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<BuildingRepository> _logger;

    public BuildingRepository(EnrollifyDbContext dbContext, ILogger<BuildingRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<BuildingId>> Create(Building newBuilding, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Buildings.AddAsync(newBuilding, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newBuilding.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateBuildingNameException(ex))
        {
            _logger.LogError(ex, "Duplicate building name: {BuildingName}", newBuilding.Name);
            return Result.Conflict($"The building name '{newBuilding.Name}' is already in use. Please choose a different name.");
        }
    }

    public async Task<Result> Delete(BuildingId id, CancellationToken cancellationToken)
    {
        try
        {
            var building = await _dbContext.Buildings.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
            if (building == null)
            {
                return Result.NotFound($"Building with ID {id.Value} not found.");
            }
            _dbContext.Buildings.Remove(building);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            // TODO: Building will probably be referenced by other tables, so the error messages in this catch block might mislead the user
            _logger.LogWarning(ex, "Cannot delete building with ID: {BuildingId} due to foreign key constraint", id.Value);
            return Result.Conflict("Cannot delete this building because it is currently in use by one or more rooms or courses. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting building with ID: {BuildingId}", id.Value);
            return Result.Error($"Unable to delete the building with ID {id.Value} due to internal error");
        }
    }

    public async Task<Result<BuildingId>> Update(Building newBuilding, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Buildings.Update(newBuilding);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newBuilding.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateBuildingNameException(ex))
        {
            _logger.LogError(ex, "Duplicate building name: {BuildingName}", newBuilding.Name);
            return Result.Conflict($"The building name '{newBuilding.Name}' is already in use. Please choose a different name.");
        }
    }

    private bool IsDuplicateBuildingNameException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("UQ_Buildings_Name") == true;
    }

    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_Building") == true;
    }
}
