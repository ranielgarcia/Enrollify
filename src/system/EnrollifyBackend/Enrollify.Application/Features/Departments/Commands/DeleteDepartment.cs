using Ardalis.Result;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using MediatR;

namespace Enrollify.Application.Features.Departments.Commands;

public static class DeleteDepartment
{
    public sealed record Command(DepartmentId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public Handler(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // TODO: Add validations, check for related entities, etc.

            return await _departmentRepository.Delete(command.id, cancellationToken);
        }
    }
}
