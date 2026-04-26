using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.Subjects.DTOs;

public class SubjectDto : BaseDto
{
    public SubjectId Id { get; set; }
    public SubjectCode Code { get; set; }
    public string Title { get; set; } = null!;
    public decimal Units { get; set; }
    public string Description { get; set; } = null!;
    public RoomTypeSummaryDto? PreferRoomType { get; set; }

    public static SubjectDto FromEntity(Subject subject)
    {
        return new SubjectDto
        {
            Id = subject.Id,
            Code = subject.Code,
            Title = subject.Title,
            Units = subject.Units,
            Description = subject.Description,
            PreferRoomType = subject.PreferRoomType != null ? RoomTypeSummaryDto.FromEntity(subject.PreferRoomType) : null,
            CreatedAt = subject.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(subject.CreatedByUser),
            UpdatedAt = subject.UpdatedAt,
            UpdatedBy = subject.UpdatedByUser != null ? BaseUserDto.FromUser(subject.UpdatedByUser) : null,
            IsActive = subject.IsActive,
        };
    }
}
