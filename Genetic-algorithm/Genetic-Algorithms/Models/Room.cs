namespace Genetic_Algorithms.Models;
public class Room
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }

    public Room(string id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public override string ToString() => $"{Id} ({Capacity} seats)";
}