using Enrollify.Application.Features.Curriculums.Commands;
using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Term = int;
using Year = int;

namespace Enrollify.WebAPI.Features.Curriculums;

public class SubjectInCurriculum
{
    public string Code { get; set; } = null!;
    public decimal? UnitsOverride { get; set; }

    public int DaysPerWeek { get; set; } = 1;
    public decimal HoursPerDay { get; set; } = 1;

    public string[] Prerequisites { get; set; } = [];
}

public class CurriculumContentRequest
{
    public int CurriculumId { get; set; }
    public Dictionary<Year, Dictionary<Term, SubjectInCurriculum[]>> Grid { get; set; } = new Dictionary<Year, Dictionary<Term, SubjectInCurriculum[]>>();
}


[HttpPut("{curriculumId}/save-content")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateCurriculumPermission)]
public class SaveCurriculumContentEndpoint (IMediator mediator)
    : Endpoint<CurriculumContentRequest, OkOrNotFoundApiResult<CurriculumDto>>
{
    public override async Task<OkOrNotFoundApiResult<CurriculumDto>>
        ExecuteAsync(CurriculumContentRequest request, CancellationToken ct)
    {
        var subjectsGrid = new Dictionary<Year, Dictionary<Term, SaveCurriculumContent.SubjectInCurriculum[]>>();

        foreach (var (year, terms) in request.Grid)
        {
            var termDict = new Dictionary<Term, SaveCurriculumContent.SubjectInCurriculum[]>();
            foreach (var (term, subjects) in terms)
            {
                var subjectArray = subjects.Select(s => new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = SubjectCode.From(s.Code),
                    UnitsOverride = s.UnitsOverride,
                    DaysPerWeek = s.DaysPerWeek,
                    HoursPerDay = s.HoursPerDay,
                    Prerequisites = s.Prerequisites.Select(p => SubjectCode.From(p)).ToArray()
                }).ToArray();
                termDict[term] = subjectArray;
            }
            subjectsGrid[year] = termDict;
        }

        var result = await mediator.Send(new SaveCurriculumContent
            .Command(CurriculumId.From(request.CurriculumId), subjectsGrid), ct);

        return result.ToUpdatedResult(id => result.Value);
    }
}
