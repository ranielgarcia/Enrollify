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

public static class InitiateAcademicYearAndTerms
{
    public sealed record Command(
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
            var existingAcademicYear = await _readRepository.FirstOrDefaultAsync(
                new GetOverlappingAcademicYearByDateRangeSpec(command.academicYearStartDate, command.academicYearEndDate),
                cancellationToken);

            if (existingAcademicYear is not null)
            {
                _logger.LogWarning(
                    "Academic year creation conflict: proposed range {Start}-{End} overlaps with existing year {ExistingId}.",
                    command.academicYearStartDate.Value, command.academicYearEndDate.Value, existingAcademicYear.Id);
                return Result.Conflict("The specified academic year overlaps with an existing academic year. Please choose a different date range.");
            }

            var expectedTermCount = new AcademicSettings().AcademicSystem;

            if (command.academicTerms is null || command.academicTerms.Length == 0)
                return Result<AcademicYearId>.Invalid(new ValidationError("At least one academic term must be provided."));

            if (command.academicTerms.Length != expectedTermCount)
                return Result<AcademicYearId>.Invalid(new ValidationError(
                    $"The academic system requires exactly {expectedTermCount} term(s), but {command.academicTerms.Length} were provided."));

            try
            {
                var academicYear = new AcademicYear(command.academicYearStartDate, command.academicYearEndDate);

                foreach (var term in command.academicTerms)
                {
                    academicYear.AddTerm(term.TermNumber, term.StartDate, term.EndDate);
                }

                var result = await _academicYearAndTermRepository.Create(academicYear, cancellationToken);
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
