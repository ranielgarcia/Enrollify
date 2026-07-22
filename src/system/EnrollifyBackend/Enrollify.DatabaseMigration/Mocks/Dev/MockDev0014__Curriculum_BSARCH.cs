using DbUp.Engine;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Mocks.Dev;

public class MockDev0014__Curriculum_BSARCH : IScript
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
            new("GE-PE1", 1, 1, []),
            new("ARCH-DES1", 1, 1, []),
            new("ARCH-VIS1", 1, 1, []),

            // Year 1, Semester 2
            new("GE-FIL1", 1, 2, []),
            new("GE-PE2", 1, 2, ["GE-PE1"]),
            new("ARCH-DES2", 1, 2, ["ARCH-DES1"]),
            new("ARCH-VIS2", 1, 2, ["ARCH-VIS1"]),
            new("ARCH-HIST1", 1, 2, []),

            // Year 1, Semester 3 (Summer)
            new("GE-RIZAL", 1, 3, []),
            new("ARCH-MATL", 1, 3, []),

            // ============================================
            // YEAR 2
            // ============================================

            // Year 2, Semester 1
            new("GE-SCI1", 2, 1, []),
            new("ARCH-DES3", 2, 1, ["ARCH-DES2"]),
            new("ARCH-VIS3", 2, 1, ["ARCH-VIS2"]),
            new("ARCH-HIST2", 2, 1, ["ARCH-HIST1"]),
            new("ARCH-STRUC1", 2, 1, []),

            // Year 2, Semester 2
            new("GE-ETHICS", 2, 2, []),
            new("ARCH-DES4", 2, 2, ["ARCH-DES3"]),
            new("ARCH-HIST3", 2, 2, ["ARCH-HIST2"]),
            new("ARCH-STRUC2", 2, 2, ["ARCH-STRUC1"]),
            new("ARCH-THEO", 2, 2, []),

            // Year 2, Semester 3 (Summer)
            new("ARCH-CAD1", 2, 3, ["ARCH-DES3"]),
            new("ARCH-UTIL1", 2, 3, []),

            // ============================================
            // YEAR 3
            // ============================================

            // Year 3, Semester 1
            new("MATH-PROB", 3, 1, ["GE-MATH1"]),
            new("ARCH-DES5", 3, 1, ["ARCH-DES4"]),
            new("ARCH-STRUC3", 3, 1, ["ARCH-STRUC2"]),
            new("ARCH-UTIL2", 3, 1, ["ARCH-UTIL1"]),

            // Year 3, Semester 2
            new("ARCH-DES6", 3, 2, ["ARCH-DES5"]),
            new("ARCH-UTIL3", 3, 2, ["ARCH-UTIL2"]),
            new("ARCH-PLAN", 3, 2, []),
            new("ARCH-CAD2", 3, 2, ["ARCH-CAD1"]),

            // Year 3, Semester 3 (Summer)
            new("PROF-ENTREP", 3, 3, []),
            new("ARCH-SUST", 3, 3, ["ARCH-DES5"]),

            // ============================================
            // YEAR 4
            // ============================================

            // Year 4, Semester 1
            new("ARCH-DES7", 4, 1, ["ARCH-DES6"]),
            new("ARCH-CONS", 4, 1, ["ARCH-STRUC3"]),
            new("ARCH-EST", 4, 1, []),
            new("ARCH-RES", 4, 1, ["MATH-PROB"]),

            // Year 4, Semester 2
            new("ARCH-DES8", 4, 2, ["ARCH-DES7"]),
            new("ARCH-BLDGC", 4, 2, []),
            new("PROF-OJT", 4, 2, []),

            // Year 4, Semester 3 (Summer)
            new("CAP-THESIS1", 4, 3, ["ARCH-RES"]),

            // ============================================
            // YEAR 5
            // ============================================

            // Year 5, Semester 1
            new("ARCH-THES", 5, 1, ["ARCH-DES8", "CAP-THESIS1"]),

            // Year 5, Semester 2
            new("CAP-THESIS2", 5, 2, ["CAP-THESIS1"]),
        };

        var scriptBuilder = new StringBuilder();

        // Get initial user and course IDs
        scriptBuilder.AppendLine("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            DECLARE @BSARCHCourseId INT = (SELECT Id FROM Courses WHERE Code='BSARCH');
            DECLARE @DraftStatusId INT = 1; -- Draft status for curriculum
            """);

        // Create Curriculum for BSARCH
        scriptBuilder.AppendLine("""

            -- ============================================
            -- CREATE CURRICULUM
            -- ============================================

            MERGE [Curriculums] AS [Target]
            USING (VALUES
                (@BSARCHCourseId, 2024, '2024-A', @DraftStatusId, 'Bachelor of Science in Architecture Curriculum 2024')
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

            DECLARE @BSARCHCurriculumId INT = (SELECT Id FROM Curriculums WHERE CourseId = @BSARCHCourseId AND Version = '2024-A');
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
            subjectValues.Add($"('BSARCH', '2024-A', '{entry.SubjectCode}', {entry.YearLevel}, {entry.TermNumber}, 0, NULL)");
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
            WHERE c.Id = @BSARCHCurriculumId
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
                prereqValues.Add($"('BSARCH', '2024-A', '{subject}', '{prerequisite}')");
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
