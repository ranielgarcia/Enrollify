using Enhance_Genetic_Algorithm_v2.Models;

namespace Enhance_Genetic_Algorithm_v2;

public class ScheduleData
{
    public List<Course> Courses { get; set; }
    public List<Section> Sections { get; set; }
    public List<Subject> Subjects { get; set; }
    public List<Professor> Professors { get; set; }
    public List<Room> Rooms { get; set; }
    public List<TimeSlot> TimeSlots { get; set; }

    public ScheduleData()
    {
        InitializePrograms();
        InitializeProfessors();
        InitializeSubjects();
        InitializeRooms();
        InitializeTimeSlots();
        InitializeSections();
    }

    private void InitializePrograms()
    {
        Courses = new List<Course>
            {
                // College of Computer Studies
                new Course("PROG1", "Bachelor of Science in Computer Science", "BSCS", "CCS"),
                new Course("PROG2", "Bachelor of Science in Information Technology", "BSIT", "CCS"),
                new Course("PROG3", "Bachelor of Science in Information Systems", "BSIS", "CCS"),
                
                // College of Arts and Sciences
                new Course("PROG4", "Bachelor of Science in Mathematics", "BSMath", "CAS"),
                new Course("PROG5", "Bachelor of Science in Physics", "BSPhysics", "CAS"),
                new Course("PROG6", "Bachelor of Science in Biology", "BSBio", "CAS"),
                
                // College of Engineering
                new Course("PROG7", "Bachelor of Science in Civil Engineering", "BSCE", "COE"),
                new Course("PROG8", "Bachelor of Science in Electrical Engineering", "BSEE", "COE"),
                new Course("PROG9", "Bachelor of Science in Mechanical Engineering", "BSME", "COE"),
                
                // College of Business
                new Course("PROG10", "Bachelor of Science in Business Administration", "BSBA", "COB"),
                new Course("PROG11", "Bachelor of Science in Accountancy", "BSA", "COB")
            };
    }

    private void InitializeProfessors()
    {
        Professors = new List<Professor>
            {
                // Computer Science Faculty
                new Professor("P1", "Dr. Maria Santos", "CCS"),
                new Professor("P2", "Prof. John Reyes", "CCS"),
                new Professor("P3", "Dr. Ana Garcia", "CCS"),
                new Professor("P4", "Prof. Robert Cruz", "CCS"),
                new Professor("P5", "Dr. Sofia Mendoza", "CCS"),
                new Professor("P6", "Prof. Michael Tan", "CCS"),
                new Professor("P7", "Dr. Patricia Ramos", "CCS"),
                new Professor("P8", "Prof. Daniel Flores", "CCS"),
                
                // Mathematics Faculty
                new Professor("P9", "Dr. Elizabeth Torres", "CAS"),
                new Professor("P10", "Prof. James Rivera", "CAS"),
                new Professor("P11", "Dr. Carmen Lopez", "CAS"),
                new Professor("P12", "Prof. David Gonzales", "CAS"),
                
                // Physics Faculty
                new Professor("P13", "Dr. Rachel Sanchez", "CAS"),
                new Professor("P14", "Prof. Steven Perez", "CAS"),
                new Professor("P15", "Dr. Linda Martinez", "CAS"),
                
                // Biology Faculty
                new Professor("P16", "Dr. Margaret Castillo", "CAS"),
                new Professor("P17", "Prof. Thomas Morales", "CAS"),
                new Professor("P18", "Dr. Jennifer Herrera", "CAS"),
                
                // Engineering Faculty
                new Professor("P19", "Engr. Carlos Diaz", "COE"),
                new Professor("P20", "Engr. Angela Ruiz", "COE"),
                new Professor("P21", "Dr. Francisco Valdez", "COE"),
                new Professor("P22", "Engr. Sarah Navarro", "COE"),
                new Professor("P23", "Dr. Joseph Cruz", "COE"),
                new Professor("P24", "Engr. Emily Jimenez", "COE"),
                
                // Business Faculty
                new Professor("P25", "Prof. Amanda Salazar", "COB"),
                new Professor("P26", "Dr. Christopher Ortiz", "COB"),
                new Professor("P27", "Prof. Michelle Gutierrez", "COB"),
                new Professor("P28", "Dr. Andrew Rojas", "COB"),
                new Professor("P29", "Prof. Nicole Medina", "COB"),
                new Professor("P30", "Dr. Kevin Romero", "COB"),
                
                // General Education Faculty
                new Professor("P31", "Prof. Sandra Aguilar", "GEN"),
                new Professor("P32", "Dr. William Fernandez", "GEN"),
                new Professor("P33", "Prof. Laura Vargas", "GEN"),
                new Professor("P34", "Dr. Richard Soto", "GEN"),
                new Professor("P35", "Prof. Diana Castro", "GEN")
            };
    }

    private void InitializeSubjects()
    {
        Subjects = new List<Subject>();

        // ========== COMPUTER SCIENCE SUBJECTS ==========
        // Programming subjects (3 days/week, 1.5 hours/day = 4.5 hours total)
        Subjects.Add(new Subject("CS101", "CS101", "Introduction to Programming", SubjectType.ComputerScience,
            3, 3, 1.5, DayPattern.MWF)
        { RequiresLab = true });
        Subjects.Add(new Subject("CS102", "CS102", "Object-Oriented Programming", SubjectType.ComputerScience,
            3, 3, 1.5, DayPattern.MWF)
        { RequiresLab = true });
        Subjects.Add(new Subject("CS201", "CS201", "Data Structures and Algorithms", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("CS202", "CS202", "Database Management Systems", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });
        Subjects.Add(new Subject("CS301", "CS301", "Software Engineering", SubjectType.ComputerScience,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("CS302", "CS302", "Computer Networks", SubjectType.ComputerScience,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("CS401", "CS401", "Artificial Intelligence", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("CS402", "CS402", "Machine Learning", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });

        // IT Subjects
        Subjects.Add(new Subject("IT101", "IT101", "Fundamentals of IT", SubjectType.ComputerScience,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("IT201", "IT201", "Web Development", SubjectType.ComputerScience,
            3, 3, 1.5, DayPattern.MWF)
        { RequiresLab = true });
        Subjects.Add(new Subject("IT301", "IT301", "System Administration", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("IT401", "IT401", "Cybersecurity", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });

        // IS Subjects
        Subjects.Add(new Subject("IS101", "IS101", "Information Systems Fundamentals", SubjectType.ComputerScience,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("IS201", "IS201", "Systems Analysis and Design", SubjectType.ComputerScience,
            3, 2, 2, DayPattern.MW));
        Subjects.Add(new Subject("IS301", "IS301", "Enterprise Systems", SubjectType.ComputerScience,
            3, 2, 1.5, DayPattern.TTh));

        // ========== MATHEMATICS SUBJECTS ==========
        Subjects.Add(new Subject("MATH101", "MATH101", "Calculus I", SubjectType.Mathematics,
            3, 3, 1, DayPattern.MWF));
        Subjects.Add(new Subject("MATH102", "MATH102", "Calculus II", SubjectType.Mathematics,
            3, 3, 1, DayPattern.MWF));
        Subjects.Add(new Subject("MATH201", "MATH201", "Linear Algebra", SubjectType.Mathematics,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("MATH202", "MATH202", "Discrete Mathematics", SubjectType.Mathematics,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("MATH301", "MATH301", "Differential Equations", SubjectType.Mathematics,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("MATH302", "MATH302", "Probability and Statistics", SubjectType.Mathematics,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("MATH401", "MATH401", "Abstract Algebra", SubjectType.Mathematics,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("MATH402", "MATH402", "Real Analysis", SubjectType.Mathematics,
            3, 2, 1.5, DayPattern.MW));

        // ========== PHYSICS SUBJECTS ==========
        Subjects.Add(new Subject("PHY101", "PHY101", "Physics I (Mechanics)", SubjectType.Physics,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });
        Subjects.Add(new Subject("PHY102", "PHY102", "Physics II (Electricity & Magnetism)", SubjectType.Physics,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("PHY201", "PHY201", "Modern Physics", SubjectType.Physics,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("PHY202", "PHY202", "Thermodynamics", SubjectType.Physics,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("PHY301", "PHY301", "Quantum Mechanics", SubjectType.Physics,
            3, 2, 2, DayPattern.MW));
        Subjects.Add(new Subject("PHY302", "PHY302", "Optics", SubjectType.Physics,
            3, 2, 1.5, DayPattern.TTh)
        { RequiresLab = true });

        // ========== BIOLOGY SUBJECTS ==========
        Subjects.Add(new Subject("BIO101", "BIO101", "General Biology I", SubjectType.Biology,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });
        Subjects.Add(new Subject("BIO102", "BIO102", "General Biology II", SubjectType.Biology,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("BIO201", "BIO201", "Genetics", SubjectType.Biology,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });
        Subjects.Add(new Subject("BIO202", "BIO202", "Microbiology", SubjectType.Biology,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("BIO301", "BIO301", "Ecology", SubjectType.Biology,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("BIO302", "BIO302", "Cell Biology", SubjectType.Biology,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });

        // ========== ENGINEERING SUBJECTS ==========
        // Civil Engineering
        Subjects.Add(new Subject("CE101", "CE101", "Engineering Drawing", SubjectType.Engineering,
            3, 2, 2, DayPattern.MW));
        Subjects.Add(new Subject("CE201", "CE201", "Statics of Rigid Bodies", SubjectType.Engineering,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("CE301", "CE301", "Structural Analysis", SubjectType.Engineering,
            3, 2, 2, DayPattern.MW));
        Subjects.Add(new Subject("CE401", "CE401", "Highway Engineering", SubjectType.Engineering,
            3, 2, 1.5, DayPattern.TTh));

        // Electrical Engineering
        Subjects.Add(new Subject("EE101", "EE101", "Circuit Theory", SubjectType.Engineering,
            3, 2, 2, DayPattern.MW)
        { RequiresLab = true });
        Subjects.Add(new Subject("EE201", "EE201", "Electronics", SubjectType.Engineering,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });
        Subjects.Add(new Subject("EE301", "EE301", "Power Systems", SubjectType.Engineering,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("EE401", "EE401", "Control Systems", SubjectType.Engineering,
            3, 2, 2, DayPattern.TTh)
        { RequiresLab = true });

        // Mechanical Engineering
        Subjects.Add(new Subject("ME101", "ME101", "Engineering Mechanics", SubjectType.Engineering,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("ME201", "ME201", "Thermodynamics", SubjectType.Engineering,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("ME301", "ME301", "Fluid Mechanics", SubjectType.Engineering,
            3, 2, 2, DayPattern.MW));
        Subjects.Add(new Subject("ME401", "ME401", "Machine Design", SubjectType.Engineering,
            3, 2, 2, DayPattern.TTh));

        // ========== BUSINESS SUBJECTS ==========
        Subjects.Add(new Subject("BUS101", "BUS101", "Introduction to Business", SubjectType.Business,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("BUS201", "BUS201", "Principles of Marketing", SubjectType.Business,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("BUS301", "BUS301", "Financial Management", SubjectType.Business,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("BUS401", "BUS401", "Strategic Management", SubjectType.Business,
            3, 2, 1.5, DayPattern.TTh));

        // Accountancy
        Subjects.Add(new Subject("ACC101", "ACC101", "Principles of Accounting I", SubjectType.Business,
            3, 2, 2, DayPattern.MW));
        Subjects.Add(new Subject("ACC102", "ACC102", "Principles of Accounting II", SubjectType.Business,
            3, 2, 2, DayPattern.TTh));
        Subjects.Add(new Subject("ACC201", "ACC201", "Cost Accounting", SubjectType.Business,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("ACC301", "ACC301", "Auditing", SubjectType.Business,
            3, 2, 1.5, DayPattern.TTh));

        // ========== GENERAL EDUCATION SUBJECTS ==========
        Subjects.Add(new Subject("ENG101", "ENG101", "English Communication", SubjectType.English,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("ENG102", "ENG102", "Technical Writing", SubjectType.English,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("FIL101", "FIL101", "Komunikasyon sa Filipino", SubjectType.General,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("HIST101", "HIST101", "Philippine History", SubjectType.SocialScience,
            3, 2, 1.5, DayPattern.TTh));
        Subjects.Add(new Subject("SOC101", "SOC101", "Understanding the Self", SubjectType.SocialScience,
            3, 2, 1.5, DayPattern.MW));
        Subjects.Add(new Subject("PE101", "PE101", "Physical Education 1", SubjectType.General,
            2, 1, 2, DayPattern.Single));
        Subjects.Add(new Subject("NSTP101", "NSTP101", "National Service Training Program 1", SubjectType.General,
            3, 1, 3, DayPattern.Single));

        // Assign professors to subjects (multiple professors per subject)
        AssignProfessorsToSubjects();
    }

    private void AssignProfessorsToSubjects()
    {
        // CS Subjects
        Subjects.First(s => s.Code == "CS101").ProfessorIds.AddRange(new[] { "P1", "P2", "P3" });
        Subjects.First(s => s.Code == "CS102").ProfessorIds.AddRange(new[] { "P1", "P2", "P4" });
        Subjects.First(s => s.Code == "CS201").ProfessorIds.AddRange(new[] { "P3", "P4", "P5" });
        Subjects.First(s => s.Code == "CS202").ProfessorIds.AddRange(new[] { "P2", "P5", "P6" });
        Subjects.First(s => s.Code == "CS301").ProfessorIds.AddRange(new[] { "P4", "P7" });
        Subjects.First(s => s.Code == "CS302").ProfessorIds.AddRange(new[] { "P5", "P6", "P8" });
        Subjects.First(s => s.Code == "CS401").ProfessorIds.AddRange(new[] { "P6", "P7" });
        Subjects.First(s => s.Code == "CS402").ProfessorIds.AddRange(new[] { "P7", "P8" });

        // IT Subjects
        Subjects.First(s => s.Code == "IT101").ProfessorIds.AddRange(new[] { "P1", "P2" });
        Subjects.First(s => s.Code == "IT201").ProfessorIds.AddRange(new[] { "P3", "P4", "P8" });
        Subjects.First(s => s.Code == "IT301").ProfessorIds.AddRange(new[] { "P5", "P6" });
        Subjects.First(s => s.Code == "IT401").ProfessorIds.AddRange(new[] { "P7", "P8" });

        // IS Subjects
        Subjects.First(s => s.Code == "IS101").ProfessorIds.AddRange(new[] { "P2", "P3" });
        Subjects.First(s => s.Code == "IS201").ProfessorIds.AddRange(new[] { "P4", "P5" });
        Subjects.First(s => s.Code == "IS301").ProfessorIds.AddRange(new[] { "P6", "P7" });

        // Math Subjects
        Subjects.First(s => s.Code == "MATH101").ProfessorIds.AddRange(new[] { "P9", "P10", "P11" });
        Subjects.First(s => s.Code == "MATH102").ProfessorIds.AddRange(new[] { "P9", "P10", "P12" });
        Subjects.First(s => s.Code == "MATH201").ProfessorIds.AddRange(new[] { "P10", "P11" });
        Subjects.First(s => s.Code == "MATH202").ProfessorIds.AddRange(new[] { "P11", "P12" });
        Subjects.First(s => s.Code == "MATH301").ProfessorIds.AddRange(new[] { "P9", "P12" });
        Subjects.First(s => s.Code == "MATH302").ProfessorIds.AddRange(new[] { "P10", "P11" });
        Subjects.First(s => s.Code == "MATH401").ProfessorIds.AddRange(new[] { "P11", "P12" });
        Subjects.First(s => s.Code == "MATH402").ProfessorIds.AddRange(new[] { "P9", "P10" });

        // Physics Subjects
        Subjects.First(s => s.Code == "PHY101").ProfessorIds.AddRange(new[] { "P13", "P14" });
        Subjects.First(s => s.Code == "PHY102").ProfessorIds.AddRange(new[] { "P13", "P15" });
        Subjects.First(s => s.Code == "PHY201").ProfessorIds.AddRange(new[] { "P14", "P15" });
        Subjects.First(s => s.Code == "PHY202").ProfessorIds.AddRange(new[] { "P13", "P14" });
        Subjects.First(s => s.Code == "PHY301").ProfessorIds.AddRange(new[] { "P15" });
        Subjects.First(s => s.Code == "PHY302").ProfessorIds.AddRange(new[] { "P14", "P15" });

        // Biology Subjects
        Subjects.First(s => s.Code == "BIO101").ProfessorIds.AddRange(new[] { "P16", "P17" });
        Subjects.First(s => s.Code == "BIO102").ProfessorIds.AddRange(new[] { "P16", "P18" });
        Subjects.First(s => s.Code == "BIO201").ProfessorIds.AddRange(new[] { "P17", "P18" });
        Subjects.First(s => s.Code == "BIO202").ProfessorIds.AddRange(new[] { "P16", "P17" });
        Subjects.First(s => s.Code == "BIO301").ProfessorIds.AddRange(new[] { "P17", "P18" });
        Subjects.First(s => s.Code == "BIO302").ProfessorIds.AddRange(new[] { "P16", "P18" });

        // Civil Engineering
        Subjects.First(s => s.Code == "CE101").ProfessorIds.AddRange(new[] { "P19", "P20" });
        Subjects.First(s => s.Code == "CE201").ProfessorIds.AddRange(new[] { "P19", "P21" });
        Subjects.First(s => s.Code == "CE301").ProfessorIds.AddRange(new[] { "P20", "P21" });
        Subjects.First(s => s.Code == "CE401").ProfessorIds.AddRange(new[] { "P21" });

        // Electrical Engineering
        Subjects.First(s => s.Code == "EE101").ProfessorIds.AddRange(new[] { "P22", "P23" });
        Subjects.First(s => s.Code == "EE201").ProfessorIds.AddRange(new[] { "P22", "P24" });
        Subjects.First(s => s.Code == "EE301").ProfessorIds.AddRange(new[] { "P23", "P24" });
        Subjects.First(s => s.Code == "EE401").ProfessorIds.AddRange(new[] { "P23" });

        // Mechanical Engineering
        Subjects.First(s => s.Code == "ME101").ProfessorIds.AddRange(new[] { "P19", "P24" });
        Subjects.First(s => s.Code == "ME201").ProfessorIds.AddRange(new[] { "P20", "P21" });
        Subjects.First(s => s.Code == "ME301").ProfessorIds.AddRange(new[] { "P22", "P23" });
        Subjects.First(s => s.Code == "ME401").ProfessorIds.AddRange(new[] { "P24" });

        // Business Subjects
        Subjects.First(s => s.Code == "BUS101").ProfessorIds.AddRange(new[] { "P25", "P26", "P27" });
        Subjects.First(s => s.Code == "BUS201").ProfessorIds.AddRange(new[] { "P26", "P27" });
        Subjects.First(s => s.Code == "BUS301").ProfessorIds.AddRange(new[] { "P27", "P28" });
        Subjects.First(s => s.Code == "BUS401").ProfessorIds.AddRange(new[] { "P28", "P29" });

        // Accountancy Subjects
        Subjects.First(s => s.Code == "ACC101").ProfessorIds.AddRange(new[] { "P28", "P29", "P30" });
        Subjects.First(s => s.Code == "ACC102").ProfessorIds.AddRange(new[] { "P29", "P30" });
        Subjects.First(s => s.Code == "ACC201").ProfessorIds.AddRange(new[] { "P28", "P30" });
        Subjects.First(s => s.Code == "ACC301").ProfessorIds.AddRange(new[] { "P30" });

        // General Education
        Subjects.First(s => s.Code == "ENG101").ProfessorIds.AddRange(new[] { "P31", "P32", "P33" });
        Subjects.First(s => s.Code == "ENG102").ProfessorIds.AddRange(new[] { "P31", "P32" });
        Subjects.First(s => s.Code == "FIL101").ProfessorIds.AddRange(new[] { "P33", "P34" });
        Subjects.First(s => s.Code == "HIST101").ProfessorIds.AddRange(new[] { "P34", "P35" });
        Subjects.First(s => s.Code == "SOC101").ProfessorIds.AddRange(new[] { "P33", "P35" });
        Subjects.First(s => s.Code == "PE101").ProfessorIds.AddRange(new[] { "P35" });
        Subjects.First(s => s.Code == "NSTP101").ProfessorIds.AddRange(new[] { "P35" });
    }

    private void InitializeRooms()
    {
        Rooms = new List<Room>
            {
                // ========== COMPUTER LABORATORIES ==========
                new Room("CLAB101", "CCS Lab 1", 40, RoomType.ComputerLab),
                new Room("CLAB102", "CCS Lab 2", 40, RoomType.ComputerLab),
                new Room("CLAB103", "CCS Lab 3", 35, RoomType.ComputerLab),
                new Room("CLAB104", "CCS Lab 4", 35, RoomType.ComputerLab),
                new Room("CLAB105", "CCS Lab 5", 30, RoomType.ComputerLab),
                new Room("CLAB106", "CCS Lab 6", 30, RoomType.ComputerLab),
                new Room("CLAB201", "IT Lab 1", 38, RoomType.ComputerLab),
                new Room("CLAB202", "IT Lab 2", 38, RoomType.ComputerLab),
                
                // ========== SCIENCE LABORATORIES ==========
                new Room("SLAB101", "Physics Lab 1", 35, RoomType.ScienceLab),
                new Room("SLAB102", "Physics Lab 2", 35, RoomType.ScienceLab),
                new Room("SLAB103", "Chemistry Lab 1", 35, RoomType.ScienceLab),
                new Room("SLAB104", "Chemistry Lab 2", 35, RoomType.ScienceLab),
                new Room("SLAB105", "Biology Lab 1", 35, RoomType.ScienceLab),
                new Room("SLAB106", "Biology Lab 2", 35, RoomType.ScienceLab),
                new Room("SLAB107", "General Science Lab", 40, RoomType.ScienceLab),
                
                // ========== ENGINEERING LABS ==========
                new Room("ELAB101", "Circuit Lab", 30, RoomType.ScienceLab),
                new Room("ELAB102", "Electronics Lab", 30, RoomType.ScienceLab),
                new Room("ELAB103", "Engineering Workshop", 25, RoomType.Regular),
                
                // ========== REGULAR CLASSROOMS (Building A) ==========
                new Room("A101", "Room A101", 40, RoomType.Regular),
                new Room("A102", "Room A102", 40, RoomType.Regular),
                new Room("A103", "Room A103", 40, RoomType.Regular),
                new Room("A104", "Room A104", 40, RoomType.Regular),
                new Room("A105", "Room A105", 35, RoomType.Regular),
                new Room("A201", "Room A201", 40, RoomType.Regular),
                new Room("A202", "Room A202", 40, RoomType.Regular),
                new Room("A203", "Room A203", 40, RoomType.Regular),
                new Room("A204", "Room A204", 40, RoomType.Regular),
                new Room("A205", "Room A205", 35, RoomType.Regular),
                new Room("A301", "Room A301", 38, RoomType.Regular),
                new Room("A302", "Room A302", 38, RoomType.Regular),
                new Room("A303", "Room A303", 38, RoomType.Regular),
                new Room("A304", "Room A304", 35, RoomType.Regular),
                new Room("A305", "Room A305", 35, RoomType.Regular),
                
                // ========== REGULAR CLASSROOMS (Building B) ==========
                new Room("B101", "Room B101", 45, RoomType.Regular),
                new Room("B102", "Room B102", 45, RoomType.Regular),
                new Room("B103", "Room B103", 40, RoomType.Regular),
                new Room("B104", "Room B104", 40, RoomType.Regular),
                new Room("B105", "Room B105", 40, RoomType.Regular),
                new Room("B201", "Room B201", 45, RoomType.Regular),
                new Room("B202", "Room B202", 45, RoomType.Regular),
                new Room("B203", "Room B203", 40, RoomType.Regular),
                new Room("B204", "Room B204", 40, RoomType.Regular),
                new Room("B205", "Room B205", 40, RoomType.Regular),
                new Room("B301", "Room B301", 42, RoomType.Regular),
                new Room("B302", "Room B302", 42, RoomType.Regular),
                new Room("B303", "Room B303", 38, RoomType.Regular),
                new Room("B304", "Room B304", 38, RoomType.Regular),
                new Room("B305", "Room B305", 38, RoomType.Regular),
                
                // ========== LECTURE HALLS ==========
                new Room("LH1", "Lecture Hall 1", 80, RoomType.LectureHall),
                new Room("LH2", "Lecture Hall 2", 80, RoomType.LectureHall),
                new Room("LH3", "Lecture Hall 3", 70, RoomType.LectureHall),
                new Room("LH4", "Lecture Hall 4", 70, RoomType.LectureHall),
                new Room("LH5", "Lecture Hall 5", 60, RoomType.LectureHall),
                new Room("LH6", "Lecture Hall 6", 60, RoomType.LectureHall),
                
                // ========== BUSINESS CLASSROOMS ==========
                new Room("BUS101", "Business Room 1", 45, RoomType.Regular),
                new Room("BUS102", "Business Room 2", 45, RoomType.Regular),
                new Room("BUS103", "Business Room 3", 40, RoomType.Regular),
                new Room("BUS104", "Business Room 4", 40, RoomType.Regular),
                new Room("BUS201", "Business Room 5", 42, RoomType.Regular),
                new Room("BUS202", "Business Room 6", 42, RoomType.Regular),
                
                // ========== GYM & SPECIAL ROOMS ==========
                new Room("GYM1", "Gymnasium", 100, RoomType.Regular),
                new Room("DRAW1", "Drawing Room 1", 35, RoomType.Regular),
                new Room("DRAW2", "Drawing Room 2", 35, RoomType.Regular)
            };
    }

    private void InitializeTimeSlots()
    {
        TimeSlots = new List<TimeSlot>();

        string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        // Time slots with different durations
        var timeSchedule = new[]
        {
                ("07:00", "08:00", 1.0),   // 1 hour
                ("07:00", "08:30", 1.5),   // 1.5 hours
                ("07:00", "09:00", 2.0),   // 2 hours
                ("07:00", "10:00", 3.0),   // 3 hours
                
                ("08:00", "09:00", 1.0),
                ("08:00", "09:30", 1.5),
                ("08:00", "10:00", 2.0),

                ("09:00", "10:00", 1.0),
                ("09:00", "10:30", 1.5),
                ("09:00", "11:00", 2.0),

                ("10:00", "11:00", 1.0),
                ("10:00", "11:30", 1.5),
                ("10:00", "12:00", 2.0),

                ("11:00", "12:00", 1.0),
                ("11:00", "12:30", 1.5),

                ("13:00", "14:00", 1.0),
                ("13:00", "14:30", 1.5),
                ("13:00", "15:00", 2.0),
                ("13:00", "16:00", 3.0),

                ("14:00", "15:00", 1.0),
                ("14:00", "15:30", 1.5),
                ("14:00", "16:00", 2.0),

                ("15:00", "16:00", 1.0),
                ("15:00", "16:30", 1.5),
                ("15:00", "17:00", 2.0),

                ("16:00", "17:00", 1.0),
                ("16:00", "17:30", 1.5),
                ("16:00", "18:00", 2.0),

                ("17:00", "18:00", 1.0),
                ("17:00", "18:30", 1.5)
            };

        int id = 1;
        foreach (var day in days)
        {
            foreach (var (start, end, duration) in timeSchedule)
            {
                TimeSlots.Add(new TimeSlot($"T{id}", day, start, end));
                id++;
            }
        }
    }

    private void InitializeSections()
    {
        Sections = new List<Section>();

        // ========== BSCS SECTIONS ==========
        var bscs1a = new Section("SEC1", "BSCS-1A", "PROG1", 1, 1, "2024-2025", 38);
        bscs1a.SubjectIds.AddRange(new[] { "CS101", "MATH101", "ENG101", "FIL101", "HIST101", "PE101", "NSTP101" });
        Sections.Add(bscs1a);

        var bscs1b = new Section("SEC2", "BSCS-1B", "PROG1", 1, 1, "2024-2025", 40);
        bscs1b.SubjectIds.AddRange(new[] { "CS101", "MATH101", "ENG101", "FIL101", "HIST101", "PE101", "NSTP101" });
        Sections.Add(bscs1b);

        var bscs2a = new Section("SEC3", "BSCS-2A", "PROG1", 2, 1, "2024-2025", 35);
        bscs2a.SubjectIds.AddRange(new[] { "CS102", "CS201", "MATH102", "MATH201", "PHY101", "ENG102", "SOC101" });
        Sections.Add(bscs2a);

        var bscs3a = new Section("SEC4", "BSCS-3A", "PROG1", 3, 1, "2024-2025", 32);
        bscs3a.SubjectIds.AddRange(new[] { "CS202", "CS301", "CS302", "MATH202", "MATH301", "PHY102" });
        Sections.Add(bscs3a);

        var bscs4a = new Section("SEC5", "BSCS-4A", "PROG1", 4, 1, "2024-2025", 30);
        bscs4a.SubjectIds.AddRange(new[] { "CS401", "CS402", "MATH302", "MATH401" });
        Sections.Add(bscs4a);

        // ========== BSIT SECTIONS ==========
        var bsit1a = new Section("SEC6", "BSIT-1A", "PROG2", 1, 1, "2024-2025", 42);
        bsit1a.SubjectIds.AddRange(new[] { "IT101", "CS101", "MATH101", "ENG101", "FIL101", "PE101", "NSTP101" });
        Sections.Add(bsit1a);

        var bsit2a = new Section("SEC7", "BSIT-2A", "PROG2", 2, 1, "2024-2025", 38);
        bsit2a.SubjectIds.AddRange(new[] { "IT201", "CS102", "MATH102", "MATH202", "ENG102", "SOC101" });
        Sections.Add(bsit2a);

        var bsit3a = new Section("SEC8", "BSIT-3A", "PROG2", 3, 1, "2024-2025", 35);
        bsit3a.SubjectIds.AddRange(new[] { "IT301", "CS201", "CS202", "MATH201" });
        Sections.Add(bsit3a);

        var bsit4a = new Section("SEC9", "BSIT-4A", "PROG2", 4, 1, "2024-2025", 32);
        bsit4a.SubjectIds.AddRange(new[] { "IT401", "CS401" });
        Sections.Add(bsit4a);

        // ========== BSIS SECTIONS ==========
        var bsis1a = new Section("SEC10", "BSIS-1A", "PROG3", 1, 1, "2024-2025", 36);
        bsis1a.SubjectIds.AddRange(new[] { "IS101", "CS101", "MATH101", "ENG101", "FIL101", "PE101", "NSTP101" });
        Sections.Add(bsis1a);

        var bsis2a = new Section("SEC11", "BSIS-2A", "PROG3", 2, 1, "2024-2025", 34);
        bsis2a.SubjectIds.AddRange(new[] { "IS201", "CS102", "MATH102", "BUS101", "ENG102" });
        Sections.Add(bsis2a);

        var bsis3a = new Section("SEC12", "BSIS-3A", "PROG3", 3, 1, "2024-2025", 30);
        bsis3a.SubjectIds.AddRange(new[] { "IS301", "CS201", "BUS201", "BUS301" });
        Sections.Add(bsis3a);

        // ========== BSMath SECTIONS ==========
        var bsmath1a = new Section("SEC13", "BSMath-1A", "PROG4", 1, 1, "2024-2025", 32);
        bsmath1a.SubjectIds.AddRange(new[] { "MATH101", "MATH102", "PHY101", "ENG101", "FIL101", "PE101", "NSTP101" });
        Sections.Add(bsmath1a);

        var bsmath2a = new Section("SEC14", "BSMath-2A", "PROG4", 2, 1, "2024-2025", 30);
        bsmath2a.SubjectIds.AddRange(new[] { "MATH201", "MATH202", "MATH301", "PHY102", "ENG102" });
        Sections.Add(bsmath2a);

        var bsmath3a = new Section("SEC15", "BSMath-3A", "PROG4", 3, 1, "2024-2025", 28);
        bsmath3a.SubjectIds.AddRange(new[] { "MATH302", "MATH401", "MATH402", "PHY201" });
        Sections.Add(bsmath3a);

        // ========== BSPhysics SECTIONS ==========
        var bsphy1a = new Section("SEC16", "BSPhy-1A", "PROG5", 1, 1, "2024-2025", 28);
        bsphy1a.SubjectIds.AddRange(new[] { "PHY101", "MATH101", "MATH102", "ENG101", "FIL101", "PE101", "NSTP101" });
        Sections.Add(bsphy1a);

        var bsphy2a = new Section("SEC17", "BSPhy-2A", "PROG5", 2, 1, "2024-2025", 26);
        bsphy2a.SubjectIds.AddRange(new[] { "PHY102", "PHY201", "MATH201", "MATH301", "ENG102" });
        Sections.Add(bsphy2a);

        var bsphy3a = new Section("SEC18", "BSPhy-3A", "PROG5", 3, 1, "2024-2025", 24);
        bsphy3a.SubjectIds.AddRange(new[] { "PHY202", "PHY301", "PHY302", "MATH302" });
        Sections.Add(bsphy3a);

        // ========== BSBio SECTIONS ==========
        var bsbio1a = new Section("SEC19", "BSBio-1A", "PROG6", 1, 1, "2024-2025", 35);
        bsbio1a.SubjectIds.AddRange(new[] { "BIO101", "MATH101", "ENG101", "FIL101", "HIST101", "PE101", "NSTP101" });
        Sections.Add(bsbio1a);

        var bsbio2a = new Section("SEC20", "BSBio-2A", "PROG6", 2, 1, "2024-2025", 32);
        bsbio2a.SubjectIds.AddRange(new[] { "BIO102", "BIO201", "MATH102", "PHY101", "ENG102" });
        Sections.Add(bsbio2a);

        var bsbio3a = new Section("SEC21", "BSBio-3A", "PROG6", 3, 1, "2024-2025", 30);
        bsbio3a.SubjectIds.AddRange(new[] { "BIO202", "BIO301", "BIO302", "PHY102" });
        Sections.Add(bsbio3a);

        // ========== BSCE SECTIONS ==========
        var bsce1a = new Section("SEC22", "BSCE-1A", "PROG7", 1, 1, "2024-2025", 40);
        bsce1a.SubjectIds.AddRange(new[] { "CE101", "MATH101", "MATH102", "PHY101", "ENG101", "PE101", "NSTP101" });
        Sections.Add(bsce1a);

        var bsce2a = new Section("SEC23", "BSCE-2A", "PROG7", 2, 1, "2024-2025", 38);
        bsce2a.SubjectIds.AddRange(new[] { "CE201", "MATH201", "MATH301", "PHY102", "ENG102" });
        Sections.Add(bsce2a);

        var bsce3a = new Section("SEC24", "BSCE-3A", "PROG7", 3, 1, "2024-2025", 35);
        bsce3a.SubjectIds.AddRange(new[] { "CE301", "MATH202", "MATH302" });
        Sections.Add(bsce3a);

        var bsce4a = new Section("SEC25", "BSCE-4A", "PROG7", 4, 1, "2024-2025", 32);
        bsce4a.SubjectIds.AddRange(new[] { "CE401" });
        Sections.Add(bsce4a);

        // ========== BSEE SECTIONS ==========
        var bsee1a = new Section("SEC26", "BSEE-1A", "PROG8", 1, 1, "2024-2025", 36);
        bsee1a.SubjectIds.AddRange(new[] { "EE101", "MATH101", "MATH102", "PHY101", "ENG101", "PE101", "NSTP101" });
        Sections.Add(bsee1a);

        var bsee2a = new Section("SEC27", "BSEE-2A", "PROG8", 2, 1, "2024-2025", 34);
        bsee2a.SubjectIds.AddRange(new[] { "EE201", "MATH201", "MATH301", "PHY102", "ENG102" });
        Sections.Add(bsee2a);

        var bsee3a = new Section("SEC28", "BSEE-3A", "PROG8", 3, 1, "2024-2025", 32);
        bsee3a.SubjectIds.AddRange(new[] { "EE301", "MATH202" });
        Sections.Add(bsee3a);

        var bsee4a = new Section("SEC29", "BSEE-4A", "PROG8", 4, 1, "2024-2025", 30);
        bsee4a.SubjectIds.AddRange(new[] { "EE401" });
        Sections.Add(bsee4a);

        // ========== BSME SECTIONS ==========
        var bsme1a = new Section("SEC30", "BSME-1A", "PROG9", 1, 1, "2024-2025", 38);
        bsme1a.SubjectIds.AddRange(new[] { "ME101", "MATH101", "MATH102", "PHY101", "ENG101", "PE101", "NSTP101" });
        Sections.Add(bsme1a);

        var bsme2a = new Section("SEC31", "BSME-2A", "PROG9", 2, 1, "2024-2025", 36);
        bsme2a.SubjectIds.AddRange(new[] { "ME201", "MATH201", "PHY102", "ENG102" });
        Sections.Add(bsme2a);

        var bsme3a = new Section("SEC32", "BSME-3A", "PROG9", 3, 1, "2024-2025", 34);
        bsme3a.SubjectIds.AddRange(new[] { "ME301", "MATH301" });
        Sections.Add(bsme3a);

        var bsme4a = new Section("SEC33", "BSME-4A", "PROG9", 4, 1, "2024-2025", 32);
        bsme4a.SubjectIds.AddRange(new[] { "ME401" });
        Sections.Add(bsme4a);

        // ========== BSBA SECTIONS ==========
        var bsba1a = new Section("SEC34", "BSBA-1A", "PROG10", 1, 1, "2024-2025", 45);
        bsba1a.SubjectIds.AddRange(new[] { "BUS101", "MATH101", "ENG101", "FIL101", "HIST101", "PE101", "NSTP101" });
        Sections.Add(bsba1a);

        var bsba1b = new Section("SEC35", "BSBA-1B", "PROG10", 1, 1, "2024-2025", 45);
        bsba1b.SubjectIds.AddRange(new[] { "BUS101", "MATH101", "ENG101", "FIL101", "HIST101", "PE101", "NSTP101" });
        Sections.Add(bsba1b);

        var bsba2a = new Section("SEC36", "BSBA-2A", "PROG10", 2, 1, "2024-2025", 42);
        bsba2a.SubjectIds.AddRange(new[] { "BUS201", "ACC101", "MATH102", "ENG102", "SOC101" });
        Sections.Add(bsba2a);

        var bsba3a = new Section("SEC37", "BSBA-3A", "PROG10", 3, 1, "2024-2025", 40);
        bsba3a.SubjectIds.AddRange(new[] { "BUS301", "ACC102", "BUS201" });
        Sections.Add(bsba3a);

        var bsba4a = new Section("SEC38", "BSBA-4A", "PROG10", 4, 1, "2024-2025", 38);
        bsba4a.SubjectIds.AddRange(new[] { "BUS401", "ACC201" });
        Sections.Add(bsba4a);

        // ========== BSA SECTIONS ==========
        var bsa1a = new Section("SEC39", "BSA-1A", "PROG11", 1, 1, "2024-2025", 42);
        bsa1a.SubjectIds.AddRange(new[] { "ACC101", "MATH101", "ENG101", "FIL101", "HIST101", "PE101", "NSTP101" });
        Sections.Add(bsa1a);

        var bsa2a = new Section("SEC40", "BSA-2A", "PROG11", 2, 1, "2024-2025", 40);
        bsa2a.SubjectIds.AddRange(new[] { "ACC102", "ACC201", "MATH102", "BUS101", "ENG102" });
        Sections.Add(bsa2a);

        var bsa3a = new Section("SEC41", "BSA-3A", "PROG11", 3, 1, "2024-2025", 38);
        bsa3a.SubjectIds.AddRange(new[] { "ACC301", "BUS201", "BUS301" });
        Sections.Add(bsa3a);

        var bsa4a = new Section("SEC42", "BSA-4A", "PROG11", 4, 1, "2024-2025", 35);
        bsa4a.SubjectIds.AddRange(new[] { "BUS401" });
        Sections.Add(bsa4a);
    }
}