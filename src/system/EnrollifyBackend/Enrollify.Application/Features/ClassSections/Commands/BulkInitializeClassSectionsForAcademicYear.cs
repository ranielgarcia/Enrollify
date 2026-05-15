using Ardalis.Result;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Mediator;

namespace Enrollify.Application.Features.ClassSections.Commands;

public static class BulkInitializeClassSectionsForAcademicYear
{
    public sealed record Payload(
        CourseId courseId,
        AcademicYearId academicYearId,
        CurriculumId curriculumId,
        int numberOfSections);

    public sealed record Command (
        List<Payload> requestPayload
        ) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        public Handler()
        {
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {

            return Result.Success();
        }
    }

}
