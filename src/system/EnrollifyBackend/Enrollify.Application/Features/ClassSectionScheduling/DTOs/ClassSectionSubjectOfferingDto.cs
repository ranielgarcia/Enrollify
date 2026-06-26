using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public class ClassSectionSubjectOfferingDto : BaseDto
{
  public int Id { get; set; }
  public int ClassSectionId { get; set; }
  public int SubjectId { get; set; }

  public string SnapshotSubjectCode { get; set; } = null!;
  public string SnapshotSubjectTitle { get; set; } = null!;
  public decimal SnapshotUnits { get; set; }
  public bool SnapshotIsElective { get; set; }
  public string? SnapshotElectiveGroupName { get; set; }

  public int DaysPerWeek { get; set; }
  public decimal HoursPerDay { get; set; }
  public int? MaxNumberOfStudents { get; set; }
  public bool IsFullyScheduled { get; set; }

  public OfferingTeacherDto? Teacher { get; set; }
  public OfferingRoomDto? Room { get; set; }
  public IReadOnlyList<ClassScheduleDto> Schedules { get; set; } = [];
  public IReadOnlyList<ClassSectionValidationIssueDto> ValidationIssues { get; set; } = [];
  public int TotalIssues => ValidationIssues.Count;

  public static ClassSectionSubjectOfferingDto FromEntity(ClassSectionSubjectOffering offering,
    List<ClassSectionValidationIssue> validationIssues)
  {
    return new ClassSectionSubjectOfferingDto
    {
      Id = offering.Id.Value,
      ClassSectionId = offering.ClassSectionId.Value,
      SubjectId = offering.SubjectId.Value,

      SnapshotSubjectCode = (string)offering.SnapshotSubjectCode,
      SnapshotSubjectTitle = offering.SnapshotSubjectTitle,
      SnapshotUnits = offering.SnapshotUnits,
      SnapshotIsElective = offering.SnapshotIsElective,
      SnapshotElectiveGroupName = offering.SnapshotElectiveGroupName,

      DaysPerWeek = offering.DaysPerWeek,
      HoursPerDay = offering.HoursPerDay,
      MaxNumberOfStudents = offering.MaxNumberOfStudents,
      IsFullyScheduled = offering.IsFullyScheduled(),

      Teacher = offering.Teacher is not null
        ? new OfferingTeacherDto
        {
          Id = offering.Teacher.Id.Value,
          FirstName = offering.Teacher.FirstName,
          LastName = offering.Teacher.LastName,
          Email = (string)offering.Teacher.Email
        }
        : null,

      Room = offering.Room is not null
        ? new OfferingRoomDto
        {
          Id = offering.Room.Id.Value,
          RoomNumber = offering.Room.RoomNumber,
          Building = new OfferingRoomBuildingDto { Name = offering.Room?.Building?.Name }
        }
        : null,

      Schedules = offering.ClassSchedules
        .Select(ClassScheduleDto.FromEntity)
        .ToList()
        .AsReadOnly(),

      ValidationIssues = validationIssues.Select(ClassSectionValidationIssueDto.FromEntity).ToList().AsReadOnly(),

      CreatedAt = offering.CreatedAt,
      CreatedBy = BaseUserDto.FromUser(offering.CreatedByUser),
      UpdatedAt = offering.UpdatedAt,
      UpdatedBy = offering.UpdatedByUser is not null ? BaseUserDto.FromUser(offering.UpdatedByUser) : null,
      IsActive = offering.IsActive
    };
  }
}

public class OfferingTeacherDto
{
  public int Id { get; set; }
  public string FirstName { get; set; } = null!;
  public string LastName { get; set; } = null!;
  public string Email { get; set; } = null!;
}

public class OfferingRoomDto
{
  public int Id { get; set; }
  public string RoomNumber { get; set; } = null!;
  public OfferingRoomBuildingDto Building { get; set; } = null!;
}

public class OfferingRoomBuildingDto
{
  public string? Name { get; set; }
}
