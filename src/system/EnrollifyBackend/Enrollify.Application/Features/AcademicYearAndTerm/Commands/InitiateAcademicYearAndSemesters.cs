using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Models;
using Enrollify.Core;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.DomainExceptions;
using Mediator;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Commands;

public static class InitiateAcademicYearAndSemesters
{
    public sealed record Command(
        AcademicYearStartDate academicYearStartDate,
        AcademicYearEndDate academicYearEndDate,
        InitiateAcademicTerm[] academicTerms) : ICommand<Result<AcademicYearId>>;

    public sealed class Handler : ICommandHandler<Command, Result<AcademicYearId>>
    {
        private readonly IAcademicYearAndTermRepository _academicYearAndTermRepository;

        public Handler(IAcademicYearAndTermRepository academicYearAndTermRepository)
        {
            _academicYearAndTermRepository = academicYearAndTermRepository;
        }

        public async ValueTask<Result<AcademicYearId>> Handle(Command command, CancellationToken cancellationToken)
        {
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
