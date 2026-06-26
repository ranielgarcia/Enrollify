using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Application.SharedDTOs;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class CurriculumToCurriculumDetailDtoProjectionSpec : Specification<Curriculum, CurriculumDetailDto>
{
    public CurriculumToCurriculumDetailDtoProjectionSpec()
    {
        Query
        .AsNoTracking()
        .AsSplitQuery()
        .Select(c => new CurriculumDetailDto
        {
            Id = c.Id,
            EffectiveYear = c.EffectiveYear.Value,
            Version = c.Version,
            Status = c.StatusId,
            Course = c.Course != null ? new CourseSummaryDto
            {
                Id = c.Course.Id,
                Name = c.Course.Name
            } : null,
            Description = c.Description,
            ApprovedDate = c.ApprovedDate,
            CurriculumSubjects = c.CurriculumSubjects
                .Where(cs => cs.IsActive)
                .Select(cs => new CurriculumSubjectDto
                {
                    Id = cs.Id,
                    SubjectId = cs.SubjectId,
                    YearLevel = cs.YearLevel,
                    TermNumber = cs.TermNumber,
                    UnitsOverride = cs.SubjectUnitsOverride,
                    IsElective = cs.IsElective,
                    ElectiveGroupName = cs.ElectiveGroupName,
                    Subject = cs.Subject != null ? new SubjectSummaryDto
                    {
                        Id = cs.Subject.Id,
                        Code = cs.Subject.Code,
                        Title = cs.Subject.Title,
                        Units = cs.Subject.Units
                    } : null,
                    Prerequisites = cs.Prerequisites
                        .Where(p => p.IsActive)
                        .Select(p => new CurriculumSubjectPrerequisiteDto
                        {
                            PrerequisiteCurriculumSubjectId = p.PrerequisiteCurriculumSubjectId,
                            MinimumGrade = p.MinimumGrade
                        })
                        .ToList()
                })
                .ToList()
        });
    }
}
