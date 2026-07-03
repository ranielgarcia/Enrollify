namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionsByIdsInOpenOrHigherStatusSpec : Specification<ClassSection>
{
    public GetClassSectionsByIdsInOpenOrHigherStatusSpec(IEnumerable<ClassSectionId> ids)
    {
        Query.Where(cs => ids.Contains(cs.Id)
            && cs.StatusId != ClassSectionStatusEnum.Draft
            && cs.StatusId != ClassSectionStatusEnum.Cancelled);
    }
}
