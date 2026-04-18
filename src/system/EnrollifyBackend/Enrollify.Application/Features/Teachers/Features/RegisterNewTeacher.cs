using Ardalis.Result;
using Enrollify.Application.Features.Teachers;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate.Models;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Teachers.Features;

public static class RegisterNewTeacher
{
    public sealed record Command (TeacherForCreation teacherForCreation) : ICommand<Result<TeacherId>>;

    public sealed class Handler : ICommandHandler<Command, Result<TeacherId>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IReadRepository<Teacher> _teacherReadRepository;

        public Handler(ITeacherRepository teacherRepository, IReadRepository<Teacher> teacherReadRepository)
        {
            _teacherRepository = teacherRepository;
            _teacherReadRepository = teacherReadRepository;
        }

        public async ValueTask<Result<TeacherId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var teacher = Teacher.Create(command.teacherForCreation);
            var result = await _teacherRepository.Create(teacher, cancellationToken);



            return result;
        }


    }
}
