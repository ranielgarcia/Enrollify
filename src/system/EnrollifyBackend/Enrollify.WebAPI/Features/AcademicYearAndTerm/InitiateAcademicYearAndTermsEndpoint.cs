using Enrollify.Application.Features.AcademicYearAndTerm.Commands;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Models;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.WebAPI.Features.AcademicYearAndTerm.Models;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

public class InitiateAcademicYearAndTermsRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AcademicTermRequest[] Terms { get; set; } = [];
}

public class InitiateAcademicYearAndTermsRequestValidator : Validator<InitiateAcademicYearAndTermsRequest>
{
    public InitiateAcademicYearAndTermsRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Please provide a start date for the academic year.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Please provide an end date for the academic year.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");

        RuleFor(x => x.Terms)
            .NotEmpty().WithMessage("Please provide at least one academic term.");

        RuleForEach(x => x.Terms).ChildRules(term =>
        {
            term.RuleFor(t => t.TermNumber)
                .InclusiveBetween(1, TermNumber.MaxTermNumber)
                .WithMessage($"Term number must be between 1 and {TermNumber.MaxTermNumber}.");

            term.RuleFor(t => t.StartDate)
                .NotEmpty().WithMessage("Please provide a start date for the term.");

            term.RuleFor(t => t.EndDate)
                .NotEmpty().WithMessage("Please provide an end date for the term.")
                .GreaterThan(t => t.StartDate).WithMessage("Term end date must be after the term start date.");
        });
    }
}

[HttpPost("")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateAcademicYearAndTermPermission)]
public class InitiateAcademicYearAndTermsEndpoint
    : Endpoint<InitiateAcademicYearAndTermsRequest, CreatedApiResult<AcademicYearDto>>
{
    private readonly IMediator _mediator;

    public InitiateAcademicYearAndTermsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<AcademicYearDto>>
        ExecuteAsync(InitiateAcademicYearAndTermsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new InitiateAcademicYearAndTerms.Command(
            AcademicYearStartDate.From(request.StartDate),
            AcademicYearEndDate.From(request.EndDate),
            request.Terms
                .Select(t => new InitiateAcademicTerm(
                    TermNumber.From(t.TermNumber),
                    AcademicTermStartDate.From(t.StartDate),
                    AcademicTermEndDate.From(t.EndDate)))
                .ToArray()),
            ct);

        return result.ToCreatedResult(
            dto => $"/academic-years/{dto.Id.Value}",
            dto => dto);
    }
}
