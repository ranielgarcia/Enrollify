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
            new Course(CollegeCode.From("CCS"), CourseCode.From("BSCS"), "Bachelor of Science in Computer Science", 4, "Bachelor of Science in Computer Science (BSCS) is a four-year program that includes the study of computing concepts and theories, algorithmic foundations, and new developments in computing."),
            new Course(CollegeCode.From("CBA"), CourseCode.From("BSBA"), "Bachelor of Science in Business Administration", 4, "The Bachelor of Science in Business Administration (BSBA) program develops students with a strong foundation in management, finance, marketing, operations, and entrepreneurship, preparing them for leadership roles in various business enterprises."),
            new Course(CollegeCode.From("CBA"), CourseCode.From("BSA"), "Bachelor of Science in Accountancy", 4, "The Bachelor of Science in Accountancy (BSA) program provides students with the technical knowledge and professional skills required for the practice of accountancy, including auditing, taxation, and financial reporting."),
            new Course(CollegeCode.From("CED"), CourseCode.From("BSEd"), "Bachelor of Secondary Education", 4, "The Bachelor of Secondary Education (BSEd) program prepares future educators to teach in secondary schools, with specializations in various subject areas and a strong foundation in pedagogical theories and practices."),
            new Course(CollegeCode.From("CED"), CourseCode.From("BECEd"), "Bachelor of Early Childhood Education", 4, "The Bachelor of Early Childhood Education (BECEd) program equips students with the knowledge and skills necessary to educate and nurture children from birth to eight years of age."),
            new Course(CollegeCode.From("CAS"), CourseCode.From("ABELS"), "Bachelor of Arts in English Language Studies", 4, "The Bachelor of Arts in English Language Studies (ABELS) program focuses on the study of the English language, its structure, literature, and its role in global communication and culture."),
            new Course(CollegeCode.From("CAS"), CourseCode.From("BSP"), "Bachelor of Science in Psychology", 4, "The Bachelor of Science in Psychology (BSP) program provides a scientific understanding of human behavior, mental processes, and the application of psychological principles in various settings."),
            new Course(CollegeCode.From("CN"), CourseCode.From("BSN"), "Bachelor of Science in Nursing", 4, "The Bachelor of Science in Nursing (BSN) program prepares students to become professional nurses who provide safe, competent, and compassionate care to individuals, families, and communities."),
            new Course(CollegeCode.From("CA"), CourseCode.From("BSARCH"), "Bachelor of Science in Architecture", 5, "The Bachelor of Science in Architecture (BSARCH) program provides a comprehensive education in architectural design, building technology, urban planning, and sustainable construction practices.")
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
