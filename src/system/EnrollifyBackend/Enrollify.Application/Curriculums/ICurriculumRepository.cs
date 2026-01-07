using Ardalis.Result;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums;

public interface ICurriculumRepository
{
    Task<Result<Curriculum>> CreateDraftCurricula(Curriculum newCurriculum, CancellationToken cancellationToken);

}
