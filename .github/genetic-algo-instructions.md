---
applyTo: "src/POCs/Genetic-algorithm/**
---

# College Course Scheduling - Genetic Algorithm

## Architecture Overview

This is a **genetic algorithm (GA) implementation** for solving university course scheduling. The system uses evolutionary computing principles—selection, crossover, and mutation—to optimize schedule assignments while satisfying hard and soft constraints.

### Core Domain Model

The scheduling problem uses a **4-level hierarchy**:

```
Course (e.g., BSCS) → Section (BSCS-1A) → Subject (CS101) → Gene (scheduled class)
```

- **Course**: Degree programs (BSCS, BSIT, BSMath) with department associations
- **Section**: Specific class groups with year level, semester, student count, and assigned subjects
- **Subject**: Individual courses with `DaysPerWeek`, `HoursPerDay`, `PreferredDayPattern` (MW/TTh/MWF), and qualified `ProfessorIds[]`
- **Gene**: A single scheduled class instance containing `Subject`, `Section`, `Room`, `TimeSlot`, and `ProfessorId`
- **Schedule** (Chromosome): Complete collection of Genes representing one possible solution

**Critical**: Each Subject must be scheduled exactly `DaysPerWeek` times per section, with timeslots matching `HoursPerDay` duration. For example, a 3-unit subject with `DaysPerWeek=2` and `HoursPerDay=1.5` requires two 1.5-hour sessions per week.

### Constraint System

The GA's fitness function uses penalty-based scoring (higher = better):

**Hard Constraints** (high penalties - must be satisfied):

- `SectionConflictPenalty=200`: No overlapping subjects within same section
- `OverlapPenalty=300`: No time overlaps within same section (checked via `TimeSlot.OverlapsWith()`)
- `DaysPerWeekPenalty=250`: Subject must be scheduled correct number of days
- `HoursPerDayPenalty=200`: Timeslot duration must match subject requirements
- `ProfessorConflictPenalty=100`: Professor cannot teach multiple classes simultaneously
- `RoomConflictPenalty=100`: Room cannot be double-booked
- `RoomCapacityPenalty=100`: Room capacity must fit section size
- `QualificationPenalty=150`: Professor must be in subject's `ProfessorIds` list

**Soft Constraints** (small penalties - for optimization):

- `TimePatternPenalty=20`: Prefer consistent times across days (e.g., always 9:00-10:30)
- `DayPatternPenalty=10`: Prefer subject's `PreferredDayPattern` (MW/TTh/MWF)
- `ProfessorGapPenalty=3`: Minimize idle time between professor's classes

See `GAConfiguration.cs` for all penalty weights. Use `GAConfiguration.RealWorldUniversity` for production-scale datasets (400 population, 4000 generations).

## Key Files

- **`GeneticAlgorithm.cs`**: Main GA loop with tournament selection, section-based crossover, and 4-type mutation
- **`Schedule.cs`**: Chromosome implementation with cached fitness calculation (`CalculateFitness()`)
- **`ConflictDetector.cs`**: Validates constraints and returns human-readable conflict messages
- **`ScheduleData.cs`**: Initializes all entities (42 sections, 60 professors, 240+ scheduled classes in current setup)
- **`GAConfiguration.cs`**: Tunable parameters—use presets like `Default`, `FastConvergence`, or `RealWorldUniversity`
- **`Program.cs`**: Entry point with interactive configuration selection and export options

## Development Workflows

### Building & Running

```bash
# Navigate to project directory
cd Genetic-algorithm/College-Course-Scheduling/Enhance-Genetic-Algorithm-v2

# Build
dotnet build

# Run with default configuration
dotnet run

# Choose configuration when prompted (1=Default, 2=Fast, 3=High Quality, 4=Real World)
```

### Testing Changes to GA Parameters

1. Modify penalty weights in `GAConfiguration.cs` (e.g., increase `SectionConflictPenalty` if sections still overlap)
2. Run with configuration preset matching your data scale
3. Monitor console output: `Gen XXXX: Fitness = XXX, Violations = X`
4. Target fitness: Default=950, RealWorldUniversity=900 (accounting for soft constraint penalties)

### GA Tuning Strategies

**When to increase Population Size:**

- Dataset has >30 sections with complex constraint interactions
- Getting stuck in local optima (fitness plateaus early and never improves)
- Many hard constraint violations persist (sections/professors/rooms conflicting)
- Rule of thumb: `PopulationSize = 5-10 × number of sections`

**When to increase Generations:**

- Fitness steadily improves but hasn't converged by final generation
- Target fitness not reached but violations decreasing
- Soft constraints improving slowly (time patterns, professor gaps)
- Rule of thumb: Monitor "No Improve" counter—if it stays <200 by end, add more generations

**When to adjust Mutation Rate:**

- **Increase** (0.25-0.4): Stuck at same fitness for 300+ generations, need more exploration
- **Decrease** (0.1-0.15): Fitness oscillating wildly, need more exploitation of good solutions
- Default 0.2 works for most cases; adaptive mutation auto-adjusts after `AdaptiveMutationThreshold`

**When to adjust Tournament Size:**

- **Increase** (10-15): Selection pressure too weak, bad schedules surviving
- **Decrease** (3-5): Premature convergence, losing diversity too fast
- Default 7-12 balances exploration vs exploitation

**Configuration Selection Guide:**

- **Default**: Testing, small datasets (<20 sections)
- **FastConvergence**: Quick prototyping, verifying constraint logic works
- **HighQuality**: Medium datasets (20-30 sections), need good results
- **RealWorldUniversity**: Production use, 40+ sections, willing to wait 2-5 minutes

### Adding New Constraints

1. Add penalty property to `GAConfiguration` (e.g., `public double NewPenalty { get; set; } = 50;`)
2. Implement check in `Schedule.CalculateFitness()` method
3. Add detection logic to `ConflictDetector.cs` for human-readable output
4. Update preset configurations with appropriate penalty weight

### Adding New Room Types

1. Add enum value to `Models/RoomType.cs`:

```csharp
public enum RoomType
{
    Regular,
    ComputerLab,
    ScienceLab,
    LectureHall,
    MusicRoom  // NEW
}
```

2. Update `Room.IsCompatibleWith()` in `Models/Room.cs`:

```csharp
public bool IsCompatibleWith(Subject subject)
{
    return Type switch
    {
        RoomType.ComputerLab => subject.RequiresLab &&
            (subject.Type == SubjectType.ComputerScience || subject.Type == SubjectType.Engineering),
        RoomType.ScienceLab => subject.RequiresLab &&
            (subject.Type == SubjectType.Physics || subject.Type == SubjectType.Chemistry),
        RoomType.MusicRoom => subject.Type == SubjectType.Music,  // NEW
        RoomType.LectureHall => subject.StudentCount >= 50,
        _ => true  // Regular rooms accept anything
    };
}
```

3. Add rooms in `ScheduleData.InitializeRooms()`:

```csharp
Rooms.Add(new Room("MUSIC101", "Music Room 1", RoomType.MusicRoom, 25, "Building E", "1st Floor"));
```

### Adding New Subject Types

1. Add enum value to `Models/SubjectType.cs`:

```csharp
public enum SubjectType
{
    ComputerScience,
    Mathematics,
    // ... existing types
    Music,  // NEW
    Arts    // NEW
}
```

2. Add subjects in `ScheduleData.InitializeSubjects()` with proper constraints:

```csharp
Subjects.Add(new Subject("MUS101", "MUS101", "Music Theory", SubjectType.Music,
    3, 2, 1.5, DayPattern.TTh));
Subjects["MUS101"].ProfessorIds.AddRange(new[] { "P61", "P62" });  // Qualified music professors
```

3. If requiring special rooms, set `RequiresLab` or create custom property:

```csharp
var musicSub = Subjects.First(s => s.Code == "MUS101");
// Music subjects will automatically filter to MusicRoom via IsCompatibleWith()
```

### Exporting Results

After GA completes, choose export format:

- **CSV**: Importable spreadsheet with all class details
- **By Section**: Text file grouped by section schedules
- **By Professor**: Text file showing each professor's teaching load
- **By Room**: Text file showing room utilization

Files saved with timestamp: `schedule_20251108_205818.csv`

## Code Patterns

### TimeSlot Operations

```csharp
// Check duration match
if (Math.Abs(timeSlot.GetDurationHours() - subject.HoursPerDay) > 0.1)
    penalty += HoursPerDayPenalty;

// Check overlap (same day, overlapping times)
if (timeSlot1.OverlapsWith(timeSlot2))
    penalty += OverlapPenalty;
```

### Subject-Section Grouping Pattern

```csharp
// Group genes by subject AND section to check DaysPerWeek
var subjectSectionSchedule = Genes.GroupBy(g =>
    new { subjectId = g.Subject.Id, sectionId = g.Section.Id });

foreach (var group in subjectSectionSchedule)
{
    var actualDays = group.Select(g => g.TimeSlot.Day).Distinct().Count();
    if (actualDays != group.First().Subject.DaysPerWeek)
        penalty += DaysPerWeekPenalty * Math.Abs(actualDays - subject.DaysPerWeek);
}
```

### Mutation Types (4 strategies)

1. **Room mutation**: Replace with compatible room (check `IsCompatibleWith()` and capacity)
2. **Timeslot mutation**: Replace with slot matching `PreferredDayPattern`
3. **Professor mutation**: Swap with different qualified professor from `ProfessorIds`
4. **Timeslot swap**: Exchange timeslots between two genes in same section

### Section-Based Crossover

Unlike traditional single-point crossover, this GA uses **section-based crossover**: randomly select which parent contributes each section's complete schedule. This preserves section coherence and reduces conflicts.

## Common Pitfalls

1. **Forgetting cached fitness**: `Schedule` caches fitness—clone schedules when modifying: `schedule.Clone()`
2. **Timeslot filtering**: Always filter by both duration (`GetDurationHours()`) and day pattern before random selection
3. **Professor qualification**: Check `Subject.ProfessorIds.Contains(professorId)` before assigning
4. **Section overlaps**: Use `TimeSlot.OverlapsWith()` for same-day overlap detection, not just ID equality
5. **Data initialization**: `ScheduleData` constructor auto-initializes all entities—don't call individual `Initialize*()` methods separately

## Units vs DaysPerWeek × HoursPerDay Relationship

**Standard Patterns** (total weekly hours = Units × 1.5):

| Units | Pattern | DaysPerWeek | HoursPerDay | Total Hours | Common Usage            |
| ----- | ------- | ----------- | ----------- | ----------- | ----------------------- |
| 3     | MWF     | 3           | 1.5         | 4.5         | Most lecture courses    |
| 3     | MW/TTh  | 2           | 2.25        | 4.5         | Longer sessions         |
| 3     | TTh     | 2           | 2           | 4.0         | Common alternative      |
| 5     | MTWTHF  | 5           | 1.5         | 7.5         | Math/Language immersion |
| 1     | Single  | 1           | 1.5         | 1.5         | PE, Seminars            |

**Lab Courses** (typically Units + extra lab time):

```csharp
// 3-unit CS course with lab: 3 hours lecture + 3 hours lab = 6 hours total
Subjects.Add(new Subject("CS201", "CS201", "Data Structures", SubjectType.ComputerScience,
    3, 2, 2, DayPattern.TTh)  // 2 days × 2 hours = 4 hours (lecture + lab combined)
{ RequiresLab = true });
```

**When Creating Subjects:**

1. Calculate total hours: `Units × 1.5` (or `Units × 2` for lab courses)
2. Choose day pattern based on subject needs
3. Set `HoursPerDay = TotalHours / DaysPerWeek`
4. Ensure timeslots exist matching `HoursPerDay` in `InitializeTimeSlots()`

**Example Calculation:**

```csharp
// 3-unit subject, prefer Tuesday-Thursday
var units = 3;
var daysPerWeek = 2;  // TTh
var totalHours = units * 1.5;  // = 4.5 hours
var hoursPerDay = totalHours / daysPerWeek;  // = 2.25 hours per session

new Subject("MATH201", "MATH201", "Calculus II", SubjectType.Mathematics,
    3, 2, 2.25, DayPattern.TTh);
```

## Documentation Files

- **`genetic_algo_guide.md`**: GA fundamentals and terminology
- **`ga_v5_enhanced_scheduling.md`**: Architecture evolution and DaysPerWeek/HoursPerDay feature details
- **`ga_v3_program_section_Claude_Sonnet_45.md`**: Program-Section-Subject hierarchy implementation guide
- **`College-Course-Scheduling-Algo-Notes/`**: Detailed explanations of selection, crossover, and mutation strategies

When adding subjects to `ScheduleData.cs`, always specify: `DaysPerWeek`, `HoursPerDay`, `PreferredDayPattern`, and populate `ProfessorIds` list with qualified professor IDs.
