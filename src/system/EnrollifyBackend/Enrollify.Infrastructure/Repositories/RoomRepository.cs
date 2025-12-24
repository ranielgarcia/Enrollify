using Enrollify.Application.Rooms;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
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

    public async Task<List<Room>> GetAllByRoomType(RoomTypeId roomTypeId, CancellationToken cancellationToken)
    {
        var rooms = await _dbContext.Rooms
            .Where(r => r.RoomTypeId == roomTypeId)
            .ToListAsync(cancellationToken);
        return rooms;
    }

    public async Task<List<Room>> GetAllByCollege(CollegeId collegeId, CancellationToken cancellationToken)
    {
        var rooms = await _dbContext.Rooms
            .Where(r => r.CollegeId == collegeId)
            .ToListAsync(cancellationToken);
        return rooms;
    }

    public async Task<List<Room>> GetAllByBuilding(BuildingId buildingId, CancellationToken cancellationToken) 
    {
        var rooms = await _dbContext.Rooms
            .Where(r => r.BuildingId == buildingId)
            .ToListAsync(cancellationToken);
        return rooms;
    }
}
