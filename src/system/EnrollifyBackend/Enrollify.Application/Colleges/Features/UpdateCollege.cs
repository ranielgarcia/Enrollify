using Ardalis.Result;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Mediator;

namespace Enrollify.Application.Colleges.Features;

public static class UpdateCollege
{
    public sealed record Command(CollegeId id, CollegeCode code, string name, string description, string dean) : ICommand<Result<CollegeId>>;
    public sealed class Handler : ICommandHandler<Command, Result<CollegeId>>
    {
        private readonly ICollegeRepository _collegeRepository;
        public Handler(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }
        public async ValueTask<Result<CollegeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existingCollege = await _collegeRepository.GetById(command.id, cancellationToken);
            if (existingCollege == null)
            {
                return Result.NotFound($"College with an ID of '{command.id}' was not found.");
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
