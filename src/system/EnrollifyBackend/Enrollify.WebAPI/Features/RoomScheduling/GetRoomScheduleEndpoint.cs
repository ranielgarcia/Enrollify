using Ardalis.Result;
using Enrollify.Application.Features.RoomScheduling.DTOs;
using Enrollify.Application.Features.RoomScheduling.Queries;

namespace Enrollify.WebAPI.Features.RoomScheduling;

public class GetRoomScheduleRequest
{
  [QueryParam] public required int AcademicTermId { get; set; }
  [QueryParam] public required string DayOfWeek { get; set; } = string.Empty;
  [QueryParam] public int? BuildingId { get; set; }
  [QueryParam] public int? RoomTypeId { get; set; }
  [QueryParam] public int? CollegeId { get; set; }
  [QueryParam] public int? CourseId { get; set; }
}

public class GetRoomScheduleRequestValidator : Validator<GetRoomScheduleRequest>
{
  private static readonly string[] ValidDays = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];

  public GetRoomScheduleRequestValidator()
  {
    RuleFor(x => x.AcademicTermId)
      .GreaterThan(0).WithMessage("AcademicTermId must be greater than 0.");

    RuleFor(x => x.DayOfWeek)
      .NotEmpty().WithMessage("DayOfWeek is required.")
      .Must(d => ValidDays.Contains(d, StringComparer.OrdinalIgnoreCase))
      .WithMessage("DayOfWeek must be one of: SUN, MON, TUE, WED, THU, FRI, SAT.");

    RuleFor(x => x.BuildingId).GreaterThan(0).When(x => x.BuildingId.HasValue);
    RuleFor(x => x.RoomTypeId).GreaterThan(0).When(x => x.RoomTypeId.HasValue);
    RuleFor(x => x.CollegeId).GreaterThan(0).When(x => x.CollegeId.HasValue);
    RuleFor(x => x.CourseId).GreaterThan(0).When(x => x.CourseId.HasValue);
  }
}

[HttpGet("schedule")]
[Group<Rooms.RoomsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewRoomsPermission)]
public class GetRoomScheduleEndpoint(IMediator mediator)
  : Endpoint<GetRoomScheduleRequest, OkOrNotFoundApiResult<RoomScheduleDto>>
{
  public override async Task<OkOrNotFoundApiResult<RoomScheduleDto>> ExecuteAsync(
    GetRoomScheduleRequest req, CancellationToken ct)
  {
    Result<RoomScheduleDto> result = await mediator.Send(
      new GetRoomScheduleForTermQuery(
        req.AcademicTermId,
        req.DayOfWeek.ToUpperInvariant(),
        req.BuildingId,
        req.RoomTypeId,
        req.CollegeId,
        req.CourseId),
      ct);

    return result.ToGetByIdResult(dto => dto);
  }
}
