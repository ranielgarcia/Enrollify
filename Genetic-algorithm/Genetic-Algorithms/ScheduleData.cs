using Genetic_Algorithms.Models;

namespace Genetic_Algorithms;

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
                new Professor("P10", "Dr. Anderson"),
                new Professor("P11", "Dr. Thomas"),
                new Professor("P12", "Dr. Jackson"),
                new Professor("P13", "Dr. White"),
                new Professor("P14", "Dr. Harris"),
                new Professor("P15", "Dr. Martin")
            };
    }

    private void InitializeCourses()
    {
        Courses = new List<Course>
            {
                // Computer Science
                new Course("CS101", "Introduction to Programming", 45, "P1"),
                new Course("CS102", "Data Structures", 38, "P1"),
                new Course("CS201", "Algorithms", 32, "P2"),
                new Course("CS202", "Database Systems", 35, "P2"),
                new Course("CS301", "Operating Systems", 28, "P3"),
                new Course("CS302", "Computer Networks", 30, "P3"),
                new Course("CS401", "Artificial Intelligence", 25, "P4"),
                new Course("CS402", "Machine Learning", 22, "P4"),
                new Course("CS403", "Computer Graphics", 20, "P5"),
                new Course("CS404", "Software Engineering", 40, "P5"),
                new Course("CS501", "Advanced Algorithms", 18, "P6"),
                new Course("CS502", "Distributed Systems", 20, "P6"),
                
                // Mathematics
                new Course("MATH101", "Calculus I", 50, "P7"),
                new Course("MATH102", "Calculus II", 45, "P7"),
                new Course("MATH201", "Linear Algebra", 42, "P8"),
                new Course("MATH202", "Discrete Mathematics", 38, "P8"),
                new Course("MATH301", "Probability Theory", 30, "P9"),
                new Course("MATH302", "Statistics", 35, "P9"),
                new Course("MATH401", "Abstract Algebra", 20, "P10"),
                new Course("MATH402", "Real Analysis", 18, "P10"),
                
                // Physics
                new Course("PHY101", "Physics I", 48, "P11"),
                new Course("PHY102", "Physics II", 45, "P11"),
                new Course("PHY201", "Mechanics", 35, "P12"),
                new Course("PHY202", "Electromagnetism", 32, "P12"),
                new Course("PHY301", "Quantum Mechanics", 25, "P13"),
                new Course("PHY302", "Thermodynamics", 28, "P13"),
                
                // Engineering
                new Course("ENG101", "Engineering Graphics", 40, "P14"),
                new Course("ENG102", "Technical Writing", 35, "P14"),
                new Course("ENG201", "Circuit Analysis", 30, "P15"),
                new Course("ENG202", "Digital Logic Design", 28, "P15"),
                new Course("ENG301", "Control Systems", 25, "P1"),
                new Course("ENG302", "Signal Processing", 22, "P2"),
                
                // Business
                new Course("BUS101", "Introduction to Business", 55, "P3"),
                new Course("BUS102", "Accounting Principles", 50, "P4"),
                new Course("BUS201", "Marketing Management", 45, "P5"),
                new Course("BUS202", "Financial Management", 40, "P6"),
                new Course("BUS301", "Operations Management", 35, "P7"),
                new Course("BUS302", "Strategic Management", 30, "P8"),
                
                // Chemistry
                new Course("CHEM101", "General Chemistry I", 45, "P9"),
                new Course("CHEM102", "General Chemistry II", 42, "P10"),
                new Course("CHEM201", "Organic Chemistry I", 35, "P11"),
                new Course("CHEM202", "Organic Chemistry II", 32, "P12"),
                
                // Biology
                new Course("BIO101", "General Biology I", 50, "P13"),
                new Course("BIO102", "General Biology II", 48, "P14"),
                new Course("BIO201", "Genetics", 35, "P15"),
                new Course("BIO202", "Microbiology", 32, "P1")
            };
    }

    private void InitializeRooms()
    {
        Rooms = new List<Room>
            {
                // Small classrooms
                new Room("R101", "Room 101", 25),
                new Room("R102", "Room 102", 25),
                new Room("R103", "Room 103", 30),
                new Room("R104", "Room 104", 30),
                new Room("R105", "Room 105", 28),
                
                // Medium classrooms
                new Room("R201", "Room 201", 35),
                new Room("R202", "Room 202", 35),
                new Room("R203", "Room 203", 40),
                new Room("R204", "Room 204", 40),
                new Room("R205", "Room 205", 38),
                
                // Large lecture halls
                new Room("R301", "Room 301", 50),
                new Room("R302", "Room 302", 50),
                new Room("R303", "Room 303", 55),
                new Room("R304", "Room 304", 55),
                new Room("R305", "Room 305", 60),
                
                // Labs
                new Room("LAB101", "Computer Lab 1", 35),
                new Room("LAB102", "Computer Lab 2", 35),
                new Room("LAB201", "Physics Lab", 30),
                new Room("LAB202", "Chemistry Lab", 30),
                new Room("LAB203", "Biology Lab", 30)
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