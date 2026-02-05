using Ardalis.Result;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums;

public interface ICurriculumRepository
{
    Task<Result<CurriculumId>> CreateDraftCurriculum(Curriculum newCurriculum, CancellationToken cancellationToken);
    Task<Result<CurriculumId>> UpdateCurriculum(Curriculum newCurriculum, CancellationToken cancellationToken);

}
