using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections;

public interface IClassSectionRepository
{
    Task<Result<ClassSectionId>> Create(ClassSection newClassSection, CancellationToken cancellationToken);
    Task<Result<ClassSectionId>> Update(ClassSection updatedClassSection, CancellationToken cancellationToken);
    Task<Result> Delete(ClassSectionId classSectionId, CancellationToken cancellationToken);
}
