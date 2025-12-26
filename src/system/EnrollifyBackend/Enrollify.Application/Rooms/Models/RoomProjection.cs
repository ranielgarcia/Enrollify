using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Rooms.Models;

public class RoomProjection
{
    public RoomId Id { get; set; }
    public string RoomNumber { get; set; } = default!;
    public int Capacity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public User? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public User? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public RoomTypeProjection? RoomType { get; set; }
    public BuildingProjection? Building { get; set; }
}
