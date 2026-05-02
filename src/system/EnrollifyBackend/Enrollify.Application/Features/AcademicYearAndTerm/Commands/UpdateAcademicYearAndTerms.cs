using System.Diagnostics;
using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Models;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Commands;

public static class UpdateAcademicYearAndTerms
{
    public sealed record Command(
        AcademicYearId id,
        AcademicYearStartDate academicYearStartDate,
        AcademicYearEndDate academicYearEndDate,
        InitiateAcademicTerm[] academicTerms) : ICommand<Result<AcademicYearId>>;

    public sealed class Handler : ICommandHandler<Command, Result<AcademicYearId>>
    {
        private readonly IAcademicYearAndTermRepository _academicYearAndTermRepository;
        private readonly IReadRepository<AcademicYear> _readRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IAcademicYearAndTermRepository academicYearAndTermRepository,
            IReadRepository<AcademicYear> readRepository,
            ILogger<Handler> logger)
        {
            _academicYearAndTermRepository = academicYearAndTermRepository;
            _readRepository = readRepository;
            _logger = logger;
        }

        public async ValueTask<Result<AcademicYearId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var expectedTermCount = new AcademicSettings().AcademicSystem;

            if (command.academicTerms is null || command.academicTerms.Length == 0)
                return Result<AcademicYearId>.Invalid(new ValidationError("At least one academic term must be provided."));

            if (command.academicTerms.Length != expectedTermCount)
            {
                return Result<AcademicYearId>.Invalid(new ValidationError(
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

                existing.UpdateStartAndEndYear(command.academicYearStartDate, command.academicYearEndDate);

                foreach (var term in existing.AcademicTerms.ToList())
                    existing.RemoveTerm(term.Id);

                foreach (var term in command.academicTerms)
                    existing.AddTerm(term.TermNumber, term.StartDate, term.EndDate);


                var result = await _academicYearAndTermRepository.Update(existing, cancellationToken);
                return result;
            }
            catch (InvalidAcademicYearRangeException ex)
            {
                return Result<AcademicYearId>.Invalid(new ValidationError(ex.Message));
            }
            catch (InvalidAcademicTermException ex)
            {
                return Result<AcademicYearId>.Invalid(new ValidationError(ex.Message));
            }
        }
    }
}
