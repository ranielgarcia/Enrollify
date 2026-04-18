using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Features;

namespace Enrollify.WebAPI.Features.Subjects;

public class SearchSubjectsPaginatedRequest
{

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int Page { get; set; }

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int PageSize { get; set; }

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class SearchSubjectsPaginatedRequestValidator : Validator<SearchSubjectsPaginatedRequest>
{
    public SearchSubjectsPaginatedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be 100 or fewer.");
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Please provide a search term.")
            .MaximumLength(100).WithMessage("Search term must be 100 characters or fewer.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}

[HttpGet("search/{page}/{pageSize}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class SearchSubjectsPaginatedEndpoint (IMediator mediator) : Endpoint<SearchSubjectsPaginatedRequest, Application.PagedResult<SubjectDTO>>
{
    public override async Task HandleAsync(SearchSubjectsPaginatedRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SearchSubjectsPaginatedQuery(request.SearchTerm, request.Page, request.PageSize), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
