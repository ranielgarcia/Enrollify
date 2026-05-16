using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.WebAPI.Features.ClassSections;

public class BulkInitializePayloadRequest
{
    public int CourseId { get; set; }
    public int CurriculumId { get; set; }
    public int NumberOfSections { get; set; }
}

public class BulkInitializeClassSectionsRequest
{
    public int AcademicTermId { get; set; }
    public int YearLevel { get; set; }
    public List<BulkInitializePayloadRequest> RequestPayload { get; set; } = new();
}

public class BulkInitializeClassSectionsRequestValidator : Validator<BulkInitializeClassSectionsRequest>
{
    public BulkInitializeClassSectionsRequestValidator()
    {
        RuleFor(x => x.AcademicTermId)
            .NotEmpty().WithMessage("Academic term is required.");

        RuleFor(x => x.YearLevel)
            .GreaterThan(0).WithMessage("Year level must be greater than zero.")
            .LessThanOrEqualTo(6).WithMessage("Year level must be 6 or less.");

        RuleFor(x => x.RequestPayload)
            .NotEmpty().WithMessage("At least one course is required.");

        RuleForEach(x => x.RequestPayload)
            .ChildRules(payload =>
            {
                payload.RuleFor(x => x.CourseId)
                    .NotEmpty().WithMessage("Course ID is required.");

                payload.RuleFor(x => x.CurriculumId)
                    .NotEmpty().WithMessage("Curriculum ID is required.");

                payload.RuleFor(x => x.NumberOfSections)
                    .GreaterThan(0).WithMessage("Number of sections must be greater than zero.")
                    .LessThanOrEqualTo(26).WithMessage("Number of sections cannot exceed 26 (A-Z).");
            });
    }
}

[HttpPost("bulk-initialize")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateClassSectionPermission)]
public class BulkInitializeClassSectionsEndpoint : Endpoint<BulkInitializeClassSectionsRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public BulkInitializeClassSectionsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult> ExecuteAsync(
        BulkInitializeClassSectionsRequest request, 
        CancellationToken cancellationToken)
    {
        var payloads = request.RequestPayload.Select(p => 
            new BulkInitializeClassSectionsForAcademicYear.Payload(
                CourseId.From(p.CourseId),
                CurriculumId.From(p.CurriculumId),
                p.NumberOfSections))
            .ToList();

        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(request.AcademicTermId),
            YearLevel.From(request.YearLevel),
            payloads);

        var result = await _mediator.Send(command, cancellationToken);

        return result.ToDeleteResult();
    }
}
