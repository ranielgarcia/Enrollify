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
    public double TimePatternPenalty { get; set; } = 20;
    public double DayPatternPenalty { get; set; } = 10;
    public double ProfessorGapPenalty { get; set; } = 3;
    public double AfternoonPenalty { get; set; } = 2;
    public double RoomProximityPenalty { get; set; } = 5;

    public double DaysPerWeekPenalty { get; set; } = 250;
    public double HoursPerDayPenalty { get; set; } = 200;

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

    public static GAConfiguration RealWorldUniversity => new GAConfiguration
    {
        // Population & Generations
        PopulationSize = 300,              // Large population for complex search space
        MaxGenerations = 3000,             // More generations for convergence

        // Genetic Operators
        CrossoverRate = 0.90,              // High crossover to combine good solutions
        MutationRate = 0.25,               // Higher mutation for exploration
        TournamentSize = 10,               // Stronger selection pressure

        // Elitism & Adaptation
        EliteCount = 30,                   // Keep more best solutions (10% of population)
        UseAdaptiveMutation = true,
        AdaptiveMutationThreshold = 150,   // Increase mutation after 150 stagnant gens

        // Target
        TargetFitness = 920,               // Realistic target (not 950) for this complexity

        // Penalty Weights (calibrated for your constraints)
        RoomCapacityPenalty = 100,
        RoomTypePenalty = 150,
        SectionConflictPenalty = 250,      // INCREASED - most critical
        ProfessorConflictPenalty = 200,    // INCREASED - very important
        RoomConflictPenalty = 200,         // INCREASED - very important
        QualificationPenalty = 150,
        DaysPerWeekPenalty = 250,          // NEW - critical for V4
        HoursPerDayPenalty = 200,          // NEW - critical for V4
        TimePatternPenalty = 25,
        DayPatternPenalty = 10,
        ProfessorGapPenalty = 3,
        AfternoonPenalty = 2,
        RoomProximityPenalty = 5
    };
}