using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Events;

public sealed class AcademicYearCreatedEvent (AcademicYear academicYear) : DomainEventBase
{
    public AcademicYear AcademicYear { get; init; } = academicYear;
}
