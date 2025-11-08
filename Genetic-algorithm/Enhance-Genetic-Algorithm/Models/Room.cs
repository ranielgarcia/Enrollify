namespace Enhance_Genetic_Algorithm.Models;
public class Room
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public RoomType Type { get; set; }  // NEW: Room type

    public Room(string id, string name, int capacity, RoomType type)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        Type = type;
    }

    // Check if this room can be used for a course
    public bool IsCompatibleWith(Course course)
    {
        switch (Type)
        {
            case RoomType.ComputerLab:
                return course.Type == CourseType.ComputerScience;

            case RoomType.ScienceLab:
                return course.Type == CourseType.Physics ||
                       course.Type == CourseType.Chemistry ||
                       course.Type == CourseType.Biology;

            case RoomType.Regular:
            case RoomType.LectureHall:
                return true;  // Can be used by any course

            default:
                return true;
        }
    }

    public override string ToString() => $"{Name} ({Type}, {Capacity} seats)";
}