using Enrollify.Application;
using Enrollify.Application.RoomTypes.DTOs;
using Enrollify.Application.RoomTypes.Features.List;
using Enrollify.Application.Users;

namespace Enrollify.Infrastructure.Data.Queries;

public class ListRoomTypesQueryService : IListRoomTypesQueryService
{
    private readonly EnrollifyDbContext _dbContext;

    public ListRoomTypesQueryService(EnrollifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomTypeDTO>> ListRoomTypesAsync(CancellationToken cancellationToken = default)
    {
        var roomTypes = await _dbContext.RoomTypes
            .Include(r => r.CreatedByUser)
            .Select(rt => new RoomTypeDTO
            {
                Id = rt.Id,
                Name = rt.Name,
                Description = rt.Description,
                CreatedAt = rt.CreatedAt,
                CreatedBy = rt.CreatedBy,
                CreatedByUser = BaseUserDTO.FromUser(rt.CreatedByUser),
                UpdatedAt = rt.UpdatedAt,
                UpdatedBy = rt.UpdatedBy,
                DeletedAt = rt.DeletedAt,
                DeletedBy = rt.DeletedBy,
                IsActive = rt.IsActive
            })
            .ToListAsync(cancellationToken);
        return roomTypes;
    }
}
