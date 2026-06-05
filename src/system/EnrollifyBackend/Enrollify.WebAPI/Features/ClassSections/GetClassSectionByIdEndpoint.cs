using Enrollify.Application.Features.ClassSections.DTOs;
using Enrollify.Application.Features.ClassSections.Queries;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSections;

[HttpGet("{id:int}")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewClassSectionsPermission)]
public class GetClassSectionByIdEndpoint : EndpointWithoutRequest<OkOrNotFoundApiResult<ClassSectionDetailDto>>
{
    private readonly IMediator _mediator;

    public GetClassSectionByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<ClassSectionDetailDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        int id = Route<int>("id");

        var result = await _mediator.Send(
            new GetClassSectionByIdQuery(ClassSectionId.From(id)),
            cancellationToken);

        return result.ToGetByIdResult(dto => dto);
    }
}

