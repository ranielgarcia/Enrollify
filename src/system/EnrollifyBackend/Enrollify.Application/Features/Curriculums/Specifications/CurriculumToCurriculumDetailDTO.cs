using Ardalis.Specification;
using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class CurriculumToCurriculumDetailDTO : Specification<Curriculum, CurriculumDetailDTO>
{
    public CurriculumToCurriculumDetailDTO()
    {
        Query
        .AsNoTracking()
        .AsSplitQuery()
        .Select(c => new CurriculumDetailDTO
        {
            Id = c.Id,
            EffectiveYear = c.EffectiveYear,
            Version = c.Version,
            Status = c.StatusId,
            Course = c.Course != null ? new CourseSummaryDTO
            {
                Id = c.Course.Id,
                Name = c.Course.Name
            } : null,
            Description = c.Description,
            ApprovedDate = c.ApprovedDate,
            CurriculumSubjects = c.CurriculumSubjects
                .Where(cs => cs.IsActive)
                .Select(cs => new CurriculumSubjectDTO
                {
                    Id = cs.Id,
                    SubjectId = cs.SubjectId,
                    YearLevel = cs.YearLevel,
                    TermNumber = cs.TermNumber,
                    IsElective = cs.IsElective,
                    ElectiveGroupName = cs.ElectiveGroupName,
                    Subject = cs.Subject != null ? new SubjectSummaryDTO
                    {
                        Id = cs.Subject.Id,
                        Code = cs.Subject.Code,
                        Title = cs.Subject.Title,
                        Units = cs.Subject.Units
                    } : null,
                    Prerequisites = cs.Prerequisites
                        .Where(p => p.IsActive)
                        .Select(p => new CurriculumSubjectPrerequisiteDTO
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
