using Ardalis.Result;
using Enrollify.Application.Features.Colleges;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Mediator;

namespace Enrollify.Application.Features.Colleges.Features;

public static class CreateCollege
{
    public sealed record Command(CollegeCode code, string name, string description, string dean) : ICommand<Result<CollegeId>>;
    public sealed class Handler : ICommandHandler<Command, Result<CollegeId>>
    {
        private readonly ICollegeRepository _collegeRepository;
        public Handler(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }
        public async ValueTask<Result<CollegeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = new College(command.code, command.name, command.description, command.dean);
            var result = await _collegeRepository.Create(college, cancellationToken);
            return result;
        }
    }
}
