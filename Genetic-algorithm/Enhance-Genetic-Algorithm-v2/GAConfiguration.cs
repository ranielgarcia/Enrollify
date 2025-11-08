namespace Enhance_Genetic_Algorithm_v2;
public class GAConfiguration
{
    public int PopulationSize { get; set; } = 200;
    public int MaxGenerations { get; set; } = 1500;
    public double CrossoverRate { get; set; } = 0.85;
    public double MutationRate { get; set; } = 0.2;
    public int TournamentSize { get; set; } = 7;
    public double TargetFitness { get; set; } = 950;
    public int EliteCount { get; set; } = 10;
    public bool UseAdaptiveMutation { get; set; } = true;
    public int AdaptiveMutationThreshold { get; set; } = 100;

    // Penalty weights
    public double RoomCapacityPenalty { get; set; } = 100;
    public double RoomTypePenalty { get; set; } = 150;
    public double SectionConflictPenalty { get; set; } = 200;
    public double ProfessorConflictPenalty { get; set; } = 100;
    public double RoomConflictPenalty { get; set; } = 100;
    public double QualificationPenalty { get; set; } = 150;
    public double TimePatternPenalty { get; set; } = 15;
    public double DayPatternPenalty { get; set; } = 10;
    public double ProfessorGapPenalty { get; set; } = 3;
    public double AfternoonPenalty { get; set; } = 2;
    public double RoomProximityPenalty { get; set; } = 5;

    public static GAConfiguration Default => new GAConfiguration();

    public static GAConfiguration FastConvergence => new GAConfiguration
    {
        PopulationSize = 150,
        MaxGenerations = 1000,
        MutationRate = 0.25,
        TournamentSize = 8
    };

    public static GAConfiguration HighQuality => new GAConfiguration
    {
        PopulationSize = 300,
        MaxGenerations = 2000,
        CrossoverRate = 0.9,
        MutationRate = 0.15,
        TournamentSize = 10,
        EliteCount = 20
    };
}