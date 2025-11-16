namespace Enhance_Genetic_Algorithm_v2.Models;
public class Gene
{
    public Subject Subject { get; set; }
    public Section Section { get; set; }
    public Room Room { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public string ProfessorId { get; set; }  // Assigned professor for this offering

    public Gene(Subject subject, Section section, Room room, TimeSlot timeSlot, string professorId)
    {
        Subject = subject;
        Section = section;
        Room = room;
        TimeSlot = timeSlot;
        ProfessorId = professorId;
    }

    public Gene Clone()
    {
        return new Gene(Subject, Section, Room, TimeSlot, ProfessorId);
    }

    public override string ToString()
    {
        return $"{Subject.Code,-10} | {Section.Name,-12} | {Room.Name,-15} | {TimeSlot,-20} | Prof. {ProfessorId}";
    }
}