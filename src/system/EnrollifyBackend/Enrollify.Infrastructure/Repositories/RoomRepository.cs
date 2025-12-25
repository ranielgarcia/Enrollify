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

    public Task<Result<RoomId>> Create(Room newRoom, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result> Delete(RoomId id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RoomId>> Update(Room newRoom, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
