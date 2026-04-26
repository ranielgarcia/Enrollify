using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.Features.SubjectEquivalences.DTOs;

public class SubjectEquivalenceGroupDto : BaseDto
{
    public SubjectEquivalenceGroupId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<SubjectSummaryDto> Subjects { get; set; } = new();

    public static SubjectEquivalenceGroupDto FromEntity(SubjectEquivalenceGroup group)
    {
        return new SubjectEquivalenceGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Subjects = group.SubjectEquivalences
                .Where(se => se.Subject != null)
                .Select(se => SubjectSummaryDto.FromEntity(se.Subject!))
                .ToList(),
            CreatedAt = group.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(group.CreatedByUser),
            UpdatedAt = group.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(group.UpdatedByUser),
            IsActive = group.IsActive,
        };
    }
}
