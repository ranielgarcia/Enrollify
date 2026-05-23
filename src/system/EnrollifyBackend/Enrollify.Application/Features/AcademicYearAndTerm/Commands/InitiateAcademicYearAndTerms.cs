using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Events;
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
        InitiateAcademicTerm[] academicTerms) : ICommand<Result<AcademicYearDto>>;

    public sealed class Handler : ICommandHandler<Command, Result<AcademicYearDto>>
    {
        private readonly IAcademicYearAndTermRepository _academicYearAndTermRepository;
        private readonly IReadRepository<AcademicYear> _readRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IAcademicYearAndTermRepository academicYearAndTermRepository,
            IReadRepository<AcademicYear> readRepository,
            IMediator mediator,
            ILogger<Handler> logger)
        {
            _academicYearAndTermRepository = academicYearAndTermRepository;
            _readRepository = readRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async ValueTask<Result<AcademicYearDto>> Handle(Command command, CancellationToken cancellationToken)
        {
            var expectedTermCount = new AcademicCoreSettings().AcademicTermSystem;

            if (command.academicTerms is null || command.academicTerms.Length == 0)
                return Result.Invalid(new ValidationError("At least one academic term must be provided."));

            if (command.academicTerms.Length != expectedTermCount)
                return Result.Invalid(new ValidationError(
                    $"The academic system requires exactly {expectedTermCount} term(s), but {command.academicTerms.Length} were provided."));

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

            try
            {
                var academicYear = new AcademicYear(command.academicYearStartDate, command.academicYearEndDate);

                foreach (var term in command.academicTerms)
                {
                    academicYear.AddTerm(term.TermNumber, term.StartDate, term.EndDate);
                }

                var result = await _academicYearAndTermRepository.Create(academicYear, cancellationToken);

                if (result.IsSuccess)
                {
                    await _mediator.Publish(new AcademicYearCreatedEvent(result.Value), cancellationToken);
                }

                return result.Map(AcademicYearDto.FromEntity);
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
