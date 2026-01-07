using Ardalis.Result;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums;

public interface ICurriculumRepository
{
    Task<Result<CurriculaId>> CreateDraftCurricula(Curriculum newCurriculum, CancellationToken cancellationToken);

}
