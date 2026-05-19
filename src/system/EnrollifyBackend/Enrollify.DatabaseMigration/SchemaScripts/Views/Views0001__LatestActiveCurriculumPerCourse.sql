-- View: returns the single latest active curriculum per course.
-- "Latest" is defined as the highest EffectiveYear among curriculums
-- where IsActive = 1 and StatusId corresponds to 'Active' in CurriculumStatuses.
-- No course will appear more than once in this view.
CREATE VIEW vw_LatestActiveCurriculumPerCourse AS
SELECT
    Id As CurriculumId,
    CourseId,
    EffectiveYear,
    Version,
    StatusId,
    Description,
    ApprovedDate
FROM (
    SELECT
        c.*,
        ROW_NUMBER() OVER (
            PARTITION BY c.CourseId
            ORDER BY c.EffectiveYear DESC, c.Version DESC, c.ApprovedDate DESC, c.Id DESC
        ) AS rn
    FROM Curriculums c
    WHERE c.IsActive = 1
      AND c.StatusId = (SELECT Id FROM CurriculumStatuses WHERE Name = 'Active')
) ranked
WHERE rn = 1;
GO
