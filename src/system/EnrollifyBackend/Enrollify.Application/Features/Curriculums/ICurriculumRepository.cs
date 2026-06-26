using Enrollify.Core.Models.Views;

namespace Enrollify.Application.Features.Curriculums;

public interface ICurriculumRepository
{
    Task<Result<CurriculumId>> CreateDraftCurriculum(Curriculum newCurriculum, CancellationToken cancellationToken);
    Task<Result<CurriculumId>> UpdateCurriculum(Curriculum newCurriculum, CancellationToken cancellationToken);
    Task<List<LatestActiveCurriculumPerCourseView>> GetAllLatestActiveCurriculumPerCourse(CancellationToken cancellationToken);
}
