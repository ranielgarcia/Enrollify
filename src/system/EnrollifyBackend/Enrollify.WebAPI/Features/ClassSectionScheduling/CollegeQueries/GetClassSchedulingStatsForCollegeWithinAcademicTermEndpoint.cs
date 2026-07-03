using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Queries.SchedulingStats;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.CollegeQueries;

public class GetClassSchedulingStatsForCollegeWithinAcademicTermRequest
{
  public required int CollegeId { get; set; }
  [QueryParam] public required int AcademicTermId { get; set; }
}

public class GetClassSchedulingStatsForCollegeWithinAcademicTermRequestValidator
  : Validator<GetClassSchedulingStatsForCollegeWithinAcademicTermRequest>
{
  public GetClassSchedulingStatsForCollegeWithinAcademicTermRequestValidator()
  {
    RuleFor(x => x.CollegeId)
      .GreaterThan(0).WithMessage("CollegeId must be greater than 0.");

    RuleFor(x => x.AcademicTermId)
      .GreaterThan(0).WithMessage("AcademicTermId must be greater than 0.");
  }
}

[HttpGet("{collegeId}/stats")]
[Group<CollegesEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasViewClassSectionsPermission)]
public class GetClassSchedulingStatsForCollegeWithinAcademicTermEndpoint(IMediator mediator)
  : Endpoint<GetClassSchedulingStatsForCollegeWithinAcademicTermRequest, OkOrNotFoundApiResult<Dictionary<string, int>>>
{
  public override async Task<OkOrNotFoundApiResult<Dictionary<string, int>>>
    ExecuteAsync(GetClassSchedulingStatsForCollegeWithinAcademicTermRequest req, CancellationToken ct)
  {

    Result<Dictionary<string, int>> toReturn = await mediator.Send(
      new GetClassSchedulingStatsForCollegeWithinAcademicTermQuery(
        CollegeId.From(req.CollegeId),
        AcademicTermId.From(req.AcademicTermId)), ct);

    return toReturn.ToGetByIdResult(dto => dto);
  }
}
