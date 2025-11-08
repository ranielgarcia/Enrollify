using Enhance_Genetic_Algorithm.Models;

namespace Enhance_Genetic_Algorithm;

// Replace current Course entity model to Subject and further enhance the algorithm to consider the following in schuduling
// 1. Program which represent a degree program or academic track (e.g. BS Computer Science)
// 2. Section which Represents a group of students assigned to the same subject offering (e.g., “BSCS-2A”, “ENG101-A”).
//          Section Attributes: Id, Name, CourseId, YearLevel, Semester, SchoolYear
// 3. Each section has a set of subjects which should not have any conflict with other section
// 4. And some subjects prefers to have a same time slot even on different days
// 5. two or more professor teach the same subject


public class ScheduleData
{
    public List<Course> Courses { get; set; }
    public List<Room> Rooms { get; set; }
    public List<TimeSlot> TimeSlots { get; set; }
    public List<Professor> Professors { get; set; }

    public ScheduleData()
    {
        InitializeProfessors();
        InitializeCourses();
        InitializeRooms();
        InitializeTimeSlots();
    }

    private void InitializeProfessors()
    {
        Professors = new List<Professor>
            {
                new Professor("P1", "Dr. Smith"),
                new Professor("P2", "Dr. Jones"),
                new Professor("P3", "Dr. Williams"),
                new Professor("P4", "Dr. Brown"),
                new Professor("P5", "Dr. Davis"),
                new Professor("P6", "Dr. Miller"),
                new Professor("P7", "Dr. Wilson"),
                new Professor("P8", "Dr. Moore"),
                new Professor("P9", "Dr. Taylor"),
                new Professor("P10", "Dr. Anderson")
            };
    }

    private void InitializeCourses()
    {
        Courses = new List<Course>
            {
                // Computer Science (3 sessions per week for labs)
                new Course("CS101", "Intro to Programming", 35, "P1", CourseType.ComputerScience, 3),
                new Course("CS102", "Data Structures", 30, "P1", CourseType.ComputerScience, 3),
                new Course("CS201", "Algorithms", 28, "P2", CourseType.ComputerScience, 2),
                new Course("CS301", "Database Systems", 25, "P2", CourseType.ComputerScience, 2),
                new Course("CS401", "AI & Machine Learning", 20, "P3", CourseType.ComputerScience, 2),
                
                // Mathematics (2 sessions per week)
                new Course("MATH101", "Calculus I", 40, "P4", CourseType.Mathematics, 2),
                new Course("MATH102", "Calculus II", 38, "P4", CourseType.Mathematics, 2),
                new Course("MATH201", "Linear Algebra", 35, "P5", CourseType.Mathematics, 2),
                new Course("MATH301", "Statistics", 32, "P5", CourseType.Mathematics, 2),
                
                // Physics (2 sessions: 1 lecture + 1 lab)
                new Course("PHY101", "Physics I", 40, "P6", CourseType.Physics, 2),
                new Course("PHY102", "Physics II", 38, "P6", CourseType.Physics, 2),
                new Course("PHY201", "Quantum Mechanics", 25, "P7", CourseType.Physics, 2),
                
                // Chemistry (2 sessions: 1 lecture + 1 lab)
                new Course("CHEM101", "General Chemistry", 35, "P7", CourseType.Chemistry, 2),
                new Course("CHEM201", "Organic Chemistry", 30, "P8", CourseType.Chemistry, 2),
                
                // Biology (2 sessions: 1 lecture + 1 lab)
                new Course("BIO101", "General Biology", 40, "P8", CourseType.Biology, 2),
                new Course("BIO201", "Genetics", 32, "P9", CourseType.Biology, 2),
                
                // Engineering (1 session per week)
                new Course("ENG101", "Engineering Graphics", 35, "P9", CourseType.Engineering, 1),
                new Course("ENG201", "Circuit Analysis", 30, "P10", CourseType.Engineering, 1),
                
                // Business (1 session per week)
                new Course("BUS101", "Business Management", 45, "P10", CourseType.Business, 1),
                new Course("BUS201", "Marketing", 40, "P1", CourseType.Business, 1)
            };
    }

    private void InitializeRooms()
    {
        Rooms = new List<Room>
            {
                // Computer Labs (for CS courses only)
                new Room("CLAB101", "Computer Lab 1", 35, RoomType.ComputerLab),
                new Room("CLAB102", "Computer Lab 2", 35, RoomType.ComputerLab),
                new Room("CLAB201", "Computer Lab 3", 30, RoomType.ComputerLab),
                
                // Science Labs (for Physics, Chemistry, Biology)
                new Room("SLAB101", "Physics Lab", 35, RoomType.ScienceLab),
                new Room("SLAB102", "Chemistry Lab", 35, RoomType.ScienceLab),
                new Room("SLAB103", "Biology Lab", 35, RoomType.ScienceLab),
                
                // Regular Classrooms
                new Room("R101", "Room 101", 30, RoomType.Regular),
                new Room("R102", "Room 102", 30, RoomType.Regular),
                new Room("R103", "Room 103", 35, RoomType.Regular),
                new Room("R104", "Room 104", 35, RoomType.Regular),
                new Room("R105", "Room 105", 40, RoomType.Regular),
                
                // Lecture Halls (large capacity)
                new Room("LH301", "Lecture Hall 1", 50, RoomType.LectureHall),
                new Room("LH302", "Lecture Hall 2", 50, RoomType.LectureHall),
                new Room("LH303", "Lecture Hall 3", 45, RoomType.LectureHall)
            };
    }

    private void InitializeTimeSlots()
    {
        TimeSlots = new List<TimeSlot>
            {
                // Monday
                new TimeSlot("T1", "Monday", "08:00"),
                new TimeSlot("T2", "Monday", "09:30"),
                new TimeSlot("T3", "Monday", "11:00"),
                new TimeSlot("T4", "Monday", "13:00"),
                new TimeSlot("T5", "Monday", "14:30"),
                new TimeSlot("T6", "Monday", "16:00"),
                
                // Tuesday
                new TimeSlot("T7", "Tuesday", "08:00"),
                new TimeSlot("T8", "Tuesday", "09:30"),
                new TimeSlot("T9", "Tuesday", "11:00"),
                new TimeSlot("T10", "Tuesday", "13:00"),
                new TimeSlot("T11", "Tuesday", "14:30"),
                new TimeSlot("T12", "Tuesday", "16:00"),
                
                // Wednesday
                new TimeSlot("T13", "Wednesday", "08:00"),
                new TimeSlot("T14", "Wednesday", "09:30"),
                new TimeSlot("T15", "Wednesday", "11:00"),
                new TimeSlot("T16", "Wednesday", "13:00"),
                new TimeSlot("T17", "Wednesday", "14:30"),
                new TimeSlot("T18", "Wednesday", "16:00"),
                
                // Thursday
                new TimeSlot("T19", "Thursday", "08:00"),
                new TimeSlot("T20", "Thursday", "09:30"),
                new TimeSlot("T21", "Thursday", "11:00"),
                new TimeSlot("T22", "Thursday", "13:00"),
                new TimeSlot("T23", "Thursday", "14:30"),
                new TimeSlot("T24", "Thursday", "16:00"),
                
                // Friday
                new TimeSlot("T25", "Friday", "08:00"),
                new TimeSlot("T26", "Friday", "09:30"),
                new TimeSlot("T27", "Friday", "11:00"),
                new TimeSlot("T28", "Friday", "13:00"),
                new TimeSlot("T29", "Friday", "14:30"),
                new TimeSlot("T30", "Friday", "16:00")
            };
    }
}