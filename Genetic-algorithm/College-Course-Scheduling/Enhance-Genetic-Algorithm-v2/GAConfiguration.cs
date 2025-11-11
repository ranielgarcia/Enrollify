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

    public double OverlapPenalty { get; set; } = 300;
    public double TimePreferencePenalty { get; set; } = 20;

    public double ExceedingProfessorMaxTimeSlotsPerDayPenalty { get; set; } = 150;

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
        // Core Parameters - OPTIMIZED for 42 sections, 240+ sessions
        PopulationSize = 600,              // INCREASED - more diversity needed
        MaxGenerations = 5000,             // INCREASED - complex constraints

        // Genetic Operators - TUNED for section conflicts
        CrossoverRate = 0.90,              // Slightly reduced for stability
        MutationRate = 0.15,               // REDUCED - too much was causing conflicts
        TournamentSize = 15,               // INCREASED - stronger selection

        // Elitism & Adaptation
        EliteCount = 60,                   // 10% of population
        UseAdaptiveMutation = true,
        AdaptiveMutationThreshold = 200,   // Be patient before increasing mutation

        // Target - realistic for this complexity
        TargetFitness = 900,               // Lowered from 920

        // Penalty Weights - BALANCED for your constraints
        RoomCapacityPenalty = 100,
        RoomTypePenalty = 150,
        SectionConflictPenalty = 300,      // HIGHEST PRIORITY
        OverlapPenalty = 550,              // NEW - CRITICAL
        ProfessorConflictPenalty = 250,    // INCREASED
        RoomConflictPenalty = 200,
        QualificationPenalty = 150,
        DaysPerWeekPenalty = 280,          // INCREASED
        HoursPerDayPenalty = 220,          // INCREASED
        TimePreferencePenalty = 25,        // NEW - soft constraint
        TimePatternPenalty = 15,
        DayPatternPenalty = 10,
        ProfessorGapPenalty = 3,
        AfternoonPenalty = 2,
        RoomProximityPenalty = 5,
        ExceedingProfessorMaxTimeSlotsPerDayPenalty = 150,
    };
}