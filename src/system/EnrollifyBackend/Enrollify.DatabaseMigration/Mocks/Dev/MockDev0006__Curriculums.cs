using DbUp.Engine;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Mocks.Dev;

public class MockDev0006__Curriculums : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        // Define curriculum subjects with year, semester, and prerequisites
        var curriculumSubjects = new List<CurriculumSubjectEntry>
        {
            // ============================================
            // YEAR 1
            // ============================================

            // Year 1, Semester 1
            new("GE-MATH1", 1, 1, []),
            new("GE-ENG1", 1, 1, []),
            new("CC-PROG1", 1, 1, []),
            new("GE-PE1", 1, 1, []),
            new("CC-DIGLOG", 1, 1, []),

            // Year 1, Semester 2
            new("GE-FIL1", 1, 2, []),
            new("CC-PROG2", 1, 2, ["CC-PROG1"]),
            new("MATH-DISCR", 1, 2, ["GE-MATH1"]),
            new("GE-PE2", 1, 2, ["GE-PE1"]),
            new("CC-COMORG", 1, 2, ["CC-DIGLOG"]),

            // Year 1, Semester 3 (Summer)
            new("GE-RIZAL", 1, 3, []),
            new("MATH-CALC1", 1, 3, ["GE-MATH1"]),

            // ============================================
            // YEAR 2
            // ============================================

            // Year 2, Semester 1
            new("CC-DSTRUC", 2, 1, ["CC-PROG2", "MATH-DISCR"]),
            new("CC-OOP", 2, 1, ["CC-PROG2"]),
            new("MATH-CALC2", 2, 1, ["MATH-CALC1"]),
            new("GE-SCI1", 2, 1, []),
            new("CC-TECHWR", 2, 1, ["GE-ENG1"]),

            // Year 2, Semester 2
            new("CC-DBMS", 2, 2, ["CC-DSTRUC"]),
            new("CC-NETW1", 2, 2, []),
            new("MATH-PROB", 2, 2, ["MATH-CALC1"]),
            new("GE-ETHICS", 2, 2, []),
            new("CC-OS", 2, 2, ["CC-DSTRUC"]),

            // Year 2, Semester 3 (Summer)
            new("CC-WEBDEV", 2, 3, ["CC-OOP", "CC-DBMS"]),
            new("MATH-LINALG", 2, 3, ["MATH-CALC1"]),

            // ============================================
            // YEAR 3
            // ============================================

            // Year 3, Semester 1
            new("CS-ALGO", 3, 1, ["CC-DSTRUC", "MATH-DISCR"]),
            new("CC-NETW2", 3, 1, ["CC-NETW1"]),
            new("IT-INFMGT", 3, 1, ["CC-DBMS"]),
            new("CC-HCI", 3, 1, ["CC-OOP"]),
            new("CS-SOFTENG", 3, 1, ["CC-OOP", "CC-DBMS"]),

            // Year 3, Semester 2
            new("CS-AUTOMATA", 3, 2, ["CS-ALGO", "MATH-DISCR"]),
            new("IT-NETSEC", 3, 2, ["CC-NETW1"]),
            new("IT-SYSAD", 3, 2, ["CC-OS", "CC-NETW1"]),
            new("MATH-NUMER", 3, 2, ["MATH-CALC2", "CC-PROG2"]),
            new("CS-COMPGR", 3, 2, ["MATH-LINALG", "CC-OOP"]),

            // Year 3, Semester 3 (Summer)
            new("IT-MOBDEV", 3, 3, ["CC-OOP", "CC-DBMS"]),
            new("PROF-ETHICS", 3, 3, []),

            // ============================================
            // YEAR 4
            // ============================================

            // Year 4, Semester 1
            new("CS-AI", 4, 1, ["CS-ALGO", "MATH-PROB"]),
            new("CS-ML", 4, 1, ["CS-ALGO", "MATH-PROB", "MATH-LINALG"]),
            new("IT-CLOUD", 4, 1, ["CC-NETW2", "IT-SYSAD"]),
            new("CAP-THESIS1", 4, 1, ["CS-SOFTENG"]),
            new("PROF-LAW", 4, 1, []),

            // Year 4, Semester 2
            new("CS-DATSCI", 4, 2, ["CS-ML", "MATH-PROB"]),
            new("CS-BIGDAT", 4, 2, ["CC-DBMS", "CS-ALGO"]),
            new("CAP-THESIS2", 4, 2, ["CAP-THESIS1"]),
            new("IT-DEVOPS", 4, 2, ["IT-CLOUD", "CS-SOFTENG"]),

            // Year 4, Semester 3 (Summer)
            new("PROF-OJT", 4, 3, ["CAP-THESIS1"]),
            new("PROF-ENTREP", 4, 3, []),
        };

        var scriptBuilder = new StringBuilder();

        // Get initial user and course IDs
        scriptBuilder.AppendLine("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            DECLARE @BSCSCourseId INT = (SELECT Id FROM Courses WHERE Code='BSCS');
            DECLARE @BSITCourseId INT = (SELECT Id FROM Courses WHERE Code='BSIT');
            DECLARE @DraftStatusId INT = 1; -- Draft status for curriculum
            """);

        // Create Curriculums for BSCS and BSIT
        scriptBuilder.AppendLine("""

            -- ============================================
            -- CREATE CURRICULUMS
            -- ============================================

            MERGE [Curriculums] AS [Target]
            USING (VALUES
                (@BSCSCourseId, 2024, '2024-A', @DraftStatusId, 'Bachelor of Science in Computer Science Curriculum 2024'),
                (@BSITCourseId, 2024, '2024-A', @DraftStatusId, 'Bachelor of Science in Information Technology Curriculum 2024')
            ) AS [Source] ([CourseId], [EffectiveYear], [Version], [StatusId], [Description])
            ON [Target].[CourseId] = [Source].[CourseId] AND [Target].[Version] = [Source].[Version]
            WHEN NOT MATCHED THEN
                INSERT ([CourseId], [EffectiveYear], [Version], [StatusId], [Description], [ApprovedDate], [CreatedBy], [CreatedAt])
                VALUES ([Source].[CourseId], [Source].[EffectiveYear], [Source].[Version], [Source].[StatusId], [Source].[Description], GETUTCDATE(), @InitialUserId, GETUTCDATE())
            WHEN MATCHED THEN
                UPDATE SET
                    [Target].[EffectiveYear] = [Source].[EffectiveYear],
                    [Target].[StatusId] = [Source].[StatusId],
                    [Target].[Description] = [Source].[Description],
                    [Target].[UpdatedBy] = @InitialUserId,
                    [Target].[UpdatedAt] = GETUTCDATE();

            DECLARE @BSCSCurriculumId INT = (SELECT Id FROM Curriculums WHERE CourseId = @BSCSCourseId AND Version = '2024-A');
            DECLARE @BSITCurriculumId INT = (SELECT Id FROM Curriculums WHERE CourseId = @BSITCourseId AND Version = '2024-A');
            """);

        // Build CurriculumSubjects MERGE statement
        scriptBuilder.AppendLine("""

            -- ============================================
            -- CREATE CURRICULUM SUBJECTS
            -- ============================================

            MERGE [CurriculumSubjects] AS [Target]
            USING (
                SELECT
                    c.Id AS CurriculumId,
                    s.Id AS SubjectId,
                    src.YearLevel,
                    src.TermNumber,
                    src.IsElective,
                    src.ElectiveGroupName
                FROM (VALUES
            """);

        var subjectValues = new List<string>();
        foreach (var entry in curriculumSubjects)
        {
            // Add for both BSCS and BSIT curriculums
            subjectValues.Add($"('BSCS', '2024-A', '{entry.SubjectCode}', {entry.YearLevel}, {entry.TermNumber}, 0, NULL)");
            subjectValues.Add($"('BSIT', '2024-A', '{entry.SubjectCode}', {entry.YearLevel}, {entry.TermNumber}, 0, NULL)");
        }

        scriptBuilder.AppendLine(string.Join(",\n            ", subjectValues));

        scriptBuilder.AppendLine("""
                ) AS src (CourseCode, CurriculumVersion, SubjectCode, YearLevel, TermNumber, IsElective, ElectiveGroupName)
                INNER JOIN Courses co ON co.Code = src.CourseCode
                INNER JOIN Curriculums c ON c.CourseId = co.Id AND c.Version = src.CurriculumVersion
                INNER JOIN Subjects s ON s.Code = src.SubjectCode
            ) AS [Source] ([CurriculumId], [SubjectId], [YearLevel], [TermNumber], [IsElective], [ElectiveGroupName])
            ON [Target].[CurriculumId] = [Source].[CurriculumId] AND [Target].[SubjectId] = [Source].[SubjectId]
            WHEN NOT MATCHED THEN
                INSERT ([CurriculumId], [SubjectId], [YearLevel], [TermNumber], [IsElective], [ElectiveGroupName], [CreatedBy], [CreatedAt])
                VALUES ([Source].[CurriculumId], [Source].[SubjectId], [Source].[YearLevel], [Source].[TermNumber], [Source].[IsElective], [Source].[ElectiveGroupName], @InitialUserId, GETUTCDATE())
            WHEN MATCHED THEN
                UPDATE SET
                    [Target].[YearLevel] = [Source].[YearLevel],
                    [Target].[TermNumber] = [Source].[TermNumber],
                    [Target].[IsElective] = [Source].[IsElective],
                    [Target].[ElectiveGroupName] = [Source].[ElectiveGroupName],
                    [Target].[UpdatedBy] = @InitialUserId,
                    [Target].[UpdatedAt] = GETUTCDATE();
            """);

        // Build CurriculumSubjectPrerequisites statements
        var prerequisiteEntries = curriculumSubjects
            .Where(e => e.PrerequisiteCodes.Length > 0)
            .SelectMany(e => e.PrerequisiteCodes.Select(prereq => (Subject: e.SubjectCode, Prerequisite: prereq)))
            .ToList();

        if (prerequisiteEntries.Count > 0)
        {
            scriptBuilder.AppendLine("""

            -- ============================================
            -- CREATE CURRICULUM SUBJECT PREREQUISITES
            -- ============================================

            -- First, soft-delete existing prerequisites that are no longer valid
            UPDATE csp
            SET csp.IsActive = 0,
                csp.DeletedBy = @InitialUserId,
                csp.DeletedAt = GETUTCDATE()
            FROM CurriculumSubjectPrerequisites csp
            INNER JOIN CurriculumSubjects cs ON cs.Id = csp.CurriculumSubjectId
            INNER JOIN Curriculums c ON c.Id = cs.CurriculumId
            WHERE c.Id IN (@BSCSCurriculumId, @BSITCurriculumId)
              AND csp.IsActive = 1;

            -- Insert prerequisites using a CTE to get CurriculumSubject IDs
            ;WITH PrerequisiteData AS (
                SELECT
                    cs_subject.Id AS CurriculumSubjectId,
                    cs_prereq.Id AS PrerequisiteCurriculumSubjectId,
                    c.Id AS CurriculumId
                FROM (VALUES
            """);

            var prereqValues = new List<string>();
            foreach (var (subject, prerequisite) in prerequisiteEntries)
            {
                // Add for both BSCS and BSIT curriculums
                prereqValues.Add($"('BSCS', '2024-A', '{subject}', '{prerequisite}')");
                prereqValues.Add($"('BSIT', '2024-A', '{subject}', '{prerequisite}')");
            }

            scriptBuilder.AppendLine(string.Join(",\n            ", prereqValues));

            scriptBuilder.AppendLine("""
                ) AS src (CourseCode, CurriculumVersion, SubjectCode, PrerequisiteCode)
                INNER JOIN Courses co ON co.Code = src.CourseCode
                INNER JOIN Curriculums c ON c.CourseId = co.Id AND c.Version = src.CurriculumVersion
                INNER JOIN Subjects s_subject ON s_subject.Code = src.SubjectCode
                INNER JOIN Subjects s_prereq ON s_prereq.Code = src.PrerequisiteCode
                INNER JOIN CurriculumSubjects cs_subject ON cs_subject.CurriculumId = c.Id AND cs_subject.SubjectId = s_subject.Id
                INNER JOIN CurriculumSubjects cs_prereq ON cs_prereq.CurriculumId = c.Id AND cs_prereq.SubjectId = s_prereq.Id
            )
            MERGE [CurriculumSubjectPrerequisites] AS [Target]
            USING PrerequisiteData AS [Source]
            ON [Target].[CurriculumSubjectId] = [Source].[CurriculumSubjectId]
               AND [Target].[PrerequisiteCurriculumSubjectId] = [Source].[PrerequisiteCurriculumSubjectId]
            WHEN NOT MATCHED THEN
                INSERT ([CurriculumSubjectId], [PrerequisiteCurriculumSubjectId], [MinimumGrade], [CreatedBy], [CreatedAt], [IsActive])
                VALUES ([Source].[CurriculumSubjectId], [Source].[PrerequisiteCurriculumSubjectId], NULL, @InitialUserId, GETUTCDATE(), 1)
            WHEN MATCHED THEN
                UPDATE SET
                    [Target].[IsActive] = 1,
                    [Target].[DeletedBy] = NULL,
                    [Target].[DeletedAt] = NULL,
                    [Target].[UpdatedBy] = @InitialUserId,
                    [Target].[UpdatedAt] = GETUTCDATE();
            """);
        }

        return scriptBuilder.ToString();
    }

    public record CurriculumSubjectEntry(string SubjectCode, int YearLevel, int TermNumber, string[] PrerequisiteCodes);
}
