using DbUp.Engine;
using Enrollify.Core.Aggregates.SubjectAggregate;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Mocks.Dev;

public class MockDev0005__Subjects : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var subjects = new List<Subject>()
        {
            // General Education Subjects
            new Subject(SubjectCode.From("GE-MATH1"), "Mathematics in the Modern World", 3.0m, "Study of mathematics as a tool for understanding the world.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-ENG1"), "Purposive Communication", 3.0m, "Development of communication skills for various purposes.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-FIL1"), "Kontekswalisadong Komunikasyon sa Filipino", 3.0m, "Filipino communication in various contexts.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-SCI1"), "Science, Technology, and Society", 3.0m, "Study of the interaction between science, technology, and society.", "Lecture Room"),

            // Core IT/CS Subjects
            new Subject(SubjectCode.From("CC-PROG1"), "Introduction to Programming", 3.0m, "Fundamentals of programming using a high-level language.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-PROG2"), "Intermediate Programming", 3.0m, "Advanced programming concepts and data structures.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DSTRUC"), "Data Structures and Algorithms", 3.0m, "Study of fundamental data structures and algorithm design.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-OOP"), "Object-Oriented Programming", 3.0m, "Principles and practices of object-oriented programming.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DBMS"), "Database Management Systems", 3.0m, "Design, implementation, and management of database systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-NETW1"), "Fundamentals of Networking", 3.0m, "Introduction to computer networks and data communications.", "Laboratory"),
            new Subject(SubjectCode.From("CC-OS"), "Operating Systems", 3.0m, "Concepts and principles of operating systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-WEBDEV"), "Web Development", 3.0m, "Design and development of web-based applications.", "Computer Lab"),

            // IT-Specific Subjects
            new Subject(SubjectCode.From("IT-SYSAD"), "Systems Administration", 3.0m, "Administration and management of IT systems.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-INFMGT"), "Information Management", 3.0m, "Management and organization of information resources.", "Lecture Room"),
            new Subject(SubjectCode.From("IT-NETSEC"), "Network Security", 3.0m, "Principles and practices of securing computer networks.", "Laboratory"),
            new Subject(SubjectCode.From("IT-MOBDEV"), "Mobile Application Development", 3.0m, "Development of applications for mobile platforms.", "Computer Lab"),

            // CS-Specific Subjects
            new Subject(SubjectCode.From("CS-ALGO"), "Algorithm Design and Analysis", 3.0m, "Advanced study of algorithm design techniques and analysis.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-AUTOMATA"), "Automata Theory and Formal Languages", 3.0m, "Study of abstract machines and formal languages.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-AI"), "Artificial Intelligence", 3.0m, "Fundamentals of artificial intelligence and machine learning.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-SOFTENG"), "Software Engineering", 3.0m, "Principles and methodologies of software development.", "Lecture Room"),

            // Capstone/Thesis
            new Subject(SubjectCode.From("CAP-THESIS1"), "Capstone Project 1", 3.0m, "First phase of capstone project development.", "Seminar Room"),
            new Subject(SubjectCode.From("CAP-THESIS2"), "Capstone Project 2", 3.0m, "Second phase of capstone project development.", "Seminar Room"),
        };

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            """);

        Dictionary<string, int> roomTypes = [];

        scriptBuilder.Append("""
                MERGE [Subjects] AS [Target]
                USING ( VALUES
            """);

        var values = new List<string>();
        foreach (var subject in subjects)
        {
            if (!roomTypes.TryGetValue(subject.preferRoomTypeName, out int roomTypeId))
            {
                var getRoomTypeCommand = dbCommandFactory();
                getRoomTypeCommand.CommandText = $"SELECT Id FROM RoomTypes WHERE Name='{subject.preferRoomTypeName}'";
                roomTypeId = (int)getRoomTypeCommand.ExecuteScalar();
                roomTypes.Add(subject.preferRoomTypeName, roomTypeId);
            }

            var descriptionValue = subject.description != null ? $"'{subject.description.Replace("'", "''")}'" : "NULL";
            var unitsValue = subject.units.HasValue ? subject.units.Value.ToString("0.0") : "NULL";

            values.Add(string.Format("('{0}', '{1}', {2}, {3}, {4})",
                subject.code,
                subject.title.Replace("'", "''"),
                unitsValue,
                descriptionValue,
                roomTypeId));
        }

        scriptBuilder.Append(string.Join(',', values));

        scriptBuilder.Append("""
            ) AS [Source] ([Code], [Title], [Units], [Description], [PreferRoomTypeId])
            ON [Target].[Code] = [Source].[Code]
            WHEN NOT MATCHED THEN
                INSERT ([Code], [Title], [Units], [Description], [PreferRoomTypeId], [CreatedBy], [CreatedAt])
                VALUES ([Source].[Code], [Source].[Title], [Source].[Units], [Source].[Description], [Source].[PreferRoomTypeId], @InitialUserId, GETUTCDATE())
            WHEN MATCHED THEN
                UPDATE SET [Target].[Title] = [Source].[Title],
                           [Target].[Units] = [Source].[Units],
                           [Target].[Description] = [Source].[Description],
                           [Target].[PreferRoomTypeId] = [Source].[PreferRoomTypeId],
                           [Target].[UpdatedBy] = @InitialUserId,
                           [Target].[UpdatedAt] = GETUTCDATE();
            """);

        return scriptBuilder.ToString();
    }

    public record Subject(SubjectCode code, string title, decimal? units, string? description, string preferRoomTypeName);
}
