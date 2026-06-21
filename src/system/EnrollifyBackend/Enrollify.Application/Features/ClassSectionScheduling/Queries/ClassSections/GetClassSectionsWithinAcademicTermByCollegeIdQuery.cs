using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.SchedulingStats;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;

public record GetClassSectionsWithinAcademicTermByCollegeIdQuery(CollegeId CollegeId, AcademicTermId AcademicTermId)
  : IRequest<Result<CollegeCoursesWithClassSectionsDto>>;

public class GetClassSectionsWithinAcademicTermByCollegeIdQueryHandler :
  IRequestHandler<GetClassSectionsWithinAcademicTermByCollegeIdQuery, Result<CollegeCoursesWithClassSectionsDto>>
{
  private readonly IReadRepository<College> _collegeReadRepository;
  private readonly IReadRepository<Course> _courseReadRepository;
  private readonly IReadRepository<ClassSection> _classSectionReadRepository;
  private readonly IReadRepository<ClassSectionSchedulingStats> _schedulingStatsReadRepository;

  public GetClassSectionsWithinAcademicTermByCollegeIdQueryHandler(
    IReadRepository<College> collegeReadRepository,
    IReadRepository<Course> courseReadRepository,
    IReadRepository<ClassSection> classSectionReadRepository,
    IReadRepository<ClassSectionSchedulingStats> schedulingStatsReadRepository)
  {
    _collegeReadRepository = collegeReadRepository;
    _courseReadRepository = courseReadRepository;
    _classSectionReadRepository = classSectionReadRepository;
    _schedulingStatsReadRepository = schedulingStatsReadRepository;
  }

  public async Task<Result<CollegeCoursesWithClassSectionsDto>> Handle(
    GetClassSectionsWithinAcademicTermByCollegeIdQuery request,
    CancellationToken cancellationToken)
  {
    College? college = await _collegeReadRepository.GetByIdAsync(request.CollegeId, cancellationToken);
    if (college is null)
      return Result.NotFound($"College with ID {request.CollegeId} not found.");

    List<Course> courses =
      await _courseReadRepository.ListAsync(new GetCoursesByCollegeIdSpec(college.Id), cancellationToken);

    if (!courses.Any())
      return Result.NotFound("No courses found for college.");

    // ### Class Section Scheduling Validation Issue Stats
    var schedulingStatsSpec = new GetClassSchedulingStatsForCollegeWithinAcademicTermSpec(
      courses.Select(c => c.Id).ToList(),
      request.AcademicTermId);

    List<ClassSectionSchedulingStats> schedulingStatsList =
      await _schedulingStatsReadRepository.ListAsync(schedulingStatsSpec, cancellationToken);

    var schedulingStatsPerClassSection = schedulingStatsList.Where(x => x.ClassSectionId != null)
      .GroupBy(x => x.ClassSectionId)
      .ToDictionary(g => g.Key!, g => g.ToList());

    // ### Class Sections for courses
    List<ClassSection> allClassSectionsForCourses = await _classSectionReadRepository
      .ListAsync(
        new GetClassSectionsWithinAcademicTermAndByCourseIdsSpec(request.AcademicTermId,
          courses.Select(c => c.Id).ToList()), cancellationToken);

    if (!allClassSectionsForCourses.Any())
      return Result.NotFound("No class-sections found for college.");

    var classSectionsPerCourse = allClassSectionsForCourses.GroupBy(cs => cs.CourseId)
      .ToDictionary(g => g.Key, g => g.ToList());

    var coursesWithSectionsDto = courses.Select(course =>
    {
      classSectionsPerCourse.TryGetValue(course.Id, out List<ClassSection>? sectionsForCourse);

      List<ClassSectionDto> sectionDtos = sectionsForCourse != null
        ? sectionsForCourse.Select(section =>
        {
          List<ClassSectionSchedulingStats> sectionSchedulingStats =
            schedulingStatsPerClassSection.TryGetValue(section.Id, out List<ClassSectionSchedulingStats>? stats)
              ? stats
              : new List<ClassSectionSchedulingStats>();
          return ClassSectionDto.FromEntity(section, sectionSchedulingStats);
        }).ToList()
        : new List<ClassSectionDto>();

      return CoursesWithClassSectionsDto.FromEntity(course, sectionDtos);
    }).ToList();

    var collegeCoursesWithClassSectionsDto =
      CollegeCoursesWithClassSectionsDto.FromEntity(college, coursesWithSectionsDto);

    return Result.Success(collegeCoursesWithClassSectionsDto);
  }
}
