# Class Section Validation — Unified Table Plan

## Objective

Consolidate `ClassSectionConflicts` and `ClassSectionEnrollmentEligibilityValidationMessages` into a single table `ClassSectionValidationIssues`. The two validators remain separate domain services, but their outputs are stored in one table via a unified orchestrator.

---

## Architecture

```
┌──────────────────────────────────────────────┐
│              Domain Services                 │
│                                              │
│  ScheduleConflictDetector (instance)         │
│    → Validate(schedules, targetSectionId?)    │
│    → returns IReadOnlyList<ConflictResult>    │
│                                              │
│  ClassSectionEnrollmentEligibilityValidator   │
│    → Validate(classSection, offerings)         │
│    → returns ValidationContext                │
└──────────┬───────────────────────┬───────────┘
           │                       │
           ▼                       ▼
┌──────────────────────────────────────────────┐
│        ClassSectionValidationOrchestrator     │  ← NEW application service
│                                              │
│  1. Loads aggregates + schedule projections   │
│  2. Calls both validators                     │
│  3. Maps both to List<ClassSectionValidationIssue>  │
│  4. Calls repo.ReplaceAllForSectionAsync()    │
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│   IClassSectionValidationIssueRepository      │  ← CONSOLIDATED repository
│     ReplaceAllForSectionAsync(sectionId, issues) │
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│        ClassSectionValidationIssues           │  ← SINGLE table
│  (replaces ClassSectionConflicts +            │
│   ClassSectionEnrollmentEligibilityMessages)  │
└──────────────────────────────────────────────┘
```

---

## Phase 1: Enum Expansion

Add 6 new entries to `ClassScheduleConflictTypeEnum` in `Enrollify.Core/Constants/`:

```csharp
// DataIntegrity tier (new)
public static readonly ClassScheduleConflictTypeEnum ADVISER_NOT_ASSIGNED  = new("ADVISER_NOT_ASSIGNED", 18, DataIntegrity);
public static readonly ClassScheduleConflictTypeEnum TEACHER_NOT_ASSIGNED  = new("TEACHER_NOT_ASSIGNED", 19, DataIntegrity);
public static readonly ClassScheduleConflictTypeEnum ROOM_NOT_ASSIGNED     = new("ROOM_NOT_ASSIGNED",    20, DataIntegrity);

// Informational tier (new)
public static readonly ClassScheduleConflictTypeEnum DAYS_PER_WEEK_DEFAULT   = new("DAYS_PER_WEEK_DEFAULT",   21, Informational);
public static readonly ClassScheduleConflictTypeEnum HOURS_PER_DAY_DEFAULT   = new("HOURS_PER_DAY_DEFAULT",   22, Informational);
public static readonly ClassScheduleConflictTypeEnum MAX_STUDENTS_AT_DEFAULT = new("MAX_STUDENTS_AT_DEFAULT", 23, Informational);
```

These replace the string codes currently hardcoded in `ClassSectionEnrollmentEligibilityValidator`:

| Current string code | New enum entry |
|---|---|
| `CLASS_SECTION_ADVISER_REQUIRED` | `ADVISER_NOT_ASSIGNED` |
| `CLASS_SECTION_SUBJECT_OFFERINGS_MISSING` | `NO_OFFERINGS` (already exists) |
| `SUBJECT_OFFERING_TEACHER_REQUIRED` | `TEACHER_NOT_ASSIGNED` |
| `SUBJECT_OFFERING_ROOM_REQUIRED` | `ROOM_NOT_ASSIGNED` |
| `SUBJECT_OFFERING_DAYS_PER_WEEK_DEFAULT_VALUE` | `DAYS_PER_WEEK_DEFAULT` |
| `SUBJECT_OFFERING_HOURS_PER_DAY_DEFAULT_VALUE` | `HOURS_PER_DAY_DEFAULT` |
| `SUBJECT_OFFERING_MAX_NUMBER_OF_STUDENTS_DEFAULT_VALUE` | `MAX_STUDENTS_AT_DEFAULT` |
| `SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES` | `SCHEDULE_COUNT_MISMATCH` (already exists) |
| `SUBJECT_OFFERING_INVALID_CLASS_SCHEDULES` | `HOURS_MISMATCH` (already exists) |

---

## Phase 2: SQL Migration

### New script: `Script0019_RenameClassSectionConflicts.sql`

Renames existing `ClassSectionConflicts` table and alters columns for unified schema.

```sql
-- Step 1: Drop old eligibility messages table
DROP TABLE IF EXISTS ClassSectionEnrollmentEligibilityValidationMessages;

-- Step 2: Alter existing ClassSectionConflicts for unified schema
ALTER TABLE ClassSectionConflicts
  ALTER COLUMN OfferingId INT NULL;

ALTER TABLE ClassSectionConflicts
  ALTER COLUMN DayOfWeek CHAR(3) NULL;

-- Step 3: Rename table
EXEC sp_rename 'ClassSectionConflicts', 'ClassSectionValidationIssues';
```

> Note: `ALTER COLUMN` to nullable requires existing data to have no NULL violations. If existing rows have `NOT NULL` values, this works in-place. The `ConflictType` column already stores string codes — no schema change needed there.

### Updated indexes

```sql
-- Drop old indexes
DROP INDEX IX_ClassSectionConflicts_ClassSectionId ON ClassSectionValidationIssues;
DROP INDEX IX_ClassSectionConflicts_OfferingId ON ClassSectionValidationIssues;
DROP INDEX IX_ClassSectionConflicts_College_Term_Severity ON ClassSectionValidationIssues;

-- Create new indexes (rename only)
CREATE NONCLUSTERED INDEX IX_ClassSectionValidationIssues_ClassSectionId
  ON ClassSectionValidationIssues (ClassSectionId);

CREATE NONCLUSTERED INDEX IX_ClassSectionValidationIssues_OfferingId
  ON ClassSectionValidationIssues (OfferingId)
  WHERE OfferingId IS NOT NULL;  -- filtered index for nullable column

CREATE NONCLUSTERED INDEX IX_ClassSectionValidationIssues_College_Term_Severity
  ON ClassSectionValidationIssues (CollegeId, AcademicTermId, Severity)
  INCLUDE (ClassSectionId, OfferingId, ConflictType, StartTime, EndTime, ComputedAt);
```

> The `OfferingId` index uses a filtered index (`WHERE OfferingId IS NOT NULL`) because SQL Server handles nullable column indexes more efficiently this way.

---

## Phase 3: Domain Entity

### New entity: `ClassSectionValidationIssue`

**File:** `Enrollify.Core/Aggregates/ClassSectionValidationIssueAggregate/ClassSectionValidationIssue.cs`

```csharp
[ValueObject<int>]
public readonly partial struct ClassSectionValidationIssueId { }

public class ClassSectionValidationIssue : EntityBase<ClassSectionValidationIssue, ClassSectionValidationIssueId>, IAggregateRoot
{
  private ClassSectionValidationIssue() { }

  // Constructor for conflict-origin issues (has times, offerings, affected offerings)
  internal ClassSectionValidationIssue(
    CollegeId collegeId,
    CourseId courseId,
    AcademicTermId academicTermId,
    ClassSectionId classSectionId,
    ClassSectionSubjectOfferingId offeringId,
    ConflictResult conflictResult)
  {
    CollegeId = Guard.Against.Null(collegeId);
    CourseId = Guard.Against.Null(courseId);
    AcademicTermId = Guard.Against.Null(academicTermId);
    ClassSectionId = Guard.Against.Null(classSectionId);
    OfferingId = Guard.Against.Null(offeringId);
    ConflictType = Guard.Against.Null(conflictResult.Type);
    Severity = Guard.Against.Null(conflictResult.Severity);
    Message = Guard.Against.NullOrEmpty(conflictResult.Message);
    DayOfWeek = Guard.Against.Null(conflictResult.DayOfWeek);
    StartTime = conflictResult.StartTime;
    EndTime = conflictResult.EndTime;
    AffectedOfferings = conflictResult.AffectedOfferings ?? [];
    ComputedAt = DateTimeOffset.UtcNow;
  }

  // Constructor for eligibility-origin issues (may lack offering/time)
  internal ClassSectionValidationIssue(
    CollegeId collegeId,
    CourseId courseId,
    AcademicTermId academicTermId,
    ClassSectionId classSectionId,
    ClassSectionSubjectOfferingId? offeringId,
    ClassScheduleConflictTypeEnum conflictType,
    DomainValidationErrorSeverityEnum severity,
    string message)
  {
    CollegeId = Guard.Against.Null(collegeId);
    CourseId = Guard.Against.Null(courseId);
    AcademicTermId = Guard.Against.Null(academicTermId);
    ClassSectionId = Guard.Against.Null(classSectionId);
    OfferingId = offeringId;  // nullable
    ConflictType = Guard.Against.Null(conflictType);
    Severity = Guard.Against.Null(severity);
    Message = Guard.Against.NullOrEmpty(message);
    DayOfWeek = null;
    StartTime = null;
    EndTime = null;
    AffectedOfferings = [];
    ComputedAt = DateTimeOffset.UtcNow;
  }

  public CollegeId CollegeId { get; private set; }
  public CourseId CourseId { get; private set; }
  public AcademicTermId AcademicTermId { get; private set; }
  public ClassSectionId ClassSectionId { get; private set; }
  public ClassSectionSubjectOfferingId? OfferingId { get; private set; }
  public ClassScheduleConflictTypeEnum ConflictType { get; private set; }
  public DomainValidationErrorSeverityEnum Severity { get; private set; }
  public string Message { get; private set; } = null!;
  public DayOfWeekEnum? DayOfWeek { get; private set; }
  public TimeOnly? StartTime { get; private set; }
  public TimeOnly? EndTime { get; private set; }
  public DateTimeOffset ComputedAt { get; private set; }
  public List<AffectedOffering> AffectedOfferings { get; private set; } = [];
}
```

**Folder structure:**
```
Enrollify.Core/Aggregates/
  ClassSectionValidationIssueAggregate/
    ClassSectionValidationIssue.cs
    ClassSectionValidationIssueId.cs
    Models/
      (reuse AffectedOffering.cs, ConflictResult.cs from existing ClassSectionConflictAggregate)
```

### Remove these files:
- `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSectionEnrollmentEligibilityValidationMessage.cs`
- `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSectionEnrollmentEligibilityValidationMessageId.cs`
- `Enrollify.Core/Aggregates/ClassSectionConflictAggregate/ClassSectionConflict.cs`
- `Enrollify.Core/Aggregates/ClassSectionConflictAggregate/ClassSectionConflictId.cs`

> Keep `AffectedOffering.cs` and `ConflictResult.cs` in their current location — they're models used by the domain service, not entities.

---

## Phase 4: EF Core Configuration

### New config: `ClassSectionValidationIssueConfiguration`

**File:** `Enrollify.Infrastructure/Data/Config/AggregateConfigs/ClassSectionValidationIssueConfigs/ClassSectionValidationIssueConfiguration.cs`

```csharp
public class ClassSectionValidationIssueConfiguration : IEntityTypeConfiguration<ClassSectionValidationIssue>
{
  public void Configure(EntityTypeBuilder<ClassSectionValidationIssue> builder)
  {
    builder.ToTable("ClassSectionValidationIssues");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.OfferingId).IsRequired(false);
    builder.Property(x => x.DayOfWeek).IsRequired(false)
           .HasConversion<DayOfWeekEnumConverter>();
    builder.Property(x => x.StartTime).IsRequired(false);
    builder.Property(x => x.EndTime).IsRequired(false);

    builder.Property(x => x.ConflictType)
           .HasConversion<ClassScheduleConflictTypeEnumConverter>()
           .IsRequired();

    builder.Property(x => x.Severity)
           .HasConversion<DomainValidationErrorSeverityEnumConverter>()
           .IsRequired();

    builder.OwnsOne(x => x.AffectedOfferings, nav =>
    {
      nav.ToJson("AffectedOfferingsJson");
    });

    builder.HasOne<ClassSection>()
           .WithMany()
           .HasForeignKey(x => x.ClassSectionId)
           .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne<ClassSectionSubjectOffering>()
           .WithMany()
           .HasForeignKey(x => x.OfferingId)
           .OnDelete(DeleteBehavior.NoAction);  // required for nullable FK
  }
}
```

### Remove these files:
- `ClassSectionConflictConfiguration.cs`
- `ClassSectionConflictVogenEfCoreConverters.cs`
- `ClassSectionEnrollmentEligibilityValidationMessageConfiguration.cs`

---

## Phase 5: Consolidated Repository

### New interface: `IClassSectionValidationIssueRepository`

**File:** `Enrollify.Application/Features/ClassSectionScheduling/Repositories/IClassSectionValidationIssueRepository.cs`

```csharp
public interface IClassSectionValidationIssueRepository
{
  Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> issues,
    CancellationToken cancellationToken = default);
}
```

### New implementation: `ClassSectionValidationIssueRepository`

**File:** `Enrollify.Infrastructure/Repositories/ClassSectionValidationIssueRepository.cs`

```csharp
public class ClassSectionValidationIssueRepository : IClassSectionValidationIssueRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly ILogger<ClassSectionValidationIssueRepository> _logger;

  public ClassSectionValidationIssueRepository(
    EnrollifyDbContext dbContext,
    ILogger<ClassSectionValidationIssueRepository> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionValidationIssue> issues,
    CancellationToken cancellationToken = default)
  {
    await _dbContext.ClassSectionValidationIssues
      .Where(i => i.ClassSectionId == classSectionId)
      .ExecuteDeleteAsync(cancellationToken);

    var freshIssues = issues.ToList();
    if (freshIssues.Count > 0)
      await _dbContext.ClassSectionValidationIssues
        .AddRangeAsync(freshIssues, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogDebug(
      "Replaced validation issues for ClassSection {ClassSectionId}: removed {RemovedCount}, inserted {InsertedCount}",
      classSectionId.Value, deletedRows, freshIssues.Count);
  }
}
```

### Remove these interface + implementation pairs:
| Interface | Implementation |
|---|---|
| `IClassSectionConflictRepository` | `ClassSectionConflictRepository` |
| `IClassSectionEligibilityValidationMessageRepository` | `ClassSectionEligibilityValidationMessageRepository` |

---

## Phase 6: DbContext Update

In `EnrollifyDbContext.cs`:

```csharp
// Remove
public DbSet<ClassSectionEnrollmentEligibilityValidationMessage> ClassSectionEnrollmentEligibilityValidationMessages => Set<ClassSectionEnrollmentEligibilityValidationMessage>();
public DbSet<ClassSectionConflict> ClassSectionConflicts => Set<ClassSectionConflict>();

// Add
public DbSet<ClassSectionValidationIssue> ClassSectionValidationIssues => Set<ClassSectionValidationIssue>();
```

---

## Phase 7: API Alignment of Domain Services

### `ScheduleConflictDetector`

File: `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDetector.cs`

Changes:
- Rename `DetectConflicts(...)` → `Validate(...)`
- Change return type from `List<ConflictResult>` → `IReadOnlyList<ConflictResult>`

```csharp
public IReadOnlyList<ConflictResult> Validate(
    IEnumerable<ScheduleConflictProjectionDto> schedules,
    ClassSectionId? targetSectionId = null)
```

### `ClassSectionEnrollmentEligibilityValidator`

File: `Enrollify.Core/Services/ClassSectionOpenForEnrollmentEligibilityValidation/ClassSectionEnrollmentEligibilityValidator.cs`

Changes:
- Remove `static` — make it an instance class
- Replace hardcoded string error codes with `ClassScheduleConflictTypeEnum.Name` references
- Keep the return type as `ClassSectionOpenForEnrollmentEligibilityValidationContext` (the orchestrator consumes it)

Before:
```csharp
context.AddClassSectionValidationMessage(
    DomainValidationErrorSeverityEnum.Error,
    "CLASS_SECTION_ADVISER_REQUIRED",
    "Class section must have an adviser assigned.");
```

After:
```csharp
context.AddClassSectionValidationMessage(
    DomainValidationErrorSeverityEnum.Error,
    ClassScheduleConflictTypeEnum.ADVISER_NOT_ASSIGNED.Name,
    "Class section must have an adviser assigned.");
```

Also rename the namespace folder:
- From: `Services/ClassSectionOpenForEnrollmentEligibilityValidation/`
- To: `Services/ClassSectionEligibilityValidation/`

---

## Phase 8: DTO Consolidation

### New DTO: `ClassSectionValidationIssueDto`

**File:** `Enrollify.Application/Features/ClassSectionScheduling/DTOs/ClassSectionValidationIssueDto.cs`

```csharp
namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public record ClassSectionValidationIssueDto
{
  public string? Id { get; init; }
  public string Type { get; init; } = string.Empty;       // ClassScheduleConflictTypeEnum.Name
  public string Severity { get; init; } = string.Empty;    // "Info" | "Warning" | "Error"
  public string Tier { get; init; } = string.Empty;        // "Hard" | "Soft" | "DataIntegrity" | "Informational"
  public string Message { get; init; } = string.Empty;
  public string? DayOfWeek { get; init; }
  public string? StartTime { get; init; }
  public string? EndTime { get; init; }
  public List<AffectedOfferingDto>? AffectedOfferings { get; init; }
}
```

### Remove these DTOs:
- `ClassSectionEnrollmentEligibilityValidationMessageDto.cs` (the entire hierarchy: base + derived + wrapper)

### Simplify `ClassSectionSubjectOfferingDto`  and `ClassSectionDetailDto`

`ClassSectionSubjectOfferingDto`:
- Remove `Conflicts` list (was `List<ConflictResultDto>`)
- Add `List<ClassSectionValidationIssueDto> Issues`

`ClassSectionDetailDto`:
- Remove `ValidationMessages` property (was `ClassSectionEnrollmentEligibilityValidationMessagesDto`)
- Add `IReadOnlyList<ClassSectionValidationIssueDto> Issues`
- `UnresolvedErrorsCount` stays (computed from issues with `Severity == Error`)
- `IsEligibleForOpenEnrollment` stays (true when no Error-severity issues exist)

---

## Phase 9: `ClassSectionValidationOrchestrator`

**File:** `Enrollify.Application/Features/ClassSectionScheduling/Services/ClassSectionValidationOrchestrator.cs`

```csharp
public class ClassSectionValidationOrchestrator
{
  private readonly IReadRepository<ClassSection> _sectionRepo;
  private readonly IReadRepository<ClassSectionSubjectOffering> _offeringRepo;
  private readonly IClassSectionSubjectOfferingScheduleConflictRepository _conflictRepo;
  private readonly ScheduleConflictDetector _conflictDetector;
  private readonly ClassSectionEnrollmentEligibilityValidator _eligibilityValidator;
  private readonly IClassSectionValidationIssueRepository _issueRepo;

  public ClassSectionValidationOrchestrator(
    IReadRepository<ClassSection> sectionRepo,
    IReadRepository<ClassSectionSubjectOffering> offeringRepo,
    IClassSectionSubjectOfferingScheduleConflictRepository conflictRepo,
    ScheduleConflictDetector conflictDetector,
    ClassSectionEnrollmentEligibilityValidator eligibilityValidator,
    IClassSectionValidationIssueRepository issueRepo)
  {
    _sectionRepo = sectionRepo;
    _offeringRepo = offeringRepo;
    _conflictRepo = conflictRepo;
    _conflictDetector = conflictDetector;
    _eligibilityValidator = eligibilityValidator;
    _issueRepo = issueRepo;
  }

  public async Task<ClassSectionValidationResult> ValidateSectionAsync(
    ClassSectionId sectionId,
    CancellationToken ct = default)
  {
    // 1. Load aggregates
    var section = await _sectionRepo.SingleAsync(
      new ClassSectionByIdSpec(sectionId), ct);
    var offerings = await _offeringRepo.ListAsync(
      new OfferingsBySectionSpec(sectionId), ct);

    // 2. Run eligibility validation
    var eligibilityContext = _eligibilityValidator.Validate(section, offerings);

    // 3. Load schedules for conflict detection
    var sectionSchedules = await _conflictRepo
      .GetSectionSchedulesForConflictDetectionAsync(sectionId, ct);

    var teacherIds = offerings.Where(o => o.TeacherId != null).Select(o => o.TeacherId!);
    var roomIds = offerings.Where(o => o.RoomId != null).Select(o => o.RoomId!);

    var relatedSchedules = teacherIds.Any() || roomIds.Any()
      ? await _conflictRepo.GetRelatedSchedulesForConflictDetectionAsync(
          teacherIds, roomIds, section.AcademicTermId, sectionId, ct)
      : [];

    // 4. Run conflict detection
    var conflictResults = _conflictDetector.Validate(
      sectionSchedules.Concat(relatedSchedules), sectionId);

    // 5. Map both to unified entity list
    var issues = new List<ClassSectionValidationIssue>();
    issues.AddRange(MapEligibilityToIssues(section, eligibilityContext));
    issues.AddRange(MapConflictsToIssues(section, conflictResults));

    // 6. Persist atomically
    await _issueRepo.ReplaceAllForSectionAsync(sectionId, issues, ct);

    // 7. Return result
    return new ClassSectionValidationResult(sectionId, issues);
  }

  // For queries that don't need to re-persist (read path)
  public ClassSectionValidationResult BuildResultFromStoredIssues(
    ClassSectionId sectionId,
    IEnumerable<ClassSectionValidationIssue> storedIssues)
  {
    return new ClassSectionValidationResult(sectionId, storedIssues.ToList());
  }

  private List<ClassSectionValidationIssue> MapEligibilityToIssues(
    ClassSection section,
    ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    var issues = new List<ClassSectionValidationIssue>();

    // Section-level messages
    foreach (var msg in context.ClassSectionValidationMessages)
    {
      issues.Add(new ClassSectionValidationIssue(
        section.CollegeId, section.CourseId, section.AcademicTermId,
        section.Id, null,  // no offering
        ClassScheduleConflictTypeEnum.FromName(msg.Code),
        msg.Severity, msg.Message));
    }

    // Offering-level messages
    foreach (var (offeringId, messages) in context.OfferingValidationMessages)
    {
      foreach (var msg in messages)
      {
        issues.Add(new ClassSectionValidationIssue(
          section.CollegeId, section.CourseId, section.AcademicTermId,
          section.Id, offeringId,
          ClassScheduleConflictTypeEnum.FromName(msg.Code),
          msg.Severity, msg.Message));
      }
    }

    return issues;
  }

  private List<ClassSectionValidationIssue> MapConflictsToIssues(
    ClassSection section,
    IReadOnlyList<ConflictResult> conflictResults)
  {
    return conflictResults.Select(cr => new ClassSectionValidationIssue(
      section.CollegeId, section.CourseId, section.AcademicTermId,
      section.Id,
      cr.AffectedOfferings?.FirstOrDefault()?.Id
        ?? throw new InvalidOperationException("ConflictResult must have at least one affected offering"),
      cr)).ToList();
  }
}
```

### New result type: `ClassSectionValidationResult`

**File:** `Enrollify.Application/Features/ClassSectionScheduling/DTOs/ClassSectionValidationResult.cs`

```csharp
public class ClassSectionValidationResult
{
  public ClassSectionId SectionId { get; }
  public IReadOnlyList<ClassSectionValidationIssue> Issues { get; }
  public bool IsEligible => Issues.All(i => i.Severity != DomainValidationErrorSeverityEnum.Error);
  public int UnresolvedErrorsCount => Issues.Count(i => i.Severity == DomainValidationErrorSeverityEnum.Error);

  public ClassSectionValidationResult(
    ClassSectionId sectionId,
    IReadOnlyList<ClassSectionValidationIssue> issues)
  {
    SectionId = sectionId;
    Issues = issues;
  }
}
```

---

## Phase 10: Remove `ConflictDetectionHelper`

**Remove file:** `Enrollify.Application/Features/ClassSectionScheduling/Services/ConflictDetectionHelper.cs`

Its responsibilities are absorbed by `ClassSectionValidationOrchestrator`:
- Loading related schedules → orchestrator step 3
- Calling `_conflictDetector.Validate()` → orchestrator step 4
- Mapping conflicts to DTOs → orchestrator step 5 + API consuming code

---

## Phase 11: Consumer Updates

### Event handler: `ClassSectionCreatedEventHandler`

Before:
```csharp
var context = ClassSectionEnrollmentEligibilityValidator.Validate(section, offerings);
await _messageRepo.ReplaceAllForSectionAsync(section.Id, MapToEntities(context));
```

After:
```csharp
var result = await _orchestrator.ValidateSectionAsync(section.Id, ct);
// result.Issues → map to DTOs if needed for response
```

### Event handler: `ClassSectionEligibilityRecomputeRequestedEventHandler`

Same pattern — replace static validator + message repo call with orchestrator.

### Command handler: `OpenClassSectionForEnrollment.Handler`

Before:
```csharp
var context = ClassSectionEnrollmentEligibilityValidator.Validate(section, offerings);
if (!context.IsEligible) return Result.Invalid(...);
```

After:
```csharp
var result = await _orchestrator.ValidateSectionAsync(section.Id, ct);
if (!result.IsEligible) return Result.Invalid(...);
```

### Query handler: `GetClassSectionByIdQueryHandler`

Before:
```csharp
var messages = await _messageRepo.ListAsync(...);
var conflicts = await _conflictHelper.DetectConflictsForSectionAsync(...);
```

After:
```csharp
var issues = await _issueRepo.ListAsync(...);  // or via orchestrator
var result = _orchestrator.BuildResultFromStoredIssues(sectionId, issues);
```

Or simpler — load issues directly from the repo:
```csharp
var issues = await _dbContext.ClassSectionValidationIssues
  .Where(i => i.ClassSectionId == sectionId)
  .ToListAsync();
```

---

## Phase 12: DI Registration

In `InfrastructureServiceExtensions.cs`:

```csharp
// Remove
services.AddScoped<IClassSectionConflictRepository, ClassSectionConflictRepository>();
services.AddScoped<IClassSectionEligibilityValidationMessageRepository, ClassSectionEligibilityValidationMessageRepository>();

// Add
services.AddScoped<IClassSectionValidationIssueRepository, ClassSectionValidationIssueRepository>();
services.AddScoped<ClassSectionValidationOrchestrator>();

// Keep (already registered)
services.AddScoped<ScheduleConflictDetector>();

// Add
services.AddScoped<ClassSectionEnrollmentEligibilityValidator>();
```

---

## Complete File Change List

### Remove (14 files)

| File | Reason |
|---|---|
| `Core/Aggregates/ClassSectionConflictAggregate/ClassSectionConflict.cs` | Replaced by `ClassSectionValidationIssue` |
| `Core/Aggregates/ClassSectionConflictAggregate/ClassSectionConflictId.cs` | Replaced by `ClassSectionValidationIssueId` |
| `Core/Aggregates/ClassSectionAggregate/ClassSectionEnrollmentEligibilityValidationMessage.cs` | Replaced by `ClassSectionValidationIssue` |
| `Core/Aggregates/ClassSectionAggregate/ClassSectionEnrollmentEligibilityValidationMessageId.cs` | Replaced by `ClassSectionValidationIssueId` |
| `Application/.../Repositories/IClassSectionConflictRepository.cs` | Consolidated into `IClassSectionValidationIssueRepository` |
| `Application/.../Repositories/IClassSectionEligibilityValidationMessageRepository.cs` | Same |
| `Infrastructure/Repositories/ClassSectionConflictRepository.cs` | Consolidated into `ClassSectionValidationIssueRepository` |
| `Infrastructure/Repositories/ClassSectionEligibilityValidationMessageRepository.cs` | Same |
| `Infrastructure/.../ClassSectionConflictConfiguration.cs` | Replaced by `ClassSectionValidationIssueConfiguration` |
| `Infrastructure/.../ClassSectionConflictVogenEfCoreConverters.cs` | No longer needed (or consolidated) |
| `Infrastructure/.../ClassSectionEnrollmentEligibilityValidationMessageConfiguration.cs` | Replaced |
| `Application/.../Services/ConflictDetectionHelper.cs` | Absorbed by orchestrator |
| `Application/.../DTOs/ClassSectionEnrollmentEligibilityValidationMessageDto.cs` | Replaced by `ClassSectionValidationIssueDto` |
| `DatabaseMigration/Scripts/Script0019__ClassSectionEnrollmentEligibilityValidationMessages.sql` | Table dropped |

### Modify (7 files)

| File | Change |
|---|---|
| `Core/Constants/ClassScheduleConflictTypeEnum.cs` | Add 6 new entries |
| `Core/Services/ScheduleConflictDetection/ScheduleConflictDetector.cs` | Rename `DetectConflicts` → `Validate`; return `IReadOnlyList<ConflictResult>` |
| `Core/Services/ClassSectionEligibilityValidation/ClassSectionEnrollmentEligibilityValidator.cs` | Remove `static`; use enum codes; move to new folder |
| `Infrastructure/Data/EnrollifyDbContext.cs` | Replace both DbSets with `DbSet<ClassSectionValidationIssue>` |
| `Infrastructure/InfrastructureServiceExtensions.cs` | Update DI registrations |
| `Application/.../DTOs/ClassSectionSubjectOfferingDto.cs` | Replace `List<ConflictResultDto> Conflicts` with `List<ClassSectionValidationIssueDto> Issues` |
| `Application/.../DTOs/ClassSectionDetailDto.cs` | Remove `ValidationMessages`; add `IReadOnlyList<ClassSectionValidationIssueDto> Issues` |

### Add (5 files)

| File | Purpose |
|---|---|
| `Core/Aggregates/ClassSectionValidationIssueAggregate/ClassSectionValidationIssue.cs` | New unified entity |
| `Core/Aggregates/ClassSectionValidationIssueAggregate/ClassSectionValidationIssueId.cs` | Vogen value object |
| `Infrastructure/.../ClassSectionValidationIssueConfiguration.cs` | EF Core config |
| `Application/.../Repositories/IClassSectionValidationIssueRepository.cs` | Consolidated repository interface |
| `Infrastructure/Repositories/ClassSectionValidationIssueRepository.cs` | Consolidated repository impl |
| `Application/.../Services/ClassSectionValidationOrchestrator.cs` | Unified orchestration service |
| `Application/.../DTOs/ClassSectionValidationIssueDto.cs` | Unified API DTO |
| `DatabaseMigration/Scripts/Script0019_RenameClassSectionConflicts.sql` | Schema migration |

### Update consumers (4 files)

| File | Change |
|---|---|
| `Application/.../EventHandlers/ClassSectionCreatedEventHandler.cs` | Use orchestrator instead of static validator + message repo |
| `Application/.../EventHandlers/ClassSectionEligibilityRecomputeRequestedEventHandler.cs` | Same |
| `Application/.../Commands/.../OpenClassSectionForEnrollment.cs` | Use orchestrator instead of static validator |
| `Application/.../Queries/.../GetClassSectionByIdQuery.cs` | Use new issue repo (or orchestrator) instead of separate message + conflict loading |

---

## Summary

| Aspect | Before | After |
|---|---|---|
| Tables | 2 (`ClassSectionConflicts` + `ClassSectionEnrollmentEligibilityValidationMessages`) | 1 (`ClassSectionValidationIssues`) |
| Domain entities | 2 (`ClassSectionConflict` + `ClassSectionEnrollmentEligibilityValidationMessage`) | 1 (`ClassSectionValidationIssue`) |
| Repositories | 2 pairs (interface + impl) | 1 pair |
| Validators | Separate (1 static, 1 instance) | **Separate** (both instance, aligned APIs) |
| Entry point | Callers manually orchestrated | `ClassSectionValidationOrchestrator` coordinates both |
| DTOs | `ConflictResultDto` + 3 eligibility message DTOs | Single `ClassSectionValidationIssueDto` |
| DI registrations | 4 scoped services | 2 scoped services + 1 orchestrator |
| Migration scripts | 2 scripts | 1 consolidated script |
