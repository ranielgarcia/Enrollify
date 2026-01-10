using Semester = int;
using Year = int;

namespace Enrollify.WebAPI.Features.Curriculums;

public class SubjectInCurriculum
{
    public string Code { get; set; } = null!;
    public string[] Prerequisites { get; set; } = [];
}

public class CurriculumContentRequest
{
    [Microsoft.AspNetCore.Mvc.FromRoute]
    public int CurriculumId { get; set; }
    public Dictionary<Year, Dictionary<Semester, SubjectInCurriculum[]>> Grid { get; set; } = new Dictionary<Year, Dictionary<Semester, SubjectInCurriculum[]>>();
}


[HttpPut("{curriculumId}/save-content")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateCurriculumPermission)]
public class SaveCurriculumnContentEndpoint
    : Endpoint<CurriculumContentRequest, CurriculumContentRequest>
    //: Endpoint<CurriculumContentRequest, Results<Created<CurriculumContentRequest>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    //public override async Task<Results<Created<CurriculumContentRequest>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
    //    ExecuteAsync(CurriculumContentRequest request, CancellationToken ct)
    //{
    //    return Send.OkAsync(request);
    //}

    public override async Task HandleAsync(CurriculumContentRequest request, CancellationToken c)
    {
        var curriculumId = request.CurriculumId;
        await Send.OkAsync(request);
    }
}
