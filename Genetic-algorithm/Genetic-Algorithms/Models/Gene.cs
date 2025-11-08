namespace Genetic_Algorithms.Models;
public class Gene
{
    public Course Course { get; set; }
    public Room Room { get; set; }
    public TimeSlot TimeSlot { get; set; }

    public Gene(Course course, Room room, TimeSlot timeSlot)
    {
        Course = course;
        Room = room;
        TimeSlot = timeSlot;
    }

    // Create a deep copy
    public Gene Clone()
    {
        return new Gene(Course, Room, TimeSlot);
    }

    public override string ToString()
    {
        return $"{Course.Name,-20} | {Room.Name,-10} | {TimeSlot,-12} | Prof. {Course.ProfessorId}";
    }
}