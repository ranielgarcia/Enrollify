using System.Text.Json;
using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Queries;
using Enrollify.Application.Filtering;

namespace Enrollify.WebAPI.Features.Subjects;

public class SearchSubjectsPaginatedRequest
{

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int Page { get; set; }

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int PageSize { get; set; }

    [QueryParam]
    public string? Filters { get; set; }

    [QueryParam]
    public string? Sort { get; set; }
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
        RuleFor(x => x.Filters)
            .Must(filters =>
            {
                try
                {
                    var result = JsonSerializer.Deserialize<List<FilterItem>>(filters!);
                    return result is not null;
                }
                catch (JsonException)
                {
                    return false;
                }
            }).WithMessage("Filters must be a valid JSON array of filter items.")
            .When(x => !string.IsNullOrEmpty(x.Filters));

        RuleFor(x => x.Sort)
            .Must(sort =>
            {
                try
                {
                    var result = JsonSerializer.Deserialize<List<FilterItem>>(sort!);
                    return result is not null;
                }
                catch (JsonException)
                {
                    return false;
                }
            }).WithMessage("Sort must be a valid JSON array of sort items.")
            .When(x => !string.IsNullOrEmpty(x.Sort));
    }
}

[HttpGet("search/{page}/{pageSize}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class SearchSubjectsPaginatedEndpoint (IMediator mediator) : Endpoint<SearchSubjectsPaginatedRequest, Application.PagedResult<SubjectDto>>
{
    public override async Task HandleAsync(SearchSubjectsPaginatedRequest request, CancellationToken cancellationToken)
    {
        var filters = request.Filters is not null
            ? JsonSerializer.Deserialize<List<FilterItem>>(request.Filters)
            : null;

        var sorts = request.Sort is not null
            ? JsonSerializer.Deserialize<List<SortItem>>(request.Sort)
            : null;

        var result = await mediator.Send(new FilterSubjectsPaginatedQuery(request.Page, request.PageSize, filters, sorts), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
