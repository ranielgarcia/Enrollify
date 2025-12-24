using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Core.Aggregates.RoomAggregate.Models;

public class RoomForCreation
{
    public string RoomNumber { get; set; }
    public int Capacity { get; set; }
    public RoomTypeId RoomTypeId { get; set; }
    public BuildingId BuildingId { get; set; }
    public CollegeId CollegeId { get; set; }
}
