# Class Scheduling Conflicts — Enrollify

**Date:** 2026-05-15  
**Scope:** `ClassSections`, `ClassSectionSubjectOffering`, `ClassSchedules`  
**Purpose:** Exhaustive catalogue of all detectable conflicts across the three-level scheduling hierarchy, with descriptions, detection logic, and recommended UI treatment.

---

## Overview

The scheduling model has three levels:

```
ClassSections
  └── ClassSectionSubjectOffering  (Subject + Teacher + Room per section)
        └── ClassSchedules         (one row per day: DayOfWeek + StartTime + EndTime)
```

Conflicts fall into four tiers based on severity and where they need to be enforced:

| Tier | Label | Color | Blocks Save? |
|------|-------|-------|-------------|
| 1 | **Hard Conflict** | Red / Destructive | Yes — save must be disabled |
| 2 | **Soft Conflict / Warning** | Amber | No — save allowed after acknowledgment |
| 3 | **Data Integrity Issue** | Orange | Yes — data is structurally invalid |
| 4 | **Informational** | Blue | No — advisory only |

---

## Tier 1 — Hard Conflicts (Block Save)

These represent absolute scheduling impossibilities. Two physical entities (a person or a room) cannot occupy two places at once.

---

### HC-01 · Teacher Double-Booked

**Type code:** `TEACHER_DOUBLE_BOOKED`  
**Severity:** Error  

**Description:**  
The same teacher is assigned to two different `ClassSectionSubjectOffering` records whose `ClassSchedules` rows overlap on the same `DayOfWeek` and time window. A teacher physically cannot teach two classes simultaneously.

**Example:**  
- Dr. Santos is assigned to BSCS-1A / CS101 → MON 09:00–10:30  
- Dr. Santos is also assigned to BSCS-2A / CS201 → MON 09:00–10:30  

Both offerings share the same `TeacherId`. Their schedule rows overlap on Monday.

**Root cause:** No DB-level uniqueness constraint prevents duplicate teacher + day + time assignments across offerings. The only DB constraint is `UNIQUE (ClassSectionSubjectOfferingId, DayOfWeek)`, which prevents duplicate days *within* a single offering, not *across* offerings.

**Detection query (conceptual):**
```sql
SELECT cs1.*, cs2.*
FROM ClassSchedules cs1
JOIN ClassSectionSubjectOffering o1 ON o1.Id = cs1.ClassSectionSubjectOfferingId
JOIN ClassSchedules cs2 ON cs2.DayOfWeek = cs1.DayOfWeek
JOIN ClassSectionSubjectOffering o2 ON o2.Id = cs2.ClassSectionSubjectOfferingId
WHERE o1.TeacherId = o2.TeacherId
  AND o1.Id <> o2.Id                          -- different offerings
  AND o1.IsActive = 1 AND o2.IsActive = 1
  AND cs1.IsActive = 1 AND cs2.IsActive = 1
  -- time overlap: not (cs1.EndTime <= cs2.StartTime OR cs1.StartTime >= cs2.EndTime)
  AND cs1.StartTime < cs2.EndTime
  AND cs1.EndTime   > cs2.StartTime
```

**Frontend detection trigger:** When saving a new `ClassSchedules` row (POST to `/api/offerings/{id}/schedules`) — the backend must check all other active schedule rows for the same `TeacherId` on the same day in the same academic term.

**UI treatment:**
- Red `ConflictCard` in the Conflicts tab
- Inline red badge on the affected `OfferingCard`
- Conflict preview panel in `ScheduleRowFormDrawer` (before save)
- Save button disabled until resolved

---

### HC-02 · Room Double-Booked

**Type code:** `ROOM_DOUBLE_BOOKED`  
**Severity:** Error  

**Description:**  
The same room is assigned to two different offerings whose schedule rows overlap on the same day and time. A room cannot host two classes simultaneously.

**Example:**  
- Room 101 is assigned to BSCS-1A / CS101 → TUE 13:00–14:30  
- Room 101 is also assigned to IT-2A / NET101 → TUE 13:00–14:30  

**Root cause:** `ClassSectionSubjectOffering.RoomId` is NOT NULL and required at creation, but no constraint prevents the same room from being booked twice at the same time.

**Detection query (conceptual):**
```sql
SELECT cs1.*, cs2.*
FROM ClassSchedules cs1
JOIN ClassSectionSubjectOffering o1 ON o1.Id = cs1.ClassSectionSubjectOfferingId
JOIN ClassSchedules cs2 ON cs2.DayOfWeek = cs1.DayOfWeek
JOIN ClassSectionSubjectOffering o2 ON o2.Id = cs2.ClassSectionSubjectOfferingId
WHERE o1.RoomId = o2.RoomId
  AND o1.Id <> o2.Id
  AND o1.IsActive = 1 AND o2.IsActive = 1
  AND cs1.IsActive = 1 AND cs2.IsActive = 1
  AND cs1.StartTime < cs2.EndTime
  AND cs1.EndTime   > cs2.StartTime
```

**Scope note:** The conflict check must be scoped to the same `AcademicTermId` (via the parent `ClassSection`). A room can be reused across different academic terms with no conflict.

**Frontend detection trigger:** Same as HC-01 — on POST/PUT of a schedule row.

**UI treatment:** Identical to HC-01 (red card, disabled save).

---

### HC-03 · Section Schedule Overlap

**Type code:** `SECTION_OVERLAP`  
**Severity:** Error (recommended — see note below)  

**Description:**  
Two different offerings within the *same* `ClassSection` have overlapping schedule rows on the same day. Students in the section would be required to attend two classes simultaneously, which is physically impossible.

**Example:**  
- BSCS-1A has CS101 (Dr. Santos, Room 101) → WED 09:00–10:30  
- BSCS-1A also has MATH101 (Prof. Reyes, Room 202) → WED 09:00–11:00  

Both offerings share the same `ClassSectionId`. Their Wednesday rows overlap.

**Root cause:** No constraint prevents two offerings in the same section from having overlapping times. The DB only enforces uniqueness of `(OfferingId, DayOfWeek)` within a single offering, not across offerings in the same section.

**Detection query (conceptual):**
```sql
SELECT cs1.*, cs2.*
FROM ClassSchedules cs1
JOIN ClassSectionSubjectOffering o1 ON o1.Id = cs1.ClassSectionSubjectOfferingId
JOIN ClassSchedules cs2 ON cs2.DayOfWeek = cs1.DayOfWeek
JOIN ClassSectionSubjectOffering o2 ON o2.Id = cs2.ClassSectionSubjectOfferingId
WHERE o1.ClassSectionId = o2.ClassSectionId   -- same section
  AND o1.Id <> o2.Id
  AND o1.IsActive = 1 AND o2.IsActive = 1
  AND cs1.IsActive = 1 AND cs2.IsActive = 1
  AND cs1.StartTime < cs2.EndTime
  AND cs1.EndTime   > cs2.StartTime
```

> **Policy Note:** The research doc marks `SECTION_OVERLAP` as a *warning* in the original conflict table (§10.1), but this is arguably a hard conflict. Regular students are assigned to a section and attend all its offerings; an overlap is physically impossible. It is recommended to treat this as a **hard error** unless the system explicitly supports irregular enrollment where students can pick and choose offerings within a section.

---

### HC-04 · Duplicate Day Within an Offering

**Type code:** `DUPLICATE_DAY_IN_OFFERING`  
**Severity:** Error  

**Description:**  
Attempting to create two `ClassSchedules` rows for the same `ClassSectionSubjectOfferingId` with the same `DayOfWeek`. This is structurally invalid — a subject cannot appear twice on the same day under the same offering record.

**Example:**  
Attempting to add:
- Offering #5 → MON 09:00–10:30  
- Offering #5 → MON 14:00–15:30  

These should instead be two separate offerings if split sessions on the same day are needed.

**Root cause:** This is already enforced by the database constraint `CONSTRAINT UQ_ClassSchedules_Offering_Day UNIQUE (ClassSectionSubjectOfferingId, DayOfWeek)`. The application layer should detect this before hitting the DB to provide a user-friendly error rather than a constraint violation.

**Detection:** Check existing `ClassSchedules` rows for the same `OfferingId` before inserting. The form's day selector should also **disable already-used days**.

**UI treatment:**  
- Disable used days in the `DayOfWeek` select dropdown in `ScheduleRowFormDrawer`
- Show error if attempted via API: "This offering already has a schedule on [day]"

---

## Tier 2 — Soft Conflicts / Warnings (Allow with Acknowledgment)

These are real problems but may have legitimate exceptions (e.g., a teacher authorised to overload, a room used temporarily beyond capacity).

---

### SC-01 · Teacher Workload Overload

**Type code:** `TEACHER_OVERLOAD`  
**Severity:** Warning  

**Description:**  
A teacher's total scheduled teaching hours (or units) across all offerings in the same `AcademicTermId` exceeds the institutional maximum teaching load. This doesn't prevent scheduling but is a policy violation that should be flagged.

**Example:**  
- Prof. Reyes is assigned to 5 different offerings totalling 25 hours/week  
- Institutional max is 18 hours/week  

**Detection logic:**
```sql
SELECT o.TeacherId, SUM(o.HoursPerDay * o.DaysPerWeek) AS TotalHoursPerWeek
FROM ClassSectionSubjectOffering o
JOIN ClassSections s ON s.Id = o.ClassSectionId
WHERE s.AcademicTermId = @termId
  AND o.TeacherId = @teacherId
  AND o.IsActive = 1
GROUP BY o.TeacherId
HAVING SUM(o.HoursPerDay * o.DaysPerWeek) > @maxLoad
```

Alternative using `ClassSchedules` directly (more accurate if `DaysPerWeek`/`HoursPerDay` may be inconsistent):
```sql
-- Sum actual scheduled hours from ClassSchedules
SELECT o.TeacherId,
       SUM(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0) AS TotalHours
FROM ClassSchedules cs
JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
JOIN ClassSections sec ON sec.Id = o.ClassSectionId
WHERE sec.AcademicTermId = @termId
  AND o.TeacherId = @teacherId
  AND o.IsActive = 1 AND cs.IsActive = 1
GROUP BY o.TeacherId
```

**UI treatment:**
- Amber badge on the teacher picker in `AssignOfferingDrawer`
- Inline hint: "Current load: 22 / 18 hrs" with amber text
- Amber `ConflictCard` in Conflicts tab (advisory, not blocking)

---

### SC-02 · Room Capacity Exceeded

**Type code:** `ROOM_CAPACITY_EXCEEDED`  
**Severity:** Warning  

**Description:**  
The number of students expected in an offering exceeds the assigned room's seating capacity. `ClassSectionSubjectOffering.MaxNumberOfStudents` (or `ClassSection.StudentCapacity` as fallback) is greater than `Rooms.Capacity`.

**Detection logic:**
```
effectiveStudents = offering.MaxNumberOfStudents ?? section.StudentCapacity
if effectiveStudents > room.Capacity → ROOM_CAPACITY_EXCEEDED
```

**Why it's a warning, not an error:**  
The field `MaxNumberOfStudents` is described as a *soft rule* in the schema: "this to allow us to override the room student capacity." This implies intentional overrides are supported.

**UI treatment:**
- Amber inline hint below the Room picker: "Room capacity: 30 · Expected students: 45"
- Amber `ConflictCard` in Conflicts tab

---

### SC-03 · Short Turnaround (Back-to-Back with No Break)

**Type code:** `TEACHER_NO_BREAK`  
**Severity:** Warning  

**Description:**  
A teacher has two offerings scheduled consecutively on the same day with zero (or insufficient) break time between them. For example, one class ends at 10:30 and the next starts at 10:30 in a different building, leaving no time to travel.

**Example:**  
- Dr. Santos: CS101 → MON 09:00–10:30, Room 101, Main Bldg  
- Dr. Santos: CS202 → MON 10:30–12:00, Room 301, Annex Bldg  

Zero minutes between back-to-back classes.

**Detection logic:**
```sql
-- Find offerings for the same teacher on the same day where gap < threshold (e.g., 15 min)
SELECT cs1.EndTime, cs2.StartTime,
       DATEDIFF(MINUTE, cs1.EndTime, cs2.StartTime) AS GapMinutes
FROM ClassSchedules cs1
JOIN ClassSectionSubjectOffering o1 ON o1.Id = cs1.ClassSectionSubjectOfferingId
JOIN ClassSchedules cs2 ON cs2.DayOfWeek = cs1.DayOfWeek
JOIN ClassSectionSubjectOffering o2 ON o2.Id = cs2.ClassSectionSubjectOfferingId
WHERE o1.TeacherId = o2.TeacherId
  AND o1.Id <> o2.Id
  AND cs1.EndTime <= cs2.StartTime   -- cs1 ends before cs2 starts (no overlap — handled by HC-01)
  AND DATEDIFF(MINUTE, cs1.EndTime, cs2.StartTime) < 15   -- less than 15 min gap
```

**UI treatment:**
- Amber warning in teacher availability hint
- Informational note in Conflicts tab

---

### SC-04 · Adviser Teaching in Their Own Section

**Type code:** `ADVISER_AS_TEACHER`  
**Severity:** Warning  

**Description:**  
The teacher assigned as the `ClassSection.AdviserId` is also assigned as the `TeacherId` of one or more offerings within that same section. This may be institutionally allowed (a homeroom adviser who also teaches a subject to their section) but is unusual enough to warrant a warning.

**Detection logic:**
```sql
SELECT s.AdviserId, o.TeacherId, o.SubjectId, s.Name
FROM ClassSectionSubjectOffering o
JOIN ClassSections s ON s.Id = o.ClassSectionId
WHERE o.TeacherId = s.AdviserId
  AND o.IsActive = 1
```

**UI treatment:**
- Amber info chip on the teacher picker when the selected teacher matches the section adviser
- Not shown in Conflicts tab by default (informational only)

---

### SC-05 · Subject Offered Outside Its Curriculum Year Level

**Type code:** `YEAR_LEVEL_MISMATCH`  
**Severity:** Warning  

**Description:**  
A subject is assigned via `ClassSectionSubjectOffering` to a `ClassSection` whose `YearLevel` does not match the subject's expected `YearLevel` in the curriculum (`CurriculumSubjects.YearLevel`). For example, a 3rd-year subject is being offered to a 1st-year section.

**Example:**  
- BSCS-1A has `YearLevel = 1`  
- CS301 (Data Structures) is listed in the curriculum at `YearLevel = 2`  

Offering CS301 to BSCS-1A is a curriculum mismatch.

**Detection logic:**
```sql
SELECT o.SubjectId, cs.YearLevel AS SectionYearLevel,
       cu.YearLevel AS CurriculumYearLevel
FROM ClassSectionSubjectOffering o
JOIN ClassSections cs ON cs.Id = o.ClassSectionId
JOIN CurriculumSubjects cu ON cu.SubjectId = o.SubjectId
JOIN Curriculums c ON c.Id = cu.CurriculumId
WHERE c.CourseId = cs.CourseId   -- same course
  AND cu.YearLevel <> cs.YearLevel
  AND o.IsActive = 1
  AND cu.IsActive = 1
```

**UI treatment:**
- Amber advisory in the subject picker when the year level doesn't match
- Amber `ConflictCard` in Conflicts tab

---

### SC-06 · Schedule Outside Standard Operating Hours

**Type code:** `OUTSIDE_OPERATING_HOURS`  
**Severity:** Warning  

**Description:**  
A `ClassSchedules` row has a `StartTime` before the institution's earliest allowed time (e.g., before 07:00) or an `EndTime` after the latest allowed time (e.g., after 21:00). The Weekly Grid is designed with slots from 07:00–21:00; schedules outside this range will not render properly.

**Detection logic:**
```
if StartTime < '07:00' OR EndTime > '21:00' → OUTSIDE_OPERATING_HOURS
```

**UI treatment:**
- Amber warning in `ScheduleRowFormDrawer` time fields
- Note in Conflicts tab if existing data violates this

---

## Tier 3 — Data Integrity Issues (Block Save — Structural Invalidity)

These are not scheduling conflicts in the resource-contention sense, but they indicate the offering's data is structurally incomplete or inconsistent.

---

### DI-01 · DaysPerWeek / Schedule Count Mismatch

**Type code:** `SCHEDULE_COUNT_MISMATCH`  
**Severity:** Error (data integrity)  

**Description:**  
`ClassSectionSubjectOffering.DaysPerWeek` declares the number of days per week the subject meets (e.g., 3 for MWF), but the actual number of active `ClassSchedules` rows for that offering differs. This indicates a partially-saved offering.

**Example:**  
- `DaysPerWeek = 3` (MWF pattern)  
- Only 2 `ClassSchedules` rows exist (MON + WED only, FRI missing)  

**Detection logic:**
```sql
SELECT o.Id, o.DaysPerWeek,
       COUNT(cs.Id) AS ActualScheduledDays
FROM ClassSectionSubjectOffering o
LEFT JOIN ClassSchedules cs ON cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
WHERE o.IsActive = 1
GROUP BY o.Id, o.DaysPerWeek
HAVING COUNT(cs.Id) <> o.DaysPerWeek
   AND o.DaysPerWeek IS NOT NULL
```

**UI treatment:**
- Orange badge on `OfferingCard`: "⚠ 2 / 3 days scheduled"
- Listed in the Conflicts tab as an incomplete offering

---

### DI-02 · HoursPerDay / Actual Duration Mismatch

**Type code:** `HOURS_MISMATCH`  
**Severity:** Warning (data integrity)  

**Description:**  
`ClassSectionSubjectOffering.HoursPerDay` declares the duration of each session (e.g., 1.5 hours), but the actual duration computed from `StartTime`–`EndTime` in the `ClassSchedules` rows differs. This creates an inconsistency between the metadata and the actual scheduled block.

**Example:**  
- `HoursPerDay = 1.5`  
- MON schedule: 09:00–10:00 → actual = 1.0 hour  

**Detection logic:**
```sql
SELECT o.Id, o.HoursPerDay,
       cs.DayOfWeek,
       DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0 AS ActualHours
FROM ClassSectionSubjectOffering o
JOIN ClassSchedules cs ON cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
WHERE o.IsActive = 1
  AND o.HoursPerDay IS NOT NULL
  AND ABS(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0 - o.HoursPerDay) > 0.1
```

**UI treatment:**
- Amber badge on schedule row: "⚠ Declared 1.5h but scheduled for 1.0h"
- Shown in Conflicts tab as a data inconsistency

---

### DI-03 · Offering with No Schedule Rows

**Type code:** `NO_SCHEDULES`  
**Severity:** Warning (incomplete)  

**Description:**  
A `ClassSectionSubjectOffering` exists but has zero active `ClassSchedules` rows. The subject and teacher are assigned, but no actual time has been set. The offering appears in the section's list but is invisible in the Weekly Grid.

**Detection logic:**
```sql
SELECT o.Id, o.SubjectId, o.ClassSectionId
FROM ClassSectionSubjectOffering o
WHERE o.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM ClassSchedules cs
    WHERE cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
  )
```

**UI treatment:**
- Amber "No schedules yet" badge on `OfferingCard`
- Distinct empty state in the schedule row list inside the card
- Listed as incomplete in Conflicts tab

---

### DI-04 · Section with No Offerings

**Type code:** `NO_OFFERINGS`  
**Severity:** Info (incomplete)  

**Description:**  
A `ClassSection` exists with zero active `ClassSectionSubjectOffering` records. The section has no subjects assigned for the term.

**Detection logic:**
```sql
SELECT s.Id, s.Name
FROM ClassSections s
WHERE s.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM ClassSectionSubjectOffering o
    WHERE o.ClassSectionId = s.Id AND o.IsActive = 1
  )
```

**UI treatment:**
- Empty state indicator on the Section Detail page Offerings tab
- Optional informational badge on the Sections list: "0 offerings"

---

### DI-05 · Duplicate Subject Offering in Same Section

**Type code:** `DUPLICATE_SUBJECT_IN_SECTION`  
**Severity:** Error (data integrity)  

**Description:**  
The same `SubjectId` is assigned to the same `ClassSectionId` more than once in active offerings. A section cannot have duplicate subject assignments — this creates ambiguity for enrollment.

**Example:**  
- BSCS-1A has two active offerings both for CS101 (different teachers, rooms, or schedules).

**Root cause:** No DB-level unique constraint prevents duplicate `(SubjectId, ClassSectionId)` among active offerings.

**Detection logic:**
```sql
SELECT o.SubjectId, o.ClassSectionId, COUNT(*) AS DuplicateCount
FROM ClassSectionSubjectOffering o
WHERE o.IsActive = 1
GROUP BY o.SubjectId, o.ClassSectionId
HAVING COUNT(*) > 1
```

**UI treatment:**
- Application-layer validation when adding an offering: check if `SubjectId` already exists for the section  
- Error toast: "CS101 is already assigned to this section"  
- Listed in Conflicts tab as a data integrity error

---

## Tier 4 — Informational Issues (Advisory Only)

These are not errors or warnings but observable conditions that a registrar may want to know about.

---

### IN-01 · Room Type Mismatch

**Type code:** `ROOM_TYPE_MISMATCH`  
**Severity:** Info  

**Description:**  
A subject that is typically held in a specific room type (e.g., a Computer Laboratory for programming subjects, or a Science Laboratory for chemistry) is assigned to a room of a different type (e.g., a regular lecture room).

**Prerequisite:** This conflict requires a `RequiredRoomTypeId` or similar field on the `Subjects` table, which is not currently in the schema. This is noted as a **future enhancement** once room-type requirements are modelled on subjects.

**Detection logic (future):**
```sql
-- Once Subjects.RequiredRoomTypeId is added:
SELECT o.Id
FROM ClassSectionSubjectOffering o
JOIN Subjects sub ON sub.Id = o.SubjectId
JOIN Rooms r ON r.Id = o.RoomId
WHERE sub.RequiredRoomTypeId IS NOT NULL
  AND sub.RequiredRoomTypeId <> r.RoomTypeId
  AND o.IsActive = 1
```

**UI treatment:**
- Blue info chip on the room picker: "Subject typically requires a Computer Lab"

---

### IN-02 · Cross-Term Teacher Booking

**Type code:** `CROSS_TERM_BOOKING`  
**Severity:** Info  

**Description:**  
If two `AcademicTerms` have overlapping date ranges (i.e., `Term1.StartDate < Term2.EndDate AND Term1.EndDate > Term2.StartDate`), a teacher could be double-booked across sections belonging to different terms on the same day/time. This is an edge case that should not occur with proper term configuration but is worth detecting.

**Detection logic:**
```sql
-- First find overlapping terms
SELECT t1.Id AS Term1Id, t2.Id AS Term2Id
FROM AcademicTerms t1
JOIN AcademicTerms t2 ON t1.Id < t2.Id
WHERE t1.StartDate < t2.EndDate
  AND t1.EndDate   > t2.StartDate
-- Then apply teacher conflict check scoped to those term pairs
```

**UI treatment:**
- Informational note in Conflicts tab only when overlapping terms are detected

---

## Summary Table

| Code | Name | Tier | Severity | Affects | Block Save? |
|------|------|------|----------|---------|------------|
| `HC-01` | Teacher Double-Booked | Hard | Error | Teacher, 2+ Offerings | Yes |
| `HC-02` | Room Double-Booked | Hard | Error | Room, 2+ Offerings | Yes |
| `HC-03` | Section Schedule Overlap | Hard | Error | Section, 2+ Offerings | Yes |
| `HC-04` | Duplicate Day in Offering | Hard | Error | Single Offering + DB constraint | Yes |
| `SC-01` | Teacher Overload | Soft | Warning | Teacher, Term | No |
| `SC-02` | Room Capacity Exceeded | Soft | Warning | Room, Offering | No |
| `SC-03` | Teacher No Break | Soft | Warning | Teacher | No |
| `SC-04` | Adviser as Teacher | Soft | Warning | Section, Teacher | No |
| `SC-05` | Year Level Mismatch | Soft | Warning | Subject, Section, Curriculum | No |
| `SC-06` | Outside Operating Hours | Soft | Warning | Schedule Row | No |
| `DI-01` | Schedule Count Mismatch | Data Integrity | Error | Offering | Yes |
| `DI-02` | Hours Per Day Mismatch | Data Integrity | Warning | Offering, Schedule Row | No |
| `DI-03` | Offering with No Schedules | Data Integrity | Warning | Offering | No |
| `DI-04` | Section with No Offerings | Data Integrity | Info | Section | No |
| `DI-05` | Duplicate Subject in Section | Data Integrity | Error | Section, Offering | Yes |
| `IN-01` | Room Type Mismatch | Informational | Info | Subject, Room | No |
| `IN-02` | Cross-Term Teacher Booking | Informational | Info | Teacher, 2+ Terms | No |

---

## Overlap Detection — Time Interval Logic

All time-based conflict checks (HC-01, HC-02, HC-03, SC-03) use the **interval overlap test**. Two intervals `[A.start, A.end)` and `[B.start, B.end)` overlap if and only if:

```
A.StartTime < B.EndTime  AND  A.EndTime > B.StartTime
```

This correctly handles:
- Exact overlap (same start and end)
- Partial overlap (one starts before the other ends)
- Adjacent schedules (A ends at 10:30, B starts at 10:30 → **no overlap**, gap = 0 min)

> **Adjacent classes are NOT a conflict** by the overlap formula. Use SC-03 (`TEACHER_NO_BREAK`) separately if a minimum gap is required.

---

## Conflict Scope Boundaries

All conflict checks **must be scoped to the same `AcademicTermId`** via the parent `ClassSection.AcademicTermId`. Specifically:

- A teacher teaching in **Term 1** and **Term 2** with non-overlapping date ranges has **no conflict**, even if the days/times match.
- A room used in **Term 1** and **Term 2** (non-overlapping) has **no conflict**.
- The `ClassSections → ClassSectionSubjectOffering → ClassSchedules` chain always carries the `AcademicTermId` via `ClassSection`, so the join path is: `ClassSchedules → ClassSectionSubjectOffering → ClassSections.AcademicTermId`.

---

## Recommended Backend API Response Shape

When a conflict is detected during `POST /api/offerings/{id}/schedules` or `POST /api/offerings`, the backend should return HTTP `409 Conflict` with the following body:

```json
{
  "conflicts": [
    {
      "code": "TEACHER_DOUBLE_BOOKED",
      "severity": "error",
      "message": "Dr. Santos is already teaching CS201 on MON 09:00–10:30 in section BSCS-2A",
      "day": "MON",
      "startTime": "09:00",
      "endTime": "10:30",
      "affectedOfferings": [
        {
          "id": 42,
          "subject": { "code": "CS201", "title": "Object-Oriented Programming" },
          "section": { "id": 7, "name": "BSCS-2A" },
          "room": { "roomNumber": "101", "building": "Main Building" }
        }
      ]
    },
    {
      "code": "ROOM_DOUBLE_BOOKED",
      "severity": "error",
      "message": "Room 101 is already assigned to MATH101 on MON 09:00–11:00",
      "day": "MON",
      "startTime": "09:00",
      "endTime": "11:00",
      "affectedOfferings": [
        {
          "id": 15,
          "subject": { "code": "MATH101", "title": "Calculus 1" },
          "section": { "id": 3, "name": "BSCS-1B" },
          "room": { "roomNumber": "101", "building": "Main Building" }
        }
      ]
    }
  ]
}
```

Soft conflicts (`severity: "warning"`) should also be included in the response body but should **not** prevent a `201 Created` if no hard conflicts exist. The frontend should surface them as non-blocking toasts or Conflicts tab entries.

---

## Unresolved Policy Questions

The following conflict behaviors require stakeholder confirmation before implementation:

| # | Question | Impact |
|---|----------|--------|
| 1 | Should `HC-03` (section overlap) be a hard block or a soft warning? | Affects irregular student support |
| 2 | Should `SC-01` (teacher overload) be configurable per teacher? | Some teachers may have higher approved loads |
| 3 | What is the minimum break time for `SC-03`? 0, 10, or 15 minutes? | Affects back-to-back detection threshold |
| 4 | Is `SC-04` (adviser as teacher) allowed or discouraged? | Affects warning visibility |
| 5 | Should `IN-01` (room type mismatch) block save once `Subjects.RequiredRoomTypeId` is added? | Affects future schema changes |

> These are referenced in `docs/user-stories/ZZ-clarifications.md` as open questions.
