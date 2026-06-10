# Hard Conflicts — Teacher Double-Booked, Room Double-Booked, Section Overlap

**Feature Type:** Conflict Detection (Tier 1 — Blocking)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:36-148`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Conflict Types

| Code | Name | Severity | Block Save? |
|------|------|----------|-------------|
| HC-01 | Teacher Double-Booked | Error | Yes |
| HC-02 | Room Double-Booked | Error | Yes |
| HC-03 | Section Schedule Overlap | Warning (design decision) | No |

## Description

Three hard conflicts that represent absolute scheduling impossibilities — two physical entities cannot occupy the same space simultaneously:

- **HC-01 (Teacher Double-Booked):** The same teacher assigned to two different offerings whose schedules overlap on the same day and time window.
- **HC-02 (Room Double-Booked):** The same room assigned to two different offerings with overlapping schedules.
- **HC-03 (Section Overlap):** Two different offerings within the same section have overlapping schedules (students cannot attend both).

## Implementation Details

All three use half-open interval overlap logic: `A.StartTime < B.EndTime AND A.EndTime > B.StartTime`.

### Detection Scoping

All conflict checks are scoped to the same `AcademicTermId` via the parent `ClassSection`. Cross-term conflicts are ignored per design decision (terms are assumed to never overlap).

### Write Path (Adding Schedules)

`POST /subject-offerings/{id}/schedules`:
1. Validates request → Adds schedules to domain model → `SaveChanges()`
2. Detects conflicts for the offering via `ConflictDetectionHelper`
3. Returns HTTP 201 with `scheduleIds` + `conflicts[]`

### Read Path (Section Detail Page)

`GET /class-sections/{id}`:
1. Loads section with all offerings + schedules
2. Collects all `TeacherId` and `RoomId` values
3. Loads related schedules from same term sharing those teachers/rooms
4. Runs `ScheduleConflictDetector.DetectConflicts()` in memory
5. Returns offerings with embedded `conflicts[]` array

### Configuration

`HC-03` was implemented as **Warning** (not Error) per design decision to support irregular student enrollment where students may not take all offerings in a section.

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDetector.cs` | Core detection logic |
| `Enrollify.Core/Services/ScheduleConflictDetection/ConflictResult.cs` | Domain conflict model |
| `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDto.cs` | Domain DTO |
| `Enrollify.Application/Features/ClassSchedules/Models/ScheduleConflictDto.cs` | Application-layer DTO |
| `Enrollify.Application/Features/ClassSchedules/Models/ConflictResultDto.cs` | API response model |
| `Enrollify.Application/Features/ClassSchedules/Services/ConflictDetectionHelper.cs` | Shared helper |
| `Enrollify.Infrastructure/Repositories/ClassSectionSubjectOfferingRepository.cs` | Dapper query implementations |
| `Enrollify.WebAPI/Features/SubjectOfferings/AddScheduleToOfferingEndpoint.cs` | POST endpoint |
