namespace Enhance_Genetic_Algorithm.Models;
public class Gene
{
    public Course Course { get; set; }
    public Room Room { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public int SessionNumber { get; set; }  // NEW: Which session (1, 2, 3...)

    public Gene(Course course, Room room, TimeSlot timeSlot, int sessionNumber = 1)
    {
        Course = course;
        Room = room;
        TimeSlot = timeSlot;
        SessionNumber = sessionNumber;
    }

    public Gene Clone()
    {
        return new Gene(Course, Room, TimeSlot, SessionNumber);
    }

    public override string ToString()
    {
        string session = Course.SessionsPerWeek > 1 ? $" (Session {SessionNumber})" : "";
        return $"{Course.Name,-30}{session} | {Room.Name,-15} | {TimeSlot,-15} | Prof. {Course.ProfessorId}";
    }
}