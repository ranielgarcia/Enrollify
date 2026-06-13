using System.Text.Json;
using Enrollify.Application;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;
using Enrollify.Application.Filtering;

namespace Enrollify.WebAPI.Features.ClassSections;

public class FilterClassSectionsPaginatedRequest
{
  public required int Page { get; set; }
  public required int PageSize { get; set; }

  [QueryParam] public string? Filters { get; set; }

  [QueryParam] public string? Sort { get; set; }

  [QueryParam] public string? JoinOperator { get; set; }

  [QueryParam] public int? AcademicYearId { get; set; }
}

public class FilterClassSectionsPaginatedRequestValidator : Validator<FilterClassSectionsPaginatedRequest>
{
  public FilterClassSectionsPaginatedRequestValidator()
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
          List<FilterItem>? result = JsonSerializer.Deserialize<List<FilterItem>>(filters!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
          List<SortItem>? result = JsonSerializer.Deserialize<List<SortItem>>(sort!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

    RuleFor(x => x.AcademicYearId)
      .GreaterThan(0).WithMessage("AcademicYearId must be greater than 0.")
      .When(x => x.AcademicYearId.HasValue);
  }
}

[HttpGet("filter/{page}/{pageSize}")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewClassSectionsPermission)]
public class FilterClassSectionsPaginatedEndpoint(IMediator mediator)
  : Endpoint<FilterClassSectionsPaginatedRequest, Application.PagedResult<ClassSectionDto>>
{
  public override async Task HandleAsync(FilterClassSectionsPaginatedRequest req, CancellationToken ct)
  {
    List<FilterItem>? filters = string.IsNullOrEmpty(req.Filters)
      ? null
      : JsonSerializer.Deserialize<List<FilterItem>>(req.Filters!,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    List<SortItem>? sorts = string.IsNullOrEmpty(req.Sort)
      ? null
      : JsonSerializer.Deserialize<List<SortItem>>(req.Sort!,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Ardalis.Result.Result<PagedResult<ClassSectionDto>> result = await mediator.Send(
      new FilterClassSectionsPaginatedQuery(req.Page, req.PageSize, req.AcademicYearId, filters, sorts,
        req.JoinOperator), ct);

    await Send.OkAsync(result, ct);
  }
}
