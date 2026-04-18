using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.Features.SubjectEquivalences.DTOs;

public class SubjectEquivalenceGroupDTO : BaseDTO
{
    public SubjectEquivalenceGroupId Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<SubjectSummaryDTO> Subjects { get; set; } = new();

    public static SubjectEquivalenceGroupDTO FromEntity(SubjectEquivalenceGroup group)
    {
        return new SubjectEquivalenceGroupDTO
        {
            Id = group.Id,
            Name = group.Name,
            Subjects = group.SubjectEquivalences
                .Where(se => se.Subject != null)
                .Select(se => SubjectSummaryDTO.FromEntity(se.Subject!))
                .ToList(),
            CreatedAt = group.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(group.CreatedByUser),
            UpdatedAt = group.UpdatedAt,
            UpdatedBy = BaseUserDTO.FromUser(group.UpdatedByUser),
            IsActive = group.IsActive,
        };
    }
}
