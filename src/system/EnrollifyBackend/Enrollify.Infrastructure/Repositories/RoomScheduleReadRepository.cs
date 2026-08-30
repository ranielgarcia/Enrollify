using Dapper;
using Enrollify.Application.Features.RoomScheduling.Models;
using Enrollify.Application.Features.RoomScheduling.Repositories;
using Enrollify.Core.Constants;
using Enrollify.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Enrollify.Infrastructure.Repositories;

public class RoomScheduleReadRepository : IRoomScheduleReadRepository
{
  private readonly IDbConnectionFactory _connectionFactory;

  public RoomScheduleReadRepository(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<RoomScheduleQueryResult> GetRoomScheduleDataAsync(
    RoomScheduleQueryFilter filter,
    CancellationToken cancellationToken)
  {
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(cancellationToken);

    const string roomsSql = @"
            SELECT
                r.Id AS Id,
                r.RoomNumber AS RoomNumber,
                r.Capacity AS Capacity,
                rt.Name AS RoomTypeName,
                b.Id AS BuildingId,
                b.Name AS BuildingName,
                col.Id AS CollegeId,
                col.Code AS CollegeCode
            FROM Rooms r
            LEFT JOIN RoomTypes rt ON rt.Id = r.RoomTypeId
            LEFT JOIN Buildings b ON b.Id = r.BuildingId
            LEFT JOIN Colleges col ON col.Id = b.CollegeId
            WHERE r.IsActive = 1
              AND (@BuildingId IS NULL OR r.BuildingId = @BuildingId)
              AND (@RoomTypeId IS NULL OR r.RoomTypeId = @RoomTypeId)
              AND (@CollegeId IS NULL OR b.CollegeId = @CollegeId)
            ORDER BY b.Name, r.RoomNumber";

    const string offeringsSql = @"
            SELECT
                cs.Id AS ScheduleId,
                o.Id AS OfferingId,
                sec.Id AS SectionId,
                CONCAT(sec.Name, '-', sec.IntendedYearLevel, sec.SectionCode) AS SectionName,
                sec.CourseId AS CourseId,
                c.Code AS CourseCode,
                sec.AcademicTermId AS AcademicTermId,
                o.RoomId AS RoomId,
                o.TeacherId AS TeacherId,
                t.FirstName AS TeacherFirstName,
                t.LastName AS TeacherLastName,
                o.SubjectId AS SubjectId,
                o.SnapshotSubjectCode AS SubjectCode,
                o.SnapshotSubjectTitle AS SubjectTitle,
                cs.DayOfWeek AS DayOfWeek,
                cs.StartTime AS StartTime,
                cs.EndTime AS EndTime
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            INNER JOIN ClassSections sec ON sec.Id = o.ClassSectionId
            INNER JOIN Courses c ON c.Id = sec.CourseId
            LEFT JOIN Teachers t ON t.Id = o.TeacherId
            LEFT JOIN Rooms r ON r.Id = o.RoomId
            LEFT JOIN Buildings b ON b.Id = r.BuildingId
            WHERE cs.IsActive = 1
              AND o.IsActive = 1
              AND sec.IsActive = 1
              AND sec.AcademicTermId = @AcademicTermId
              AND sec.StatusId != @ExcludeCancelledStatus
              AND cs.DayOfWeek = @DayOfWeek
              AND (@CourseId IS NULL OR sec.CourseId = @CourseId)
              AND (
                  o.RoomId IS NULL
                  OR (
                      (@BuildingId IS NULL OR r.BuildingId = @BuildingId)
                      AND (@RoomTypeId IS NULL OR r.RoomTypeId = @RoomTypeId)
                      AND (@CollegeId IS NULL OR b.CollegeId = @CollegeId)
                  )
              )";

    var parameters = new
    {
      filter.AcademicTermId,
      filter.DayOfWeek,
      filter.BuildingId,
      filter.RoomTypeId,
      filter.CollegeId,
      filter.CourseId,
      ExcludeCancelledStatus = ClassSectionStatusEnum.Cancelled.Value
    };

    IEnumerable<RoomScheduleRoomRow> rooms =
      await conn.QueryAsync<RoomScheduleRoomRow>(new CommandDefinition(roomsSql, parameters, cancellationToken: cancellationToken));

    IEnumerable<RoomScheduleOfferingRow> offerings =
      await conn.QueryAsync<RoomScheduleOfferingRow>(new CommandDefinition(offeringsSql, parameters, cancellationToken: cancellationToken));

    return new RoomScheduleQueryResult(rooms.ToList(), offerings.ToList());
  }
}
