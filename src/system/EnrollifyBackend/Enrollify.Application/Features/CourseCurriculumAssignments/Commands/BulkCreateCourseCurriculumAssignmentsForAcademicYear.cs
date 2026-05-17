using Ardalis.Result;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Mediator;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class BulkCreateCourseCurriculumAssignmentsForAcademicYear
{
    public record Command(AcademicYearId AcademicYearId) : ICommand<Result>;
    public class Handler : ICommandHandler<Command, Result>
    {
        //private readonly ICourseCurriculumAssignmentRepository _repository;
        //private readonly IUnitOfWork _unitOfWork;
        //public Handler(ICourseCurriculumAssignmentRepository repository, IUnitOfWork unitOfWork)
        //{
        //    _repository = repository;
        //    _unitOfWork = unitOfWork;
        //}

        //public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        //{
        //    // Get all courses and curricula for the specified academic year
        //    var courses = await _repository.GetCoursesForAcademicYearAsync(request.AcademicYearId, cancellationToken);
        //    var curricula = await _repository.GetCurriculaForAcademicYearAsync(request.AcademicYearId, cancellationToken);
        //    // Create CourseCurriculumAssignments for each course and curriculum combination
        //    foreach (var course in courses)
        //    {
        //        foreach (var curriculum in curricula)
        //        {
        //            var assignment = new CourseCurriculumAssignment(course.Id, request.AcademicYearId, curriculum.Id);
        //            await _repository.AddAsync(assignment, cancellationToken);
        //        }
        //    }
        //    // Commit the changes to the database
        //    await _unitOfWork.CommitAsync(cancellationToken);
        //}
        public ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
