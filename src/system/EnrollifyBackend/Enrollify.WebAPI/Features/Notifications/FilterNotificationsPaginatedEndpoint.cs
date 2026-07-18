using System.Text.Json;
using Enrollify.Application.Features.Notifications.DTOs;
using Enrollify.Application.Features.Notifications.Queries;
using Enrollify.Application.Filtering;

namespace Enrollify.WebAPI.Features.Notifications;

public class FilterNotificationsPaginatedRequest
{
  public string? SearchTerm { get; set; }
  public required int Page { get; set; }

  public required int PageSize { get; set; }

  [QueryParam]
  public string? Filters { get; set; }
}

public class FilterNotificationsPaginatedRequestValidator : Validator<FilterNotificationsPaginatedRequest>
{
  public FilterNotificationsPaginatedRequestValidator()
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
  }
}

[HttpGet("filter/{page}/{pageSize}")]
[Group<NotificationEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewNotificationPermission)]
public class FilterNotificationsPaginatedEndpoint (IMediator mediator)
  : Endpoint<FilterNotificationsPaginatedRequest, Application.PagedResult<NotificationDto>>
{
  public override async Task HandleAsync(FilterNotificationsPaginatedRequest req, CancellationToken ct)
  {
    var filters = string.IsNullOrEmpty(req.Filters)
      ? null
      : JsonSerializer.Deserialize<List<FilterItem>>(req.Filters!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    var result = await mediator.Send(new FilterNotificationsPaginatedQuery(req.SearchTerm ?? "", req.Page, req.PageSize, filters), ct);
    await Send.OkAsync(result, ct);
  }
}
