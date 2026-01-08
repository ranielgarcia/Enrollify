using Ardalis.Result;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums;

public interface ICurriculumRepository
{
    Task<Result<CurriculumId>> CreateDraftCurricula(Curriculum newCurriculum, CancellationToken cancellationToken);

}
