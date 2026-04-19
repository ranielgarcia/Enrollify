using Enrollify.Application.Features.Subjects.Commands;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.WebAPI.Features.Subjects;

public class DeleteSubjectRequest
{
    [QueryParam]
    public int Id { get; set; }
}

public class DeleteSubjectRequestValidator : Validator<DeleteSubjectRequest>
{
    public DeleteSubjectRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid subject ID.");
    }
}

[HttpDelete("")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteSubjectPermission)]
public class DeleteEndpoint (IMediator mediator) 
    : Endpoint<DeleteSubjectRequest, DeleteApiResult>
{
    public override async Task<DeleteApiResult>
        ExecuteAsync (DeleteSubjectRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSubject.Command(SubjectId.From(request.Id)), cancellationToken);
        return result.ToDeleteResult();
    }
}
