using System.Text.Json;
using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Queries;
using Enrollify.Application.Filtering;

namespace Enrollify.WebAPI.Features.Subjects;

public class FilterSubjectsPaginatedRequest
{

    //[Microsoft.AspNetCore.Mvc.FromRoute]
    public required int Page { get; set; }

    //[Microsoft.AspNetCore.Mvc.FromRoute]
    public required int PageSize { get; set; }

    [QueryParam]
    public string? Filters { get; set; }

    [QueryParam]
    public string? Sort { get; set; }

    [QueryParam]
    public string? JoinOperator { get; set; }
}

public class FilterSubjectsPaginatedRequestValidator : Validator<FilterSubjectsPaginatedRequest>
{
    public FilterSubjectsPaginatedRequestValidator()
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
                    var result = JsonSerializer.Deserialize<List<FilterItem>>(filters!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                    var result = JsonSerializer.Deserialize<List<SortItem>>(sort!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result is not null;
                }
                catch (JsonException)
                {
                    return false;
                }
            }).WithMessage("Sort must be a valid JSON array of sort items.")
            .When(x => !string.IsNullOrEmpty(x.Sort));

        RuleFor(x => x.JoinOperator)
            .Must(jo => jo != null && (jo.ToLower().Trim() == "and" || jo.ToLower().Trim() == "or"))
            .When(x => !string.IsNullOrEmpty(x.JoinOperator));
    }
}

[HttpGet("filter/{page}/{pageSize}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class FilterSubjectsPaginatedEndpoint (IMediator mediator) : Endpoint<FilterSubjectsPaginatedRequest, Application.PagedResult<SubjectDto>>
{
    public override async Task HandleAsync(FilterSubjectsPaginatedRequest request, CancellationToken ct)
    {
        var filters = request.Filters is not null
            ? JsonSerializer.Deserialize<List<FilterItem>>(request.Filters, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            : null;

        var sorts = request.Sort is not null
            ? JsonSerializer.Deserialize<List<SortItem>>(request.Sort, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            : null;

        var result = await mediator.Send(new FilterSubjectsPaginatedQuery(request.Page, request.PageSize, filters, sorts, request.JoinOperator), ct);
        await Send.OkAsync(result.Value);
    }
}
