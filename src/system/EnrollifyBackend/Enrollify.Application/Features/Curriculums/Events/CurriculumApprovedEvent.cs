using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Application.Features.Curriculums.Events;

public sealed class CurriculumApprovedEvent(Curriculum curriculum) : DomainEventBase
{
    public Curriculum Curriculum { get; init; } = curriculum;
}
