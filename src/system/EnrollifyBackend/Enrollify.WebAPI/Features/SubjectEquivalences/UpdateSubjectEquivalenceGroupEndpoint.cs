using Enrollify.Application.Features.SubjectEquivalences.Commands;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class UpdateSubjectEquivalenceGroupRequest
{
    //[Microsoft.AspNetCore.Mvc.FromRoute]
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class UpdateSubjectEquivalenceGroupRequestValidator : Validator<UpdateSubjectEquivalenceGroupRequest>
{
    public UpdateSubjectEquivalenceGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a name for the subject equivalence group.")
            .MaximumLength(255).WithMessage("Name must be 255 characters or fewer.");
    }
}

[HttpPut("{Id}")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectEquivalenceGroupPermission)]
public class UpdateSubjectEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<UpdateSubjectEquivalenceGroupRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>>
        ExecuteAsync(UpdateSubjectEquivalenceGroupRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateSubjectEquivalenceGroup
            .Command(SubjectEquivalenceGroupId.From(request.Id), request.Name), ct);
        return result.ToUpdateResult(id => request.Id);
    }
}
