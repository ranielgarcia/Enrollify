using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.DTOs;

public class SubjectDTO : BaseDTO
{
    public SubjectId Id { get; set; }
    public SubjectCode Code { get; set; }
    public string Title { get; set; } = null!;
    public decimal Units { get; set; }
    public string Description { get; set; } = null!;
    public CourseSummaryDTO? Course { get; set; }
    public RoomTypeSummaryDTO? PreferRoomType { get; set; }

    public static SubjectDTO FromEntity(Subject subject)
    {
        return new SubjectDTO
        {
            Id = subject.Id,
            Code = subject.Code,
            Title = subject.Title,
            Units = subject.Units,
            Description = subject.Description,
            Course = subject.Course != null ? CourseSummaryDTO.FromEntity(subject.Course) : null,
            PreferRoomType = subject.PreferRoomType != null ? RoomTypeSummaryDTO.FromEntity(subject.PreferRoomType) : null,
            CreatedAt = subject.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(subject.CreatedByUser),
            UpdatedAt = subject.UpdatedAt,
            UpdatedBy = subject.UpdatedByUser != null ? BaseUserDTO.FromUser(subject.UpdatedByUser) : null,
            IsActive = subject.IsActive,
        };
    }
}
