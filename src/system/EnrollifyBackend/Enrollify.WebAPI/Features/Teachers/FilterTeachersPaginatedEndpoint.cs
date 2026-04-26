using System.Text.Json;
using Enrollify.Application.Features.Teachers.DTOs;
using Enrollify.Application.Features.Teachers.Queries;
using Enrollify.Application.Filtering;

namespace Enrollify.WebAPI.Features.Teachers;


public class FilterTeachersPaginatedRequest
{

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int Page { get; set; }

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int PageSize { get; set; }

    [QueryParam]
    public string? Filters { get; set; }

    [QueryParam]
    public string? Sort { get; set; }

    [QueryParam]
    public string? JoinOperator { get; set; }
}


public class FilterTeachersPaginatedRequestValidator : Validator<FilterTeachersPaginatedRequest>
{
    public FilterTeachersPaginatedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be 100 Or fewer.");
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
            .Must(jo => jo != null && (jo.ToLower().Trim() == "And" || jo.ToLower().Trim() == "Or"))
            .When(x => !string.IsNullOrEmpty(x.JoinOperator));
    }
}

[HttpGet("filter/{page}/{pageSize}")]
[Group<TeacherEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewTeacherPermission)]
public class FilterTeachersPaginatedEndpoint (IMediator mediator)
    : Endpoint<FilterTeachersPaginatedRequest, Application.PagedResult<TeacherDto>>
{
    public override async Task HandleAsync(FilterTeachersPaginatedRequest req, CancellationToken ct)
    {
        var filters = string.IsNullOrEmpty(req.Filters)
            ? null
            : JsonSerializer.Deserialize<List<FilterItem>>(req.Filters!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var sorts = string.IsNullOrEmpty(req.Sort)
            ? null
            : JsonSerializer.Deserialize<List<SortItem>>(req.Sort!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var result = await mediator.Send(new FilterTeachersPaginatedQuery(req.Page, req.PageSize, filters, sorts, req.JoinOperator), ct);
        await Send.OkAsync(result, ct);
    }
}
