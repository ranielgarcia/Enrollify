using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
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
            var existing = await _readRepository.FirstOrDefaultAsync(new GetAcademicYearByIdSpec(command.id), cancellationToken);
            if (existing is null)
            {
                return Result.NotFound("The specified academic year was not found.");
            }

            var today = DateTime.UtcNow.Date;
            if (existing.EndDate.Value < today)
            {
                return Result.Invalid(new ValidationError("Deleting past academic year is not allowed."));
            }

            // TODO: Prevent deletion of academic year if
            // - it has associated terms or other related entities.
            // - its terms have associated enrollments or other related entities like class section
            // GetClassSectionsByAcademicTermIdsSpec

            var result = await _repository.Delete(existing, cancellationToken);
            return result;
        }
    }
}
