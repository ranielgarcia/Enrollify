using Enrollify.Application.Features.Teachers.DTOs;
using Enrollify.Application.Features.Teachers.Queries;

namespace Enrollify.WebAPI.Features.Teachers;

public class SearchTeachersPaginatedRequest
{
    public required int Page { get; set; }

    public required int PageSize { get; set; }

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class SearchTeachersPaginatedRequestValidator : Validator<SearchTeachersPaginatedRequest>
{
    public SearchTeachersPaginatedRequestValidator()
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
[Group<TeacherEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewTeacherPermission)]
public class SearchTeachersPaginatedEndpoint(IMediator mediator)
    : Endpoint<SearchTeachersPaginatedRequest, Application.PagedResult<TeacherDto>>
{
    public override async Task HandleAsync(SearchTeachersPaginatedRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SearchTeachersPaginatedQuery(request.SearchTerm, request.Page, request.PageSize), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
