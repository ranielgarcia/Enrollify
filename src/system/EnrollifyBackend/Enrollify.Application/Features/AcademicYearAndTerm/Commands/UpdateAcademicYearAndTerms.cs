using System.Diagnostics;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Models;
using Enrollify.Core;
using Enrollify.Core.DomainExceptions;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Commands;

public static class UpdateAcademicYearAndTerms
{
    public sealed record Command(
        AcademicYearId id,
        AcademicYearStartDate academicYearStartDate,
        AcademicYearEndDate academicYearEndDate,
        InitiateAcademicTerm[] academicTerms) : IRequest<Result<AcademicYearDto>>;

    public sealed class Handler : IRequestHandler<Command, Result<AcademicYearDto>>
    {
        private readonly IAcademicYearAndTermRepository _academicYearAndTermRepository;
        private readonly IReadRepository<AcademicYear> _readRepository;
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IAcademicYearAndTermRepository academicYearAndTermRepository,
            IReadRepository<AcademicYear> readRepository,
            IReadRepository<ClassSection> classSectionReadRepository,
            ILogger<Handler> logger)
        {
            _academicYearAndTermRepository = academicYearAndTermRepository;
            _readRepository = readRepository;
            _classSectionReadRepository = classSectionReadRepository;
            _logger = logger;
        }

        public async Task<Result<AcademicYearDto>> Handle(Command command, CancellationToken cancellationToken)
        {
            var expectedTermCount = new AcademicCoreSettings().AcademicTermSystem;

            if (command.academicTerms is null || command.academicTerms.Length == 0)
                return Result<AcademicYearDto>.Invalid(new ValidationError("At least one academic term must be provided."));

            if (command.academicTerms.Length != expectedTermCount)
            {
                return Result<AcademicYearDto>.Invalid(new ValidationError(
                    $"The academic system requires exactly {expectedTermCount} term(s), but {command.academicTerms.Length} were provided."));
            }

            var overlappingAcademicYears = await _readRepository.ListAsync(
                new GetOverlappingAcademicYearByDateRangeSpec(command.academicYearStartDate, command.academicYearEndDate, command.id),
                cancellationToken);

            if (overlappingAcademicYears.Count > 0)
            {
                _logger.LogWarning(
                    "Academic year update conflict: proposed range {Start}-{End} for year {Id} overlaps with {Count} existing year(s).",
                    command.academicYearStartDate.Value, command.academicYearEndDate.Value, command.id, overlappingAcademicYears.Count);
                return Result.Conflict("The specified academic year overlaps with an existing academic year. Please choose a different date range.");
            }

            try
            {
                var existing = await _readRepository.FirstOrDefaultAsync(new GetAcademicYearByIdSpec(command.id), cancellationToken);

                if (existing is null)
                    return Result.NotFound("The specified academic year was not found.");

                // Block updates when there are Active or Completed sections for this academic year's terms
                var termIds = existing.AcademicTerms.Select(t => t.Id).ToList();
                if (termIds.Count > 0)
                {
                    var sections = await _classSectionReadRepository.ListAsync(
                        new GetClassSectionsByAcademicTermIdsSpec(termIds), cancellationToken);
                    var lockedSections = sections
                        .Where(s => s.StatusId == ClassSectionStatusEnum.Active
                                    || s.StatusId == ClassSectionStatusEnum.Completed)
                        .ToList();
                    if (lockedSections.Count > 0)
                        return Result.Forbidden(
                            $"Cannot update academic year — {lockedSections.Count} class section(s) are Active or Completed for its terms.");
                }

                existing.UpdateStartAndEndYear(command.academicYearStartDate, command.academicYearEndDate);

                foreach (var term in command.academicTerms)
                    existing.UpdateTerm(term.TermNumber, term.StartDate, term.EndDate);

                var result = await _academicYearAndTermRepository.Update(existing, cancellationToken);
                return Result.Success(AcademicYearDto.FromEntity(result.Value));
            }
            catch (InvalidAcademicYearRangeException ex)
            {
                return Result<AcademicYearDto>.Invalid(new ValidationError(ex.Message));
            }
            catch (InvalidAcademicTermException ex)
            {
                return Result<AcademicYearDto>.Invalid(new ValidationError(ex.Message));
            }
        }
    }
}

