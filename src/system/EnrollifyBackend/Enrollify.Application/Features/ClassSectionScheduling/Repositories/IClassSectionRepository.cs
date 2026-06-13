using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionRepository
{
    Task<Result<ClassSectionId>> Create(ClassSection newClassSection, CancellationToken cancellationToken);
    Task<Result<ClassSectionId>> Update(ClassSection updatedClassSection, CancellationToken cancellationToken);
    Task<Result> BulkUpdate(List<ClassSection> classSectionsToUpdate, CancellationToken cancellationToken);
    Task<Result> Delete(ClassSection classSection, CancellationToken cancellationToken);
}
