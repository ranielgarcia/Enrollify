using Ardalis.Result;
using Enrollify.Application.Subjects.DTOs;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public class ListSubjectPrerequisitesQuery : IQuery<Result<List<SubjectDTO>>>
{
    public SubjectId SubjectId { get; set; }
}

public class ListSubjectPrerequisitesQueryHandler : IQueryHandler<ListSubjectPrerequisitesQuery, Result<List<SubjectDTO>>>
{
    private readonly ISubjectRepository _subjectRepository;

    public ListSubjectPrerequisitesQueryHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }
    public async ValueTask<Result<List<SubjectDTO>>> Handle(ListSubjectPrerequisitesQuery request, CancellationToken cancellationToken)
    {
        var prerequisitesSubjects = await _subjectRepository.ListSubjectPrerequisite(request.SubjectId, cancellationToken);
        var toReturn = prerequisitesSubjects
            .Select(s => SubjectDTO.FromEntity(s!))
            .ToList();
        return toReturn;
    }
}