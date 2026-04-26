using Ardalis.Result;
using Enrollify.Application.Features.SubjectEquivalences.Commands;
using Enrollify.Application.Features.SubjectEquivalences.DTOs;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class AddSubjectsToEquivalenceGroupRequest
{
    //[Microsoft.AspNetCore.Mvc.FromRoute]
    public int Id { get; set; }
    public List<int> SubjectIds { get; set; } = new List<int>();
}

public class AddSubjectsToEquivalenceGroupRequestValidator : Validator<AddSubjectsToEquivalenceGroupRequest>
{
    public AddSubjectsToEquivalenceGroupRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().WithMessage("Please provide the subject equivalence group id");

        RuleFor(x => x.SubjectIds)
            .NotEmpty().WithMessage("Please provide at least one subject id.");
    }
}

[HttpPut("{Id}/add-subjects")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectEquivalenceGroupPermission)]
public class AddSubjectsToEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<AddSubjectsToEquivalenceGroupRequest, OkOrNotFoundApiResult<SubjectEquivalenceGroupDto>>
{
    public override async Task<OkOrNotFoundApiResult<SubjectEquivalenceGroupDto>>
        ExecuteAsync(AddSubjectsToEquivalenceGroupRequest request, CancellationToken ct)
    {
        var subjectIds = request.SubjectIds.Select(id => SubjectId.From(id)).ToList();
        var result = await mediator.Send(new AddSubjectsToEquivalenceGroup
            .Command(SubjectEquivalenceGroupId.From(request.Id), subjectIds), ct);
        return result.ToUpdateResult(id => result.Value);
    }
}
