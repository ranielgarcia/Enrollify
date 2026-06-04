using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate.Models;

public class ClassSectionSubjectOfferingForCreation
{
  public required ClassSectionId ClassSectionId { get; set; }
  public required SubjectId SubjectId { get; set; }
  public decimal? SubjectUnitsOverride { get; set; }
  public TeacherId? TeacherId { get; set; }
  public RoomId? RoomId { get; set; }

  public int DaysPerWeek { get; set; } = 1; // default 1 day per week
  public decimal HoursPerDay { get; set; } = 1; // default 1 hour per day
  public int? MaxNumberOfStudents { get; set; }


  public required CurriculumSubjectId CurriculumSubjectId { get; set; }
  public required SubjectCode SnapshotSubjectCode { get; set; }
  public required string SnapshotSubjectTitle { get; set; }
  public required decimal SnapshotUnits { get; set; }
  public required bool SnapshotIsElective { get; set; }
  public string? SnapshotElectiveGroupName { get; set; }
}
