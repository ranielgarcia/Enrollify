using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionSubjectOfferingRepository
{
  Task<Result<ClassSectionSubjectOfferingId>> Create(ClassSectionSubjectOffering newClassSectionSubjectOffering,
    CancellationToken cancellationToken);

  Task<Result<ClassSectionSubjectOfferingId>> Update(ClassSectionSubjectOffering updatedClassSectionSubjectOffering,
    CancellationToken cancellationToken);

  Task<Result> Delete(ClassSectionSubjectOffering classSectionSubjectOffering, CancellationToken cancellationToken);

  Task<Dictionary<ClassSectionId, int>> GetSubjectOfferingsCountPerClassSection(
    List<ClassSectionId> classSectionIds, CancellationToken cancellationToken);
}
