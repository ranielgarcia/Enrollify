using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Core.Aggregates.SubjectAggregate;

public class SubjectForCreation
{
    public SubjectCode Code { get; set; }
    public string Title { get; set; } = null!;
    public decimal Units { get; set; }
    public string Description { get; set; } = null!;
    public CourseId CourseId { get; set; }
    public RoomTypeId PreferRoomTypeId { get; set; }
}
