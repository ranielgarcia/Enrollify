using Ardalis.Result;
using Enrollify.Application.Buildings.DTOs;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public class ListBuildingsQuery : IQuery<Result<List<BuildingDTO>>>
{
}

public class ListBuildingsQueryHandler : IQueryHandler<ListBuildingsQuery, Result<List<BuildingDTO>>>
{
    private readonly IBuildingRepository _buildingRepository;

    public ListBuildingsQueryHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async ValueTask<Result<List<BuildingDTO>>> Handle (ListBuildingsQuery request, CancellationToken cancellationToken)
    {
        var buildings = await _buildingRepository.ListBuildings(cancellationToken);

        var toReturn = buildings
            .Select(b => new BuildingDTO
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Address = b.Address,
                CreatedAt = b.CreatedAt,
                CreatedBy = b.CreatedBy,
                CreatedByUser = BaseUserDTO.FromUser(b.CreatedByUser),
                UpdatedByUser = BaseUserDTO.FromUser(b.UpdatedByUser),
                UpdatedAt = b.UpdatedAt,
                UpdatedBy = b.UpdatedBy,
                DeletedAt = b.DeletedAt,
                DeletedBy = b.DeletedBy,
                IsActive = b.IsActive
            }).ToList();

        return Result.Success(toReturn);
    }
}
