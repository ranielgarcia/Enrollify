using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

namespace Enrollify.Application.Features.CourseCurriculumAssignments;

public interface ICourseCurriculumAssignmentRepository
{
    Task<Result> BulkCreate(List<CourseCurriculumAssignment> courseCurriculumAssignments, CancellationToken ct);
    Task<Result> BulkUpdate(List<CourseCurriculumAssignment> courseCurriculumAssignments, CancellationToken ct);
}
