namespace Enrollify.Application.Features.Curriculums.Specifications;

/// <summary>
/// Specification for filtering curriculum by ID.
/// </summary>
public class GetCurriculumByIdSpec : Specification<Curriculum>
{
    /// Note: When used with WithProjectionOf(), Includes are not needed as
    /// EF Core generates optimized SQL from the Select projection.
    /// AsNoTracking is required to avoid owned entity tracking issues with ValueObjects.
    public GetCurriculumByIdSpec(CurriculumId id) =>
        Query
            .AsNoTracking()
            .Where(c => c.Id == id);
}
