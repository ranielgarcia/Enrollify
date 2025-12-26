using Ardalis.Result;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Departments.Features;

public static class CreateDepartment
{
    public sealed record Command(DepartmentCode code, string name, string chairperson, string description, CollegeId collegeId) 
        : ICommand<Result<DepartmentId>>;

    public sealed class Handler : ICommandHandler<Command, Result<DepartmentId>>
    {
        private readonly IReadRepository<College> _collegeReadRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public Handler(IReadRepository<College> collegeReadRepository, IDepartmentRepository departmentRepository)
        {
            _collegeReadRepository = collegeReadRepository;
            _departmentRepository = departmentRepository;
        }

        public async ValueTask<Result<DepartmentId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an ID of {command.collegeId} not found");
            }

            var department = new Department(command.code, command.name, command.chairperson, command.description, college.Id);
            var result = await _departmentRepository.Create(department, cancellationToken);
            return result;
        }
    }
}
