namespace Enrollify.Application.Features.Subjects.Specifications;

public class ListSubjectsPaginatedSpec : Specification<Subject>
{
    public ListSubjectsPaginatedSpec(int pageNumber, int pageSize) =>
        Query
            .AsNoTracking()
            .Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser)
            .Include(s => s.PreferRoomType)
            .OrderBy(s => s.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
}
