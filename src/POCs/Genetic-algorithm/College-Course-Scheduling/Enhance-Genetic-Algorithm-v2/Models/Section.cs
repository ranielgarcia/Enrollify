namespace Enhance_Genetic_Algorithm_v2.Models;
public class Section
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CourseId { get; set; }
    public int YearLevel { get; set; }
    public int Semester { get; set; }
    public string SchoolYear { get; set; }
    public int StudentCount { get; set; }
    public List<string> SubjectIds { get; set; }  // Subjects assigned to this section

    public Section(string id, string name, string courseId, int yearLevel,
                  int semester, string schoolYear, int studentCount)
    {
        Id = id;
        Name = name;
        CourseId = courseId;
        YearLevel = yearLevel;
        Semester = semester;
        SchoolYear = schoolYear;
        StudentCount = studentCount;
        SubjectIds = new List<string>();
    }

    public override string ToString() => $"{Name} (Year {YearLevel}, Sem {Semester})";
}