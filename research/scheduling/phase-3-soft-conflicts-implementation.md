# Phase 3: Soft Conflicts & Admin Dashboard

**Status:** Deferred  
**Created:** 2026-06-06  
**Dependencies:** Phase 1 + 2 must be complete and tested in production

---

## Overview

Phase 3 adds:
1. **Soft conflict detection** (SC-01 through SC-06) — warnings that don't block saves
2. **Data integrity checks** (DI-01, DI-02, DI-03) — incomplete/inconsistent offerings
3. **Admin conflict dashboard** — dedicated page showing all conflicts across an academic term
4. **Per-teacher/per-room conflict views** — filtered conflict pages

---

## Soft Conflicts (Tier 2)

###SC-01: Teacher Workload Overload

**Detection:** Total teaching hours for a teacher in a term exceeds configured maximum

```sql
SELECT o.TeacherId,
       SUM(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0) AS TotalHours
FROM ClassSchedules cs
JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
JOIN ClassSections sec ON sec.Id = o.ClassSectionId
WHERE sec.AcademicTermId = @termId
  AND o.TeacherId = @teacherId
  AND o.IsActive = 1 AND cs.IsActive = 1
GROUP BY o.TeacherId
HAVING SUM(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0) > @maxLoad
```

**Prerequisite:** Add `MaxTeacherHoursPerWeek` to `AcademicCoreSettings` (e.g., 18 hours)

---

### SC-02: Room Capacity Exceeded

**Detection:** Expected students exceed room capacity

```csharp
var effectiveStudents = offering.MaxNumberOfStudents ?? section.StudentCapacity;
if (effectiveStudents > room.Capacity) → ROOM_CAPACITY_EXCEEDED
```

**Severity:** Warning (overrides are allowed per schema design)

---

### SC-03: Teacher No Break

**Detection:** Back-to-back classes with gap < `MinimumTeacherBreakMinutes`

```sql
SELECT cs1.EndTime, cs2.StartTime,
       DATEDIFF(MINUTE, cs1.EndTime, cs2.StartTime) AS GapMinutes
FROM ClassSchedules cs1
JOIN ClassSectionSubjectOffering o1 ON o1.Id = cs1.ClassSectionSubjectOfferingId
JOIN ClassSchedules cs2 ON cs2.DayOfWeek = cs1.DayOfWeek
JOIN ClassSectionSubjectOffering o2 ON o2.Id = cs2.ClassSectionSubjectOfferingId
WHERE o1.TeacherId = o2.TeacherId
  AND o1.Id <> o2.Id
  AND cs1.EndTime <= cs2.StartTime
  AND DATEDIFF(MINUTE, cs1.EndTime, cs2.StartTime) < @minBreakMinutes
```

**Configuration:** `AcademicCoreSettings.MinimumTeacherBreakMinutes` (already added in Phase 1)

---

### SC-04: Adviser as Teacher

**Detection:** Section adviser teaches in their own section

```sql
SELECT s.AdviserId, o.TeacherId
FROM ClassSectionSubjectOffering o
JOIN ClassSections s ON s.Id = o.ClassSectionId
WHERE o.TeacherId = s.AdviserId AND o.IsActive = 1
```

**Severity:** Info (may be institutionally allowed)

---

### SC-05: Year Level Mismatch

**Detection:** Subject offered to wrong year level per curriculum

```sql
SELECT o.SubjectId, cs.IntendedYearLevel AS SectionYearLevel,
       cu.YearLevel AS CurriculumYearLevel
FROM ClassSectionSubjectOffering o
JOIN ClassSections cs ON cs.Id = o.ClassSectionId
JOIN CurriculumSubjects cu ON cu.SubjectId = o.SubjectId
WHERE cu.CurriculumId = cs.CurriculumId
  AND cu.YearLevel <> cs.IntendedYearLevel
  AND o.IsActive = 1
```

---

### SC-06: Outside Operating Hours

**Detection:** Schedule outside `AcademicCoreSettings` configured hours

```csharp
if (schedule.StartTime < settings.EarliestClassStartTime ||
    schedule.EndTime > settings.LatestClassEndTime)
    → OUTSIDE_OPERATING_HOURS
```

**Configuration:** `EarliestClassStartTime` / `LatestClassEndTime` (already added in Phase 1)

---

## Data Integrity Checks (Tier 3)

### DI-01: Schedule Count Mismatch

**Detection:** `DaysPerWeek` doesn't match actual schedule count

```sql
SELECT o.Id, o.DaysPerWeek, COUNT(cs.Id) AS ActualScheduledDays
FROM ClassSectionSubjectOffering o
LEFT JOIN ClassSchedules cs ON cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
WHERE o.IsActive = 1
GROUP BY o.Id, o.DaysPerWeek
HAVING COUNT(cs.Id) <> o.DaysPerWeek
```

---

### DI-02: Hours Per Day Mismatch

**Detection:** Actual duration differs from `HoursPerDay`

```sql
SELECT o.Id, o.HoursPerDay,
       DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0 AS ActualHours
FROM ClassSectionSubjectOffering o
JOIN ClassSchedules cs ON cs.ClassSectionSubjectOfferingId = o.Id
WHERE ABS(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0 - o.HoursPerDay) > 0.1
```

---

### DI-03: Offering with No Schedules

**Detection:** Active offering with zero schedule rows

```sql
SELECT o.Id
FROM ClassSectionSubjectOffering o
WHERE o.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM ClassSchedules cs
    WHERE cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
  )
```

---

## Admin Dashboard Design

### Page 1: Conflict Overview

**URL:** `/admin/scheduling/conflicts?termId=10`

**Features:**
- Summary cards: Total conflicts, by type, by severity
- Filters: Academic term, conflict type, severity, teacher, room, section
- Table: All conflicts with affected offerings, sortable, exportable to CSV

**API Endpoint:** `GET /api/admin/scheduling/conflicts?termId=10`

**Response:**
```json
{
  "summary": {
    "totalConflicts": 42,
    "byType": {
      "TEACHER_DOUBLE_BOOKED": 15,
      "ROOM_DOUBLE_BOOKED": 10,
      "SECTION_OVERLAP": 5,
      "TEACHER_OVERLOAD": 8,
      "ROOM_CAPACITY_EXCEEDED": 4
    },
    "bySeverity": { "error": 30, "warning": 12 }
  },
  "conflicts": [ /* ConflictResultDto[] */ ]
}
```

---

### Page 2: Teacher Conflict View

**URL:** `/admin/scheduling/teachers/{id}/conflicts?termId=10`

**Features:**
- Teacher card: Photo, name, total hours, overload status
- Weekly grid: Visual timeline (07:00-21:00 × MON-SAT) with conflict highlights
- Conflict list: All conflicts affecting this teacher

---

### Page 3: Room Conflict View

**URL:** `/admin/scheduling/rooms/{id}/conflicts?termId=10`

**Features:**
- Room card: Building, capacity, type, utilization %
- Weekly grid: Booking timeline with conflict highlights
- Conflict list: All conflicts for this room

---

## Implementation Steps

1. **Extend `ScheduleConflictDetector`:**
   - Add methods: `DetectSoftConflicts()`, `DetectDataIntegrityIssues()`
   - Each method returns `List<ConflictResult>`

2. **Create `GetAllConflictsForAcademicTermQuery`:**
   - Loads all schedule DTOs for the year
   - Runs full conflict detection (all 17 types)
   - Returns grouped/filtered conflict list

3. **Create admin endpoints:**
   - `GET /api/admin/scheduling/conflicts` (overview)
   - `GET /api/admin/scheduling/teachers/{id}/conflicts`
   - `GET /api/admin/scheduling/rooms/{id}/conflicts`

4. **Frontend:**
   - Admin pages under `/admin/scheduling/`
   - Weekly grid component (7×14 time grid with slot rendering)
   - Conflict badge components (color-coded by severity)

5. **Testing:**
   - Unit tests for each soft conflict type
   - Integration tests for batch conflict queries
   - Performance test: Load 45,000 schedules, detect all conflicts, measure time

---

## Configuration Requirements

Before Phase 3, ensure these settings exist:

| Setting | Location | Default | Purpose |
|---------|----------|---------|---------|
| `MinimumTeacherBreakMinutes` | `AcademicCoreSettings` | 10 | SC-03 |
| `EarliestClassStartTime` | `AcademicCoreSettings` | 07:00 | SC-06 |
| `LatestClassEndTime` | `AcademicCoreSettings` | 21:00 | SC-06 |
| `MaxTeacherHoursPerWeek` | `AcademicCoreSettings` | 18 | SC-01 (add in Phase 3) |

---

## Performance Considerations

- **Batch conflict detection:** Load all schedules for a term (~13 MB worst case), group by (entity, day), run O(n²) within groups → ~100ms total
- **Caching:** Consider caching conflict report for 5 minutes (conflicts change infrequently)
- **Incremental updates:** When a schedule changes, only recompute conflicts for affected teachers/rooms

---

## References

- Conflict taxonomy: `research/scheduling/class-scheduling-conflicts.md`
- Implementation guide: `research/how-to-properly-implement-a-validation-logic-that-.md`
- Phase 1 implementation: `Enrollify.Core/Services/ScheduleConflictDetection/`
