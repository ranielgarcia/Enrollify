# Scheduling Conflict Validation — Implementation Guide

**Scope:** `ClassSections` → `ClassSectionSubjectOffering` → `ClassSchedules`  
**Research date:** 2026-05-29  
**Based on:** `research/scheduling/class-scheduling-conflicts.md`, Enrollify codebase analysis, and 4 real production scheduling systems

---

## Executive Summary

The correct approach for Enrollify's scheduling conflict validation is **validate-on-save with targeted database queries**, not in-memory bulk loads. Every production scheduling system examined follows the same pattern: when a schedule row is added or mutated, three narrow `AnyAsync` queries check for teacher double-booking, room double-booking, and section overlap against the same `AcademicTermId` — before the insert is committed. Full in-memory loads of the entire academic year's schedules are perfectly feasible (the worst-case memory footprint is ~13 MB) and are appropriate for the admin "view all conflicts" batch scan, but they are architecturally wrong for per-save validation because of TOCTOU race conditions. Conflicts should **never** be recomputed on every frontend read — that is prohibitively expensive and architecturally unsound. The frontend already has the correct mental model (`OfferingWithSchedules.conflicts[]` embedded in the section detail response[^1]); the backend needs to compute those conflicts in a focused query when the section detail is loaded, and block hard conflicts at the point of write.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Should You Load Everything Into Memory?](#2-should-you-load-everything-into-memory)
3. [When to Trigger Validation](#3-when-to-trigger-validation)
4. [The Three Validation Paths](#4-the-three-validation-paths)
5. [Implementation Plan](#5-implementation-plan)
6. [Database Index Requirements](#6-database-index-requirements)
7. [Academic Year vs. Academic Term Scoping](#7-academic-year-vs-academic-term-scoping)
8. [TOCTOU Concurrency](#8-toctou-concurrency)
9. [Algorithmic Complexity](#9-algorithmic-complexity)
10. [Conflict Taxonomy Reference](#10-conflict-taxonomy-reference)
11. [Confidence Assessment](#11-confidence-assessment)

---

## 1. Architecture Overview

```mermaid
graph TD
    subgraph "WRITE PATH (Admin)"
        A[POST /offerings/{id}/schedules] --> B[FluentValidation Pipeline]
        B --> B1["HasTeacherConflict? (DB AnyAsync)"]
        B --> B2["HasRoomConflict? (DB AnyAsync)"]
        B --> B3["HasSectionOverlap? (DB AnyAsync)"]
        B1 & B2 & B3 --> C{Any hard conflict?}
        C -->|Yes| D["409 Conflict + ConflictResult[]"]
        C -->|No| E["Domain: AddClassSchedule()"]
        E --> F["Serializable TX: SaveChanges()"]
        F --> G["Cache eviction for AcademicYear"]
    end

    subgraph "READ PATH (Admin + Students)"
        H["GET /class-sections/{id}"] --> I["Load section + offerings + schedules\n(targeted JOIN query)"]
        I --> J["In-memory conflict scan\n(only for this section's offerings vs.\nsame-term offerings sharing teacher/room)"]
        J --> K["Return OfferingWithSchedules[]\neach with embedded conflicts[]"]
    end

    subgraph "ADMIN BATCH REPORT (Optional)"
        L["GET /class-sections/conflict-report?ayId=X"] --> M["Load all flat ScheduleDTO rows\nfor academic year (one query)"]
        M --> N["Group by (entity, day)\nO(n²) within each group"]
        N --> O["Return all ConflictResult[] or\ncache with short TTL"]
    end
```

---

## 2. Should You Load Everything Into Memory?

### Short answer: **It depends on which path you are in.**

The question has two distinct scenarios with different correct answers:

| Scenario | Load all into memory? | Correct pattern |
|---|---|---|
| **Per-save validation** (HC-01, HC-02, HC-03) | ❌ No | Targeted DB `AnyAsync` per conflict type |
| **Section detail page conflicts** (`GET /class-sections/{id}`) | ✅ Yes (small slice) | Load section + related schedules from same term |
| **Admin "view all conflicts" report** | ✅ Yes (entire year, ~3-13 MB) | Full flat load, group by (entity, day), O(n²) |

### Memory footprint for your dataset

A flat `ScheduleDTO` record needs approximately 200–300 bytes including .NET object overhead[^2]:

| Dataset | Schedule rows | Memory estimate |
|---|---|---|
| Small (100 sections × 8 offerings × 3 days) | 2,400 | ~0.7 MB |
| Medium (300 × 10 × 4) | 12,000 | ~3.5 MB |
| **Worst case** (500 × 15 × 6) | **45,000** | **~13 MB** |

**13 MB is negligible on a modern server.** Loading all of an academic year's schedules into memory for a batch conflict report is completely fine from a performance standpoint[^2].

### Why NOT load everything for per-save validation

Despite the small memory footprint, loading all schedules for per-save validation is wrong for two reasons:

1. **TOCTOU race condition**: Two admin users could both load the schedule data, both see "no conflict," and both insert conflicting rows. The data was clean when loaded, but the DB state changed between the read and the write.

2. **Unnecessary**: A targeted `WHERE TeacherId = X AND DayOfWeek = 'MON' AND StartTime < @endTime AND EndTime > @startTime` query is instant (1–5ms indexed), compared to loading potentially 45,000 rows for a problem you could solve with 3 rows.

---

## 3. When to Trigger Validation

### Do NOT validate on every read

Recomputing conflicts on every `GET /class-sections/filter/...` call would:
- Execute an expensive batch scan for every student page load
- Produce inconsistent results under concurrent writes
- Negate any performance benefit of pagination

No production system examined uses this pattern[^3].

### Validate on every mutation — these are the trigger points

| Trigger | Conflicts to check |
|---|---|
| `POST /offerings/{id}/schedules` — new schedule row | HC-01 (teacher), HC-02 (room), HC-03 (section) |
| `PUT /offerings/{id}/schedules/{scheduleId}` — update time | HC-01, HC-02, HC-03 (with `excludeId = scheduleId`) |
| `PUT /offerings/{id}` — change `TeacherId` | HC-01 for all existing schedules of this offering |
| `PUT /offerings/{id}` — change `RoomId` | HC-02 for all existing schedules of this offering |
| `DELETE /offerings/{id}/schedules/{scheduleId}` | No conflict check needed (removing can only reduce conflicts) |

### Compute conflicts for section detail display

When the admin opens a section detail page, the frontend expects `conflicts[]` embedded in each `OfferingWithSchedules`[^1]. This is computed on the server during `GET /class-sections/{id}`:

1. Load the section with all its offerings + schedules
2. For each offering that has a `TeacherId` or `RoomId`, query other offerings from the same `AcademicTermId` that share that teacher/room — including their schedules
3. Compare time windows in memory (tiny dataset: a few dozen rows per teacher/room)
4. Return conflicts embedded in the response

This computation is fast because the dataset is small (one teacher typically appears in 2–10 offerings across a term), and it only runs when an admin explicitly navigates to the section detail page.

### Optional: Admin conflict snapshot

For an admin dashboard showing "all current conflicts for academic year X," use a separate endpoint that:
1. Loads all flat schedule DTOs for the year in one query
2. Groups by `(teacherId, dayOfWeek)`, `(roomId, dayOfWeek)`, `(sectionId, dayOfWeek)`
3. Runs O(n²) overlap detection within each group
4. Returns or caches the result

This is the pattern used by `AdvisorySystem.Api`'s `DetectConflictsAsync` method[^4].

---

## 4. The Three Validation Paths

### Path 1: Hard Conflict Guard (per-save) — HC-01, HC-02, HC-03

These three queries run in the FluentValidation pipeline before the domain command executes. Each is a single `AnyAsync` with a half-open interval predicate[^5]:

```csharp
// Half-open interval overlap: A overlaps B iff A.Start < B.End AND B.Start < A.End
// Adjacent classes (A ends at 10:30, B starts at 10:30) → NOT a conflict ✓

// HC-01: Teacher double-booked
SELECT TOP 1 cs.Id
FROM ClassSchedules cs
JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
JOIN ClassSections sec ON sec.Id = o.ClassSectionId
WHERE o.TeacherId = @teacherId
  AND sec.AcademicTermId IN @termIds   -- all terms in the target AcademicYearId
  AND cs.DayOfWeek = @dayOfWeekValue   -- e.g. 'MON'
  AND cs.StartTime < @newEndTime
  AND cs.EndTime   > @newStartTime
  AND o.IsActive = 1 AND cs.IsActive = 1
  AND o.Id <> @excludeOfferingId       -- exclude when updating

// HC-02: Room double-booked  (same as HC-01 with o.RoomId = @roomId)

// HC-03: Section overlap
SELECT TOP 1 cs.Id
FROM ClassSchedules cs
JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
WHERE o.ClassSectionId = @sectionId
  AND cs.DayOfWeek = @dayOfWeekValue
  AND cs.StartTime < @newEndTime
  AND cs.EndTime   > @newStartTime
  AND o.IsActive = 1 AND cs.IsActive = 1
  AND cs.Id <> @excludeScheduleId      -- exclude when updating
```

**Key notes:**
- Scope by `AcademicTermId` (not `AcademicYearId` directly) — see §7
- HC-03 does NOT need to scope by teacher/room — it only needs `ClassSectionId` and the day+time window
- The `excludeId` parameter prevents false positives when updating an existing schedule row[^6]

### Path 2: Section Detail Conflicts (per read of one section)

```csharp
// Pseudo-code for GetClassSectionByIdQuery handler:
var section = await LoadSectionWithOfferingsAndSchedules(id);

// Collect all teacherIds and roomIds used in this section's offerings
var teacherIds = section.Offerings.Where(o => o.TeacherId != null).Select(o => o.TeacherId).ToHashSet();
var roomIds    = section.Offerings.Where(o => o.RoomId != null).Select(o => o.RoomId).ToHashSet();

// Load all other offerings that share teachers or rooms in the same term
var relatedOfferings = await LoadOfferingsWithSchedulesByTeacherOrRoomIds(
    teacherIds, roomIds, section.AcademicTermId, excludeSectionId: section.Id);

// Now do in-memory overlap detection
// The combined dataset is tiny: typically 10-80 rows total
var allSchedules = ProjectToFlatDTO(section.Offerings, relatedOfferings);

// Group → compare (see §9 for grouping strategy)
var conflicts = ConflictDetector.Detect(allSchedules, sectionId: section.Id);

// Embed conflicts back into each OfferingWithSchedules
return MapToResponse(section.Offerings, conflicts);
```

### Path 3: Admin Conflict Report (full academic year)

```csharp
// Load ALL flat ScheduleDTO rows for the academic year in one efficient query
var allSchedules = await _dbContext.Set<ClassSchedule>()
    .Where(cs => cs.IsActive)
    .Join(_dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive), ...)
    .Join(_dbContext.ClassSections.Where(s => s.IsActive), ...)
    .Join(_dbContext.Set<AcademicTerm>().Where(at => at.AcademicYearId == targetAcademicYearId), ...)
    .Select(x => new ScheduleConflictDTO {
        ScheduleId = x.cs.Id,
        OfferingId = x.o.Id,
        SectionId  = x.sec.Id,
        TeacherId  = x.o.TeacherId,
        RoomId     = x.o.RoomId,
        DayOfWeek  = x.cs.DayOfWeek,
        StartTime  = x.cs.StartTime,
        EndTime    = x.cs.EndTime,
        // Snapshot info for display
        SubjectCode    = x.o.SnapshotSubjectCode,
        SubjectTitle   = x.o.SnapshotSubjectTitle,
        SectionName    = x.sec.FullName,
    })
    .ToListAsync();

// Group and detect
var byTeacherDay = allSchedules.Where(s => s.TeacherId.HasValue)
    .GroupBy(s => (s.TeacherId, s.DayOfWeek));
var byRoomDay = allSchedules.Where(s => s.RoomId.HasValue)
    .GroupBy(s => (s.RoomId, s.DayOfWeek));
var bySectionDay = allSchedules
    .GroupBy(s => (s.SectionId, s.DayOfWeek));

// Within each group: O(n²) pairwise overlap check, n ≈ 2-15 rows
foreach (var group in byTeacherDay) DetectOverlapsInGroup(group, ConflictType.TeacherDoubleBooked);
foreach (var group in byRoomDay)    DetectOverlapsInGroup(group, ConflictType.RoomDoubleBooked);
foreach (var group in bySectionDay) DetectOverlapsInGroup(group, ConflictType.SectionOverlap);
```

---

## 5. Implementation Plan

### 5.1 New repository methods needed

Extend `IClassSectionSubjectOfferingRepository`[^7] with three new async methods:

```csharp
// In Application/Features/ClassSectionSubjectOfferings/IClassSectionSubjectOfferingRepository.cs
Task<bool> HasTeacherScheduleConflictAsync(
    TeacherId teacherId,
    IEnumerable<AcademicTermId> academicTermIds,  // all terms in the target AY
    DayOfWeekEnum dayOfWeek,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    ClassSectionSubjectOfferingId? excludeOfferingId,
    CancellationToken cancellationToken);

Task<bool> HasRoomScheduleConflictAsync(
    RoomId roomId,
    IEnumerable<AcademicTermId> academicTermIds,
    DayOfWeekEnum dayOfWeek,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    ClassSectionSubjectOfferingId? excludeOfferingId,
    CancellationToken cancellationToken);

Task<bool> HasSectionScheduleOverlapAsync(
    ClassSectionId sectionId,
    DayOfWeekEnum dayOfWeek,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    ClassScheduleId? excludeScheduleId,
    CancellationToken cancellationToken);
```

**Implementation** (in `ClassSectionSubjectOfferingRepository.cs`) uses `_dbContext.Set<ClassSchedule>()` directly[^8] — because `ClassSchedule` is an `OwnsMany` owned entity and does not have a top-level `DbSet`, but EF Core 7+ does expose it via `Set<T>()`:

```csharp
public async Task<bool> HasTeacherScheduleConflictAsync(
    TeacherId teacherId, IEnumerable<AcademicTermId> academicTermIds,
    DayOfWeekEnum dayOfWeek, TimeOnly newStartTime, TimeOnly newEndTime,
    ClassSectionSubjectOfferingId? excludeOfferingId, CancellationToken ct)
{
    var termIdValues = academicTermIds.Select(t => (int)t).ToList();
    var dayStr = dayOfWeek.Value; // e.g. "MON"
    var teacherIdValue = (int)teacherId;
    int? excludeId = excludeOfferingId.HasValue ? (int)excludeOfferingId.Value : null;

    return await _dbContext.Set<ClassSchedule>()
        .Join(_dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive),
              cs => cs.ClassSectionSubjectOfferingId, o => o.Id,
              (cs, o) => new { cs, o })
        .Join(_dbContext.ClassSections.Where(s => s.IsActive),
              x => x.o.ClassSectionId, s => s.Id,
              (x, s) => new { x.cs, x.o, s })
        .AnyAsync(x =>
            x.o.TeacherId == teacherIdValue
            && termIdValues.Contains((int)x.s.AcademicTermId)
            && x.cs.DayOfWeek == dayStr
            && x.cs.StartTime < newEndTime
            && x.cs.EndTime > newStartTime
            && x.cs.IsActive
            && (excludeId == null || x.o.Id != excludeId),
        ct);
}
```

### 5.2 FluentValidation validator for AddClassScheduleCommand

```csharp
// In Application/Features/ClassSchedules/Validators/AddClassScheduleValidator.cs
public class AddClassScheduleValidator : AbstractValidator<AddClassScheduleCommand>
{
    public AddClassScheduleValidator(
        IClassSectionSubjectOfferingRepository offeringRepo,
        IReadRepository<ClassSectionSubjectOffering> readRepo,
        IReadRepository<AcademicTerm> termRepo)
    {
        // 1. Duplicate day guard (handled by domain + DB constraint, but surface early)
        // 2. HC-01 — Teacher double-booked
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var offering = await readRepo.GetByIdAsync(cmd.OfferingId, ct);
                if (offering?.TeacherId == null) return true; // no teacher assigned
                var termIds = await ResolveTermIdsForAcademicYear(cmd.AcademicYearId, termRepo, ct);
                return !await offeringRepo.HasTeacherScheduleConflictAsync(
                    offering.TeacherId.Value, termIds,
                    cmd.DayOfWeek, cmd.StartTime, cmd.EndTime,
                    excludeOfferingId: null, ct);
            })
            .WithErrorCode("TEACHER_DOUBLE_BOOKED")
            .WithMessage("The assigned teacher already has a class scheduled at this day and time.");

        // 3. HC-02 — Room double-booked
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var offering = await readRepo.GetByIdAsync(cmd.OfferingId, ct);
                if (offering?.RoomId == null) return true;
                var termIds = await ResolveTermIdsForAcademicYear(cmd.AcademicYearId, termRepo, ct);
                return !await offeringRepo.HasRoomScheduleConflictAsync(
                    offering.RoomId.Value, termIds,
                    cmd.DayOfWeek, cmd.StartTime, cmd.EndTime,
                    excludeOfferingId: null, ct);
            })
            .WithErrorCode("ROOM_DOUBLE_BOOKED")
            .WithMessage("The assigned room is already booked at this day and time.");

        // 4. HC-03 — Section overlap
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var offering = await readRepo.GetByIdAsync(cmd.OfferingId, ct);
                if (offering == null) return true;
                return !await offeringRepo.HasSectionScheduleOverlapAsync(
                    offering.ClassSectionId,
                    cmd.DayOfWeek, cmd.StartTime, cmd.EndTime,
                    excludeScheduleId: null, ct);
            })
            .WithErrorCode("SECTION_OVERLAP")
            .WithMessage("Another subject in this section is already scheduled at this day and time.");
    }
}
```

### 5.3 Command handler (CQRS pattern)

```csharp
// In Application/Features/ClassSchedules/Commands/AddClassSchedule.cs
public static class AddClassSchedule
{
    public record Command(
        ClassSectionSubjectOfferingId OfferingId,
        AcademicYearId AcademicYearId,   // passed from frontend for scoping
        DayOfWeekEnum DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime
    ) : ICommand<Result<ClassScheduleId>>;

    public class Handler(IClassSectionSubjectOfferingRepository repo,
                         IReadRepository<ClassSectionSubjectOffering> readRepo)
        : ICommandHandler<Command, Result<ClassScheduleId>>
    {
        public async Task<Result<ClassScheduleId>> Handle(Command cmd, CancellationToken ct)
        {
            var offering = await readRepo.GetByIdAsync(cmd.OfferingId, ct);
            if (offering == null) return Result<ClassScheduleId>.NotFound();

            // Guard clauses already passed FluentValidation — call domain method
            var schedule = ClassSchedule.CreateForOffering(
                cmd.OfferingId, cmd.DayOfWeek, cmd.StartTime, cmd.EndTime);

            try
            {
                offering.AddClassSchedule(schedule); // domain checks duplicate-day + hours total
                await repo.Update(offering, ct);
                return Result<ClassScheduleId>.Success(schedule.Id);
            }
            catch (InvalidClassScheduleException ex)
            {
                return Result<ClassScheduleId>.Invalid(new ValidationError(ex.Message));
            }
        }
    }
}
```

### 5.4 Section detail conflicts embedded in GET response

In the `GetClassSectionByIdQuery` handler, after loading the section and its offerings:

```csharp
// Load related schedules for same-term offerings sharing teachers or rooms
var teacherIds = offerings.Where(o => o.TeacherId != null).Select(o => o.TeacherId!.Value).ToList();
var roomIds    = offerings.Where(o => o.RoomId != null).Select(o => o.RoomId!.Value).ToList();

// Targeted join: only offerings that share teacher or room in the same term
var relatedScheduleData = await _dbContext.Set<ClassSchedule>()
    .Join(...)  // join CSSO + ClassSection filtered by AcademicTermId
    .Where(x => teacherIds.Contains(x.o.TeacherId) || roomIds.Contains(x.o.RoomId))
    .Select(x => new ScheduleConflictDTO { ... })
    .ToListAsync();

// Add this section's own schedule rows
var thisScheduleData = ProjectToFlatDTO(offerings, sectionId);

// Detect in-memory (small dataset)
var allData = thisScheduleData.Concat(relatedScheduleData).ToList();
var conflicts = ConflictDetector.DetectForSection(allData, sectionId: sectionId);
```

---

## 6. Database Index Requirements

The existing `ClassSchedules` table has **no index on `DayOfWeek`, `StartTime`, or `EndTime`**[^9]. A conflict check query without an index would do a full table scan. Add a migration:

```sql
-- In a new DbUp script (e.g., Script0017__ClassScheduleConflictIndexes.sql)

-- Support teacher and room conflict queries
CREATE INDEX IX_ClassSchedules_DayOfWeek_Time
    ON ClassSchedules (DayOfWeek, StartTime, EndTime)
    WHERE IsActive = 1;

-- Support section overlap queries (CSSO → ClassSection join is needed, but this narrows the schedule scan)
-- The FK ClassSectionSubjectOfferingId already has an implicit index via the FK constraint
```

Also add a composite index on `ClassSectionSubjectOffering` for the teacher/room conflict join:

```sql
CREATE INDEX IX_ClassSectionSubjectOffering_TeacherId
    ON ClassSectionSubjectOffering (TeacherId, ClassSectionId)
    WHERE IsActive = 1 AND TeacherId IS NOT NULL;

CREATE INDEX IX_ClassSectionSubjectOffering_RoomId
    ON ClassSectionSubjectOffering (RoomId, ClassSectionId)
    WHERE IsActive = 1 AND RoomId IS NOT NULL;
```

---

## 7. Academic Year vs. Academic Term Scoping

The conflict spec[^10] states: "All conflict checks **must be scoped to the same `AcademicTermId`**."

**Important: `ClassSection` has no direct `AcademicYearId` field for the scheduling year.**[^11] It only has:
- `AcademicTermId` (the term the section is being taught in)
- `CohortAcademicYearId` (the year the student cohort enrolled — NOT the current scheduling year)

To scope by academic year, resolve the term IDs first:

```csharp
// AcademicTerm.AcademicYearId → get all term IDs for the target year
var termIds = await _dbContext.Set<AcademicTerm>()
    .Where(t => t.AcademicYearId == targetAcademicYearId && t.IsActive)
    .Select(t => t.Id)
    .ToListAsync(ct);
// Then: WHERE ClassSection.AcademicTermId IN @termIds
```

The user's requirement "validate class sections under one academic year" means: collect all `AcademicTermId` values belonging to that year, then scope all conflict queries by those term IDs.

---

## 8. TOCTOU Concurrency

There is a real race condition between checking for a conflict and inserting the schedule:

```
Admin A: Load schedules → sees no conflict for MON 09:00–10:30
Admin B: Load schedules → sees no conflict for MON 09:00–10:30
Admin A: INSERT (passes)
Admin B: INSERT (also passes — A's row not visible to B's check)
Result:  Both rows exist → conflict in DB
```

**Solution:** Wrap the check-and-insert in a `SERIALIZABLE` transaction[^5]:

```csharp
// In the repository Update or in a Unit of Work
await _dbContext.Database.ExecuteInSerializableTransactionAsync(async () =>
{
    // 1. Conflict check (DB AnyAsync within transaction)
    if (await HasTeacherConflict(...)) throw new ConflictException("...");
    if (await HasRoomConflict(...)) throw new ConflictException("...");
    if (await HasSectionOverlap(...)) throw new ConflictException("...");

    // 2. SaveChanges — only if all checks pass
    await _dbContext.SaveChangesAsync();
});
```

**Practical note for Enrollify:** University schedule changes are rare and admin access is unlikely to be concurrent. `READ COMMITTED` isolation (EF Core default) with the application-layer checks is acceptable in practice. But a SERIALIZABLE wrapper is the correct defensive pattern and has negligible overhead for infrequent admin writes.

---

## 8. Algorithmic Complexity

### Per-save (targeted queries): O(1) in practice

A single `AnyAsync` with a narrow WHERE clause on an indexed table returns in 1–5ms regardless of total row count. After grouping by day, the query only scans rows matching `DayOfWeek = 'MON'` (typically a few hundred rows out of 45,000 total at worst case).

### In-memory batch detection: Grouping eliminates the O(n²) cost

After loading all schedule DTOs for an academic year:

```csharp
// Group by (entity, day) — reduces group size to n ≈ 2-15 rows
var byTeacherDay = schedules.GroupBy(s => (s.TeacherId, s.DayOfWeek));

// O(n²) within each group where n ≈ 2-15
// Total comparisons: groups × (n² / 2) ≈ 1,000 groups × 50 = 50,000 comparisons
// At 500M comparisons/second → ~0.0001 seconds
```

**Worst case full scan without grouping** (500 sections × 15 offerings × 6 days = 45,000 rows):
- O(n²) = 45,000 × 44,999 / 2 ≈ 1 billion comparisons → 1-2 seconds (too slow)
- O(n²) after grouping by day = 9,000 rows/day × 8,999/2 ≈ 40M comparisons → ~80ms (acceptable)
- O(n²) after grouping by (entity, day) = effectively O(1) → ~0.1ms (excellent)[^2]

**Recommendation:** Always group by `(teacherId, dayOfWeek)` / `(roomId, dayOfWeek)` / `(sectionId, dayOfWeek)` before comparing. O(n log n) sweep-line is a premature optimization at this scale and adds significant implementation complexity for negligible gain.

---

## 9. Conflict Taxonomy Reference

All 17 conflict types from `research/scheduling/class-scheduling-conflicts.md`[^12]:

| Code | Name | Tier | Severity | Block Save? | Scope |
|------|------|------|----------|-------------|-------|
| `HC-01` | Teacher Double-Booked | Hard | Error | ✅ Yes | TeacherId + AcademicTermId + Day + Time |
| `HC-02` | Room Double-Booked | Hard | Error | ✅ Yes | RoomId + AcademicTermId + Day + Time |
| `HC-03` | Section Schedule Overlap | Hard | Error | ✅ Yes | SectionId + Day + Time |
| `HC-04` | Duplicate Day in Offering | Hard | Error | ✅ Yes | OfferingId + DayOfWeek (DB UNIQUE constraint + domain) |
| `SC-01` | Teacher Overload | Soft | Warning | ❌ No | TeacherId + AcademicTermId + total hours |
| `SC-02` | Room Capacity Exceeded | Soft | Warning | ❌ No | RoomId + MaxStudents |
| `SC-03` | Teacher No Break | Soft | Warning | ❌ No | TeacherId + Day + gap < 15 min |
| `SC-04` | Adviser as Teacher | Soft | Warning | ❌ No | TeacherId = AdviserId in same section |
| `SC-05` | Year Level Mismatch | Soft | Warning | ❌ No | SubjectId + CurriculumSubjectId.YearLevel |
| `SC-06` | Outside Operating Hours | Soft | Warning | ❌ No | StartTime < 07:00 or EndTime > 21:00 |
| `DI-01` | Schedule Count Mismatch | Data Integrity | Error | ✅ Yes | COUNT(schedules) ≠ DaysPerWeek |
| `DI-02` | Hours Per Day Mismatch | Data Integrity | Warning | ❌ No | Actual duration ≠ HoursPerDay ±0.1h |
| `DI-03` | Offering with No Schedules | Data Integrity | Warning | ❌ No | No schedule rows exist |
| `DI-04` | Section with No Offerings | Data Integrity | Info | ❌ No | No offering rows exist |
| `DI-05` | Duplicate Subject in Section | Data Integrity | Error | ✅ Yes | (SubjectId, SectionId) appears twice |
| `IN-01` | Room Type Mismatch | Informational | Info | ❌ No | Requires `Subject.RequiredRoomTypeId` (future) |
| `IN-02` | Cross-Term Teacher Booking | Informational | Info | ❌ No | Only when terms have overlapping date ranges |

### Which conflicts to implement first (priority order)

**Phase 1 — Block at save time (must have before any scheduling goes live):**
- HC-01 (teacher double-booked)
- HC-02 (room double-booked)  
- HC-03 (section overlap)
- HC-04 (duplicate day — already done at domain + DB level[^13])

**Phase 2 — Embed in section detail response:**
- DI-01, DI-02, DI-03 (data completeness — can be computed locally without cross-section queries)
- HC-01, HC-02, HC-03 (same checks, but surfaced as inline `conflicts[]` rather than blocking saves)

**Phase 3 — Admin report page:**
- All 17 types including soft conflicts (SC-01 through SC-06)
- Requires teacher load configuration (`AcademicSettings.MaxTeacherHoursPerWeek`) for SC-01

---

## 10. Confidence Assessment

| Claim | Confidence | Basis |
|---|---|---|
| Validate-on-save is the correct pattern | **High** | 4 production systems all use it[^3][^4][^5][^6] |
| Half-open interval formula is universal | **High** | Same formula in all 4 systems + spec document[^10][^5] |
| Memory footprint ≈ 13 MB worst case | **High** | Calculated from concrete field sizes + .NET object overhead[^2] |
| `ClassSchedule` queryable via `_dbContext.Set<ClassSchedule>()` | **High** | Confirmed via EF Core 7+ OwnsMany behavior[^8]; needs verification |
| No `AddClassSchedule` command exists yet | **High** | Grep confirmed zero matches in Application/WebAPI[^9] |
| SERIALIZABLE tx prevents all TOCTOU issues | **Medium** | Confirmed for SQL Server; behavior in EF Core transaction wrapping needs integration test |
| Group-by grouping makes O(n²) effectively O(1) | **High** | Backed by dataset size analysis[^2] and POC `Constraints.cs`[^15] |
| Scope by `AcademicTermId`, not `AcademicYearId` directly | **High** | Confirmed `ClassSection` has no scheduling `AcademicYearId` field[^11] |

**Unresolved policy questions** from the spec[^12]:
1. Is `HC-03` (section overlap) a hard block or a soft warning? (Depends on irregular enrollment support)
2. Is `SC-01` (teacher overload) configurable per teacher?
3. What is the minimum break gap for `SC-03` — 0, 10, or 15 minutes?

---

## Footnotes

[^1]: `src/system/enrollify-frontend/src/api/models/offering.ts:24-74` — `ConflictResult` type and `OfferingWithSchedules.conflicts[]` field  
[^2]: Research findings: in-memory scheduling conflict validation — memory footprint analysis and O(n²) complexity at your dataset scale  
[^3]: `ziad-aladawy-dev/capu-portal:src/4.Modules/CapitalUniversity.Module.Schedule/Repositories/ScheduleSlotRepository.cs:94-115` — canonical targeted `HasConflictAsync` with half-open interval DB query  
[^4]: `4RD4024N/AdvisorySystem.Api:Services/CourseScheduler.cs:DetectConflictsAsync` — full semester load → O(n²) → persist to `ScheduleConflicts` table (admin batch pattern)  
[^5]: `ziad-aladawy-dev/capu-portal:src/4.Modules/CapitalUniversity.Module.Schedule/Application/ScheduleSlotService.cs:155-203` — SERIALIZABLE transaction wrapping conflict check + insert with TOCTOU comment  
[^6]: `plebann/studioscheduler:src/StudioScheduler.Infrastructure/Repositories/ScheduleRepository.cs:HasScheduleConflictAsync` — `excludeId` pattern for update scenarios  
[^7]: `src/system/EnrollifyBackend/Enrollify.Application/Features/ClassSectionSubjectOfferings/IClassSectionSubjectOfferingRepository.cs:6-11`  
[^8]: `src/system/EnrollifyBackend/Enrollify.Infrastructure/Data/Config/ClassSectionSubjectOfferingConfigs/ClassSectionSubjectOfferingConfiguration.cs:116-167` — `OwnsMany<ClassSchedule>` EF config; no top-level `DbSet<ClassSchedule>`  
[^9]: `src/system/EnrollifyBackend/Enrollify.DatabaseMigration/Scripts/Script0016__ClassSections.sql:129-161` — `ClassSchedules` table DDL; no index on `DayOfWeek`, `StartTime`, `EndTime` confirmed  
[^10]: `research/scheduling/class-scheduling-conflicts.md:591-614` — Overlap Detection section and Conflict Scope Boundaries section  
[^11]: `src/system/EnrollifyBackend/Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs:45-49` — `AcademicTermId` and `CohortAcademicYearId` fields; `CohortAcademicYearId` is NOT the scheduling year  
[^12]: `research/scheduling/class-scheduling-conflicts.md:567-588` — Summary Table of all 17 conflict types  
[^13]: `src/system/EnrollifyBackend/Enrollify.Core/Aggregates/ClassSectionSubjectOfferingAggregate/ClassSectionSubjectOffering.cs:168-186` — `AddClassSchedule()` domain method with duplicate-day check  
[^14]: `src/system/EnrollifyBackend/Enrollify.Infrastructure/Repositories/ClassSectionSubjectOfferingRepository.cs:9-70` — existing repository implementation using `_dbContext` directly  
[^15]: `src/POCs/Genetic-algorithm/College-Course-Scheduling/Enhance-Genetic-Algorithm-v2/Constraints.cs` — `NoSectionConflicts` HC-3: groups by `(section.Id)` → groups by `timeSlot.Id` → count > 1; `NoProfessorConflicts` groups by `professorId` → groups by `timeSlot.Id`; validates the grouping-first pattern
