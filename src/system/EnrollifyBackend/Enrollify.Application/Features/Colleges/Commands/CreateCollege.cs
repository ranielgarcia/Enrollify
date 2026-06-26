namespace Enrollify.Application.Features.Colleges.Commands;

public static class CreateCollege
{
    public sealed record Command(CollegeCode code, string name, string description, string dean) : IRequest<Result<CollegeId>>;
    public sealed class Handler : IRequestHandler<Command, Result<CollegeId>>
    {
        private readonly ICollegeRepository _collegeRepository;
        public Handler(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }
        public async Task<Result<CollegeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = new College(command.code, command.name, command.description, command.dean);
            var result = await _collegeRepository.Create(college, cancellationToken);
            return result;
        }
    }
}
