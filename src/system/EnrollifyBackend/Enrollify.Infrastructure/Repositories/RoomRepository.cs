using Ardalis.Result;
using Enrollify.Application.Rooms;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<RoomRepository> _logger;

    public RoomRepository(EnrollifyDbContext dbContext, ILogger<RoomRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<RoomId>> Create(Room newRoom, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Rooms.AddAsync(newRoom, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newRoom.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateException(ex))
        {
            _logger.LogError(ex, "Duplicate room number: {RoomNumber}", newRoom.RoomNumber);
            return Result.Conflict($"The room number '{newRoom.RoomNumber}' is already in use. Please choose a different number.");
        }
    }

    public async Task<Result> Delete(RoomId id, CancellationToken cancellationToken)
    {
        try
        {
            var room = await _dbContext.Rooms.FindAsync(new object[] { id.Value }, cancellationToken);
            if (room == null)
            {
                return Result.NotFound($"Room with ID {id.Value} not found.");
            }

            _dbContext.Rooms.Remove(room);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
        {
            _logger.LogWarning(ex, "Cannot delete room with ID: {roomId} due to foreign key constraint", id.Value);
            return Result.Conflict("Cannot delete this room because it is currently in use by one or more rooms or courses. Please remove all references before deleting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room with ID: {roomId}", id.Value);
            return Result.Error($"Unable to delete the room with ID {id.Value} due to internal error");
        }
    }

    public async Task<Result<RoomId>> Update(Room newRoom, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Rooms.Update(newRoom);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newRoom.Id);
        }
        catch (DbUpdateException ex) when (IsDuplicateException(ex))
        {
            _logger.LogError(ex, "Duplicate room number: {RoomNumber}", newRoom.RoomNumber);
            return Result.Conflict($"The room number '{newRoom.RoomNumber}' is already in use. Please choose a different number.");
        }
    }

    private bool IsDuplicateException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate") == true ||
               ex.InnerException?.Message.Contains("UIdx_Rooms_RoomNumber_room_IsActive") == true;
    }


    private bool IsForeignKeyConstraintException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true ||
               ex.InnerException?.Message.Contains("FK_") == true ||
               ex.InnerException?.Message.Contains("_Room") == true;
    }
}
