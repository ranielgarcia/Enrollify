using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Commands;

public static class DeleteAcademicYearAndTerms
{
    public sealed record Command(AcademicYearId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IAcademicYearAndTermRepository _repository;
        private readonly IReadRepository<AcademicYear> _readRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;

        public Handler(
            IAcademicYearAndTermRepository repository,
            IReadRepository<AcademicYear> readRepository,
            IReadRepository<ClassSection> classSectionReadRepository)
        {
            _repository = repository;
            _readRepository = readRepository;
            _classSectionReadRepository = classSectionReadRepository;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
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

            var termIds = existing.AcademicTerms.Select(t => t.Id).ToList();
            if (termIds.Count > 0)
            {
                var sections = await _classSectionReadRepository.ListAsync(
                    new GetClassSectionsByAcademicTermIdsSpec(termIds), cancellationToken);
                if (sections.Count > 0)
                    return Result.Forbidden(
                        $"Cannot delete academic year — {sections.Count} class section(s) are associated with its terms.");
            }

            var result = await _repository.Delete(existing, cancellationToken);
            return result;
        }
    }
}
