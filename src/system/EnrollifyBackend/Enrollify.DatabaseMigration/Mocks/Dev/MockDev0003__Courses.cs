using DbUp.Engine;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Mocks.Dev;

public class MockDev0003__Courses : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var courses = new List<Course>()
        {
            new Course(CollegeCode.From("CCS"), CourseCode.From("BSIT"), "Bachelor of Science in Information Technology", 4, "Information Technology is the study of utilization of both hardware and software technologies to provide computing solutions that address the needs of various users and organizations."),
            new Course(CollegeCode.From("CCS"), CourseCode.From("BSCS"), "Bachelor of Science in Computer Science", 4, "Bachelor of Science in Computer Science (BSCS) is a four-year program that includes the study of computing concepts and theories, algorithmic foundations, and new developments in computing.")
        };

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            """);

        Dictionary<CollegeCode, int> colleges = new Dictionary<CollegeCode, int>();

        scriptBuilder.Append("""
                MERGE [Courses] AS [Target]
                USING ( VALUES
            """);

        var values = new List<string>();
        foreach (var course in courses)
        {
            if (!colleges.TryGetValue(course.collegeCode, out int collegeId))
            {
                var getCollegeCommand = dbCommandFactory();
                getCollegeCommand.CommandText = $"SELECT Id FROM Colleges WHERE Code='{course.collegeCode}'";
                collegeId = (int)getCollegeCommand.ExecuteScalar();
                colleges.Add(course.collegeCode, collegeId);
            }

            values.Add(string.Format("('{0}', '{1}', {2}, '{3}', {4})", course.code, course.name, course.durationYears, course.description, collegeId));

        }

        scriptBuilder.Append(string.Join(',', values));

        scriptBuilder.Append("""
            ) AS [Source] ([Code], [Name], [DurationYears], [Description], [CollegeId])
            ON [Target].[Code] = [Source].[Code]
            WHEN NOT MATCHED THEN
                INSERT ([Code], [Name], [DurationYears], [Description], [CollegeId], [CreatedBy], [CreatedAt])
                VALUES ([Source].[Code], [Source].[Name], [Source].[DurationYears], [Source].[Description], [Source].[CollegeId], @InitialUserId, GETUTCDATE())
            WHEN MATCHED THEN
                UPDATE SET [Target].[Name] = [Source].[Name],
                           [Target].[DurationYears] = [Source].[DurationYears],
                           [Target].[Description] = [Source].[Description],
                           [Target].[CollegeId] = [Source].[CollegeId],
                           [Target].[UpdatedBy] = @InitialUserId,
                           [Target].[UpdatedAt] = GETUTCDATE();
            """);

        return scriptBuilder.ToString();
    }

    public record Course(CollegeCode collegeCode, CourseCode code, string name, int durationYears, string description);
}
