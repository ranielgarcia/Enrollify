using System.Text.Json;
using Enrollify.Application.Features.Notifications.DTOs;
using Enrollify.Application.Features.Notifications.Queries;
using Enrollify.Application.Filtering;
using Enrollify.Core.Constants;

namespace Enrollify.WebAPI.Features.Notifications;

public class FilterNotificationsPaginatedRequest
{
  [QueryParam]
  public string? SearchTerm { get; set; }
  public required int Page { get; set; }

  public required int PageSize { get; set; }

  [QueryParam]
  public string? Category { get; set; }

  [QueryParam]
  public string? Severity { get; set; }

  [QueryParam]
  public string? ReadState { get; set; }

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
    NotificationCategoryEnum? category = req.Category != null && req.Category.ToLower() != "all" ? NotificationCategoryEnum.FromName(req.Category) : null;
    NotificationSeverityEnum? severity = req.Severity != null && req.Severity.ToLower() != "all" ? NotificationSeverityEnum.FromName(req.Severity) : null;

    var result = await mediator.Send(new FilterNotificationsPaginatedQuery(req.SearchTerm ?? "", req.Page, req.PageSize, category, severity, req.ReadState), ct);
    await Send.OkAsync(result, ct);
  }
}
