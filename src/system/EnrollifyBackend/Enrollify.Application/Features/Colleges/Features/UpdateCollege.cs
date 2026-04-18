using Ardalis.Result;
using Enrollify.Application.Features.Colleges;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Colleges.Features;

public static class UpdateCollege
{
    public sealed record Command(CollegeId id, CollegeCode code, string name, string description, string dean) : ICommand<Result<CollegeId>>;
    public sealed class Handler : ICommandHandler<Command, Result<CollegeId>>
    {
        private readonly ICollegeRepository _collegeRepository;
        private readonly IReadRepository<College> _collegeReadRepository;

        public Handler(ICollegeRepository collegeRepository, IReadRepository<College> collegeReadRepository)
        {
            _collegeRepository = collegeRepository;
            _collegeReadRepository = collegeReadRepository;
        }
        public async ValueTask<Result<CollegeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existingCollege = await _collegeReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existingCollege == null)
            {
                return Result.NotFound($"College with an ID of {command.id.Value} was not found.");
            }

            existingCollege
                .UpdateCode(command.code)
                .UpdateName(command.name)
                .UpdateDescription(command.description)
                .UpdateDean(command.dean);

            var updateResult = await _collegeRepository.Update(existingCollege, cancellationToken);
            return updateResult;
        }
    }
}
