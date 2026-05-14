using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings;

public interface IClassSectionSubjectOfferingRepository
{
    Task<Result<ClassSectionSubjectOfferingId>> Create(ClassSectionSubjectOffering newClassSectionSubjectOffering, CancellationToken cancellationToken);
    Task<Result<ClassSectionSubjectOfferingId>> Update(ClassSectionSubjectOffering updatedClassSectionSubjectOffering, CancellationToken cancellationToken);
    Task<Result> Delete(ClassSectionSubjectOfferingId classSectionSubjectOfferingId, CancellationToken cancellationToken);
}
