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
            new Subject(SubjectCode.From("GE-ETHICS"), "Ethics", 3.0m, "Study of moral principles and ethical decision-making.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-RIZAL"), "Life and Works of Rizal", 3.0m, "Study of the life, works, and writings of Jose Rizal.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-PE1"), "Physical Education 1", 2.0m, "Foundation of physical fitness and wellness.", "Gymnasium"),
            new Subject(SubjectCode.From("GE-PE2"), "Physical Education 2", 2.0m, "Team sports and recreational activities.", "Gymnasium"),

            // Mathematics and Science Subjects
            new Subject(SubjectCode.From("MATH-CALC1"), "Calculus 1", 3.0m, "Differential calculus and its applications.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-CALC2"), "Calculus 2", 3.0m, "Integral calculus and its applications.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-DISCR"), "Discrete Mathematics", 3.0m, "Mathematical structures for computer science.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-LINALG"), "Linear Algebra", 3.0m, "Study of vectors, matrices, and linear transformations.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-PROB"), "Probability and Statistics", 3.0m, "Statistical methods and probability theory.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-NUMER"), "Numerical Methods", 3.0m, "Computational methods for solving mathematical problems.", "Computer Lab"),

            // Core IT/CS Subjects
            new Subject(SubjectCode.From("CC-PROG1"), "Introduction to Programming", 3.0m, "Fundamentals of programming using a high-level language.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-PROG2"), "Intermediate Programming", 3.0m, "Advanced programming concepts and data structures.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-PROG3"), "Advanced Programming", 3.0m, "Complex programming paradigms and design patterns.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DSTRUC"), "Data Structures and Algorithms", 3.0m, "Study of fundamental data structures and algorithm design.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-OOP"), "Object-Oriented Programming", 3.0m, "Principles and practices of object-oriented programming.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DBMS"), "Database Management Systems", 3.0m, "Design, implementation, and management of database systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-NETW1"), "Fundamentals of Networking", 3.0m, "Introduction to computer networks and data communications.", "Laboratory"),
            new Subject(SubjectCode.From("CC-NETW2"), "Advanced Networking", 3.0m, "Advanced network protocols and configurations.", "Laboratory"),
            new Subject(SubjectCode.From("CC-OS"), "Operating Systems", 3.0m, "Concepts and principles of operating systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-WEBDEV"), "Web Development", 3.0m, "Design and development of web-based applications.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DIGLOG"), "Digital Logic Design", 3.0m, "Fundamentals of digital circuits and logic design.", "Laboratory"),
            new Subject(SubjectCode.From("CC-COMORG"), "Computer Organization and Architecture", 3.0m, "Study of computer hardware organization and architecture.", "Lecture Room"),
            new Subject(SubjectCode.From("CC-HCI"), "Human-Computer Interaction", 3.0m, "Principles of designing user-friendly interfaces.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-TECHWR"), "Technical Writing", 3.0m, "Writing technical documents and reports.", "Lecture Room"),

            // IT-Specific Subjects
            new Subject(SubjectCode.From("IT-SYSAD"), "Systems Administration", 3.0m, "Administration and management of IT systems.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-INFMGT"), "Information Management", 3.0m, "Management and organization of information resources.", "Lecture Room"),
            new Subject(SubjectCode.From("IT-NETSEC"), "Network Security", 3.0m, "Principles and practices of securing computer networks.", "Laboratory"),
            new Subject(SubjectCode.From("IT-MOBDEV"), "Mobile Application Development", 3.0m, "Development of applications for mobile platforms.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-CLOUD"), "Cloud Computing", 3.0m, "Fundamentals of cloud infrastructure and services.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-DEVOPS"), "DevOps Practices", 3.0m, "Continuous integration, delivery, and deployment practices.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-SYSINT"), "Systems Integration and Architecture", 3.0m, "Enterprise systems integration patterns.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-ITPROJ"), "IT Project Management", 3.0m, "Project management methodologies for IT projects.", "Lecture Room"),
            new Subject(SubjectCode.From("IT-BUSANA"), "Business Analytics", 3.0m, "Data-driven decision making for business.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-ECOMM"), "E-Commerce Technologies", 3.0m, "Technologies and platforms for electronic commerce.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-MULMED"), "Multimedia Systems", 3.0m, "Design and development of multimedia applications.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-QUALTY"), "IT Service Quality Management", 3.0m, "Quality assurance in IT service delivery.", "Lecture Room"),

            // CS-Specific Subjects
            new Subject(SubjectCode.From("CS-ALGO"), "Algorithm Design and Analysis", 3.0m, "Advanced study of algorithm design techniques and analysis.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-AUTOMATA"), "Automata Theory and Formal Languages", 3.0m, "Study of abstract machines and formal languages.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-AI"), "Artificial Intelligence", 3.0m, "Fundamentals of artificial intelligence and machine learning.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-SOFTENG"), "Software Engineering", 3.0m, "Principles and methodologies of software development.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-COMPIL"), "Compiler Design", 3.0m, "Design and implementation of programming language compilers.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-ML"), "Machine Learning", 3.0m, "Statistical learning algorithms and applications.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-DATSCI"), "Data Science", 3.0m, "Data analysis, visualization, and predictive modeling.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-DISTSY"), "Distributed Systems", 3.0m, "Design and implementation of distributed computing systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-PARSYS"), "Parallel Computing", 3.0m, "Parallel algorithms and programming techniques.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-COMPGR"), "Computer Graphics", 3.0m, "Fundamentals of 2D and 3D computer graphics.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-CYBERS"), "Cybersecurity Fundamentals", 3.0m, "Security principles, threats, and countermeasures.", "Laboratory"),
            new Subject(SubjectCode.From("CS-GAMEDEV"), "Game Development", 3.0m, "Design and development of computer games.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-NLPROC"), "Natural Language Processing", 3.0m, "Computational techniques for processing human language.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-IMGPRC"), "Image Processing", 3.0m, "Digital image processing techniques and applications.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-BIGDAT"), "Big Data Analytics", 3.0m, "Processing and analyzing large-scale datasets.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-IOTSYS"), "Internet of Things", 3.0m, "Design and implementation of IoT systems.", "Laboratory"),
            new Subject(SubjectCode.From("CS-BLKCHN"), "Blockchain Technology", 3.0m, "Fundamentals of blockchain and decentralized systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-QUANT"), "Quantum Computing", 3.0m, "Introduction to quantum computing concepts.", "Lecture Room"),

            // Professional and Elective Subjects
            new Subject(SubjectCode.From("PROF-ETHICS"), "Professional Ethics in Computing", 3.0m, "Ethical issues and responsibilities in computing profession.", "Lecture Room"),
            new Subject(SubjectCode.From("PROF-LAW"), "IT Laws and Policies", 3.0m, "Legal frameworks governing information technology.", "Lecture Room"),
            new Subject(SubjectCode.From("PROF-ENTREP"), "Technopreneurship", 3.0m, "Entrepreneurship in technology-based ventures.", "Lecture Room"),
            new Subject(SubjectCode.From("PROF-OJT"), "On-the-Job Training", 6.0m, "Industry immersion and practical training.", "Seminar Room"),

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
