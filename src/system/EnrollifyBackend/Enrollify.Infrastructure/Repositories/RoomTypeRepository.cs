using Ardalis.Result;
using Enrollify.Application.RoomTypes;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class RoomTypeRepository : IRoomTypeRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<RoomTypeRepository> _logger;

    public RoomTypeRepository(EnrollifyDbContext dbContext, ILogger<RoomTypeRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<RoomTypeId>> Create(RoomType newRoomType, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.RoomTypes.AddAsync(newRoomType, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newRoomType.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateRoomTypeException(ex))
        {
            _logger.LogError(ex, "Duplicate room type name: {RoomTypeName}", newRoomType.Name);
            return Result.Conflict($"The room type name '{newRoomType.Name}' is already in use. Please choose a different name.");
        }
    }


    public async Task<Result<RoomTypeId>> Update(RoomType newRoomType, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.RoomTypes.Update(newRoomType);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newRoomType.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateRoomTypeException(ex))
        {
            _logger.LogError(ex, "Duplicate room type name: {RoomTypeName}", newRoomType.Name);
            return Result.Conflict($"The room type name '{newRoomType.Name}' is already in use. Please choose a different name.");
        }
    }


    public async Task<Result> Delete(RoomTypeId id, CancellationToken cancellation)
    {
        try
        {
            var roomType = await _dbContext.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id, cancellation);
            if (roomType == null)
            {
                return Result.NotFound($"Room type with ID {id} not found.");
            }
            _dbContext.RoomTypes.Remove(roomType);
            await _dbContext.SaveChangesAsync(cancellation);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete room type with ID: {RoomTypeId} due to foreign key constraint", id.Value);
            return Result.Conflict("Cannot delete this room type because it is currently in use by one or more rooms or courses. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room type with ID: {RoomTypeId}", id.Value);
            return Result.Error($"Unable to delete the room type with ID {id.Value} due to internal error");
        }
    }

    private bool IsDuplicateRoomTypeException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        return message?.Contains("UQ_RoomTypes_Name") == true;
    }

    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_RoomType") == true;
    }
}
