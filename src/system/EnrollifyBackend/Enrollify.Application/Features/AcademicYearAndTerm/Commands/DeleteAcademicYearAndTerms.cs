using Ardalis.Result;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Commands;


public static class DeleteAcademicYearAndTerms
{
    public sealed record Command(AcademicYearId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IAcademicYearAndTermRepository _repository;
        private readonly IReadRepository<AcademicYear> _readRepository;

        public Handler(IAcademicYearAndTermRepository repository, IReadRepository<AcademicYear> readRepository)
        {
            _repository = repository;
            _readRepository = readRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await _readRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("The specified academic year was not found.");
            }

            var result = await _repository.Delete(existing, cancellationToken);
            return result;
        }
    }
}
