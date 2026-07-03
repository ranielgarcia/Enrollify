namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionSubjectOfferingRepository
{
  Task<Result<ClassSectionSubjectOfferingId>> Create(ClassSectionSubjectOffering newClassSectionSubjectOffering,
    CancellationToken cancellationToken);

  Task<Result<ClassSectionSubjectOfferingId>> Update(ClassSectionSubjectOffering updatedClassSectionSubjectOffering,
    CancellationToken cancellationToken);

  Task<Result> Delete(ClassSectionSubjectOffering classSectionSubjectOffering, CancellationToken cancellationToken);
}
