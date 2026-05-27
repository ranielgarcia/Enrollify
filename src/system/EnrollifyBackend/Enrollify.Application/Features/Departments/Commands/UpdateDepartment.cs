using Ardalis.Result;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Departments.Commands;

public static class UpdateDepartment
{
    public sealed record Command(DepartmentId id, DepartmentCode code, string name, string chairperson, string description, CollegeId collegeId)
        : IRequest<Result<DepartmentId>>;

    public sealed class Handler : IRequestHandler<Command, Result<DepartmentId>>
    {
        private readonly IReadRepository<College> _collegeReadRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IReadRepository<Department> _departmentReadRepository;

        public Handler(
            IReadRepository<College> collegeReadRepository,
            IDepartmentRepository departmentRepository,
            IReadRepository<Department> departmentReadRepository)
        {
            _collegeReadRepository = collegeReadRepository;
            _departmentRepository = departmentRepository;
            _departmentReadRepository = departmentReadRepository;
        }

        public async Task<Result<DepartmentId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an ID of {command.collegeId} not found");
            }

            var existing = await _departmentReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing == null)
            {
                return Result.NotFound($"Department with an ID of {command.id} not found");
            }

            existing
                .UpdateCode(command.code)
                .UpdateName(command.name)
                .UpdateChairperson(command.chairperson)
                .UpdateDescription(command.description)
                .UpdateCollegeId(command.collegeId);

            var updateResult = await _departmentRepository.Update(existing, cancellationToken);
            return updateResult;
        }
    }

}
