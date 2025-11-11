namespace Enhance_Genetic_Algorithm_v2.Models;
public class Room
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public RoomType Type { get; set; }

    public Room(string id, string name, int capacity, RoomType type)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        Type = type;
    }

    public bool IsCompatibleWith(Subject subject)
    {
        switch (Type)
        {
            case RoomType.ComputerLab:
                return subject.Type == SubjectType.ComputerScience;

            case RoomType.ScienceLab:
                return subject.Type == SubjectType.Physics ||
                       subject.Type == SubjectType.Chemistry ||
                       subject.Type == SubjectType.Biology;

            case RoomType.Regular:
            case RoomType.LectureHall:
                return true;

            default:
                return true;
        }
    }

    public override string ToString() => $"{Name} ({Type}, {Capacity} seats)";
}