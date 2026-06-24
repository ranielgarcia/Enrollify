using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.CollegeQueries;

public class GetClassSectionsWithinAcademicTermByCollegeRequest
{
  public required int CollegeId { get; set; }
  [QueryParam] public required int AcademicTermId { get; set; }
}

public class
  GetClassSectionsWithinAcademicTermByCollegeRequestValidator : Validator<
  GetClassSectionsWithinAcademicTermByCollegeRequest>
{
  public GetClassSectionsWithinAcademicTermByCollegeRequestValidator()
  {
    RuleFor(x => x.CollegeId)
      .GreaterThan(0).WithMessage("CollegeId must be greater than 0.");

    RuleFor(x => x.AcademicTermId)
      .GreaterThan(0).WithMessage("AcademicTermId must be greater than 0.");
  }
}

[HttpGet("{collegeId}/class-sections")]
[Group<CollegesEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasViewClassSectionsPermission)]
public class GetClassSectionsWithinAcademicTermByCollegeEndpoint(IMediator mediator)
  : Endpoint<GetClassSectionsWithinAcademicTermByCollegeRequest, CollegeCoursesWithClassSectionsDto>
{
  public override async Task HandleAsync(GetClassSectionsWithinAcademicTermByCollegeRequest req, CancellationToken ct)
  {
    Result<CollegeCoursesWithClassSectionsDto> toReturn = await mediator.Send(
      new GetClassSectionsWithinAcademicTermByCollegeIdQuery(
        CollegeId.From(req.CollegeId),
        AcademicTermId.From(req.AcademicTermId)), ct);

    await Send.OkAsync(toReturn, ct);
  }
}
