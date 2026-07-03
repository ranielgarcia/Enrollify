using DbUp.Engine;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Mocks.Dev;

public class MockDev0007__Curriculum_BSBA : IScript
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
            new("BA-PMGT", 1, 1, []),
            new("BA-ECON1", 1, 1, []),

            // Year 1, Semester 2
            new("GE-FIL1", 1, 2, []),
            new("GE-PE2", 1, 2, ["GE-PE1"]),
            new("BA-ACC1", 1, 2, []),
            new("BA-MKTG", 1, 2, []),
            new("BA-BUSCOM", 1, 2, []),

            // Year 1, Semester 3 (Summer)
            new("GE-RIZAL", 1, 3, []),
            new("BA-ECON2", 1, 3, ["BA-ECON1"]),

            // ============================================
            // YEAR 2
            // ============================================

            // Year 2, Semester 1
            new("GE-SCI1", 2, 1, []),
            new("BA-ACC2", 2, 1, ["BA-ACC1"]),
            new("BA-FIN1", 2, 1, ["BA-ACC1"]),
            new("BA-HRM", 2, 1, ["BA-PMGT"]),
            new("BA-STAT", 2, 1, ["GE-MATH1"]),

            // Year 2, Semester 2
            new("GE-ETHICS", 2, 2, []),
            new("BA-OPMGT", 2, 2, ["BA-PMGT"]),
            new("BA-SCM", 2, 2, ["BA-OPMGT"]),
            new("BA-BUSLAW", 2, 2, []),
            new("BA-BUSETH", 2, 2, []),

            // Year 2, Semester 3 (Summer)
            new("BA-ENTREP", 2, 3, ["BA-PMGT", "BA-MKTG"]),
            new("BA-ORGDEV", 2, 3, ["BA-HRM"]),

            // ============================================
            // YEAR 3
            // ============================================

            // Year 3, Semester 1
            new("BA-FIN2", 3, 1, ["BA-FIN1"]),
            new("BA-INTLBU", 3, 1, ["BA-ECON2"]),
            new("BA-TQM", 3, 1, ["BA-OPMGT"]),
            new("BA-FEASIB", 3, 1, ["BA-ACC2", "BA-FIN1"]),
            new("BA-GOVCOR", 3, 1, ["BA-BUSETH"]),

            // Year 3, Semester 2
            new("BA-SMGT", 3, 2, ["BA-ORGDEV", "BA-FIN1"]),
            new("BA-BUSRES", 3, 2, ["BA-STAT"]),
            new("BA-PRAC1", 3, 2, []),
            new("MATH-PROB", 3, 2, ["GE-MATH1"]),

            // Year 3, Semester 3 (Summer)
            new("PROF-ENTREP", 3, 3, []),
            new("BA-PRAC2", 3, 3, ["BA-PRAC1"]),

            // ============================================
            // YEAR 4
            // ============================================

            // Year 4, Semester 1
            new("CAP-THESIS1", 4, 1, ["BA-BUSRES"]),
            new("PROF-OJT", 4, 1, []),

            // Year 4, Semester 2
            new("CAP-THESIS2", 4, 2, ["CAP-THESIS1"]),
        };

        var scriptBuilder = new StringBuilder();

        // Get initial user and course IDs
        scriptBuilder.AppendLine("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            DECLARE @BSBACourseId INT = (SELECT Id FROM Courses WHERE Code='BSBA');
            DECLARE @ActiveStatusId INT = 2; -- Active status for curriculum
            """);

        // Create Curriculum for BSBA
        scriptBuilder.AppendLine("""

            -- ============================================
            -- CREATE CURRICULUM
            -- ============================================

            MERGE [Curriculums] AS [Target]
            USING (VALUES
                (@BSBACourseId, 2024, '2024-A', @ActiveStatusId, 'Bachelor of Science in Business Administration Curriculum 2024')
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

            DECLARE @BSBACurriculumId INT = (SELECT Id FROM Curriculums WHERE CourseId = @BSBACourseId AND Version = '2024-A');
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
            subjectValues.Add($"('BSBA', '2024-A', '{entry.SubjectCode}', {entry.YearLevel}, {entry.TermNumber}, 0, NULL)");
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
            WHERE c.Id = @BSBACurriculumId
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
                prereqValues.Add($"('BSBA', '2024-A', '{subject}', '{prerequisite}')");
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
