using Ardalis.Result;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Mediator;

namespace Enrollify.Application.Features.Departments.Commands;

public static class DeleteDepartment
{
    public sealed record Command(DepartmentId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public Handler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // TODO: Add validations, check for related entities, etc.

            return await _departmentRepository.Delete(command.id, cancellationToken);
        }
    }
}
