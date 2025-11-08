namespace Genetic_Algorithms.Models;
public class Professor
{
    public string Id { get; set; }
    public string Name { get; set; }

    public Professor(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;
}