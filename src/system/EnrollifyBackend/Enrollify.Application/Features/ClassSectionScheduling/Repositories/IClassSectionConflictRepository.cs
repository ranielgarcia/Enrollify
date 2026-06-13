using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionConflictAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionConflictRepository
{
  Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionConflict> conflicts,
    CancellationToken cancellationToken = default);
}
