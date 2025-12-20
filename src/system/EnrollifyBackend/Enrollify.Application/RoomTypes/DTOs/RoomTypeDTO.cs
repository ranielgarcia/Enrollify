using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.RoomTypes.DTOs;

public class RoomTypeDTO : BaseDTO
{
    public RoomTypeId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
