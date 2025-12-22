using Enrollify.Application.Colleges.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.WebAPI.Authorization;
using Enrollify.WebAPI.Extensions;
using FastEndpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features.Colleges;

public class DeleteRequest 
{
    [QueryParam]
    public int Id { get; set; }
}

public class DeleteRequestValidator : Validator<DeleteRequest>
{
    public DeleteRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid college ID.");
    }
}

[HttpDelete("")]
[Group<CollegeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasDeleteCollegePermission)]
public class DeleteEndpoint : Endpoint<DeleteRequest, Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public DeleteEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>> 
        ExecuteAsync (DeleteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCollege.Command(CollegeId.From(request.Id)));
        return result.ToDeleteResult();
    }
}
