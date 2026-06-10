# ScheduleConflictDetector Domain Service

**Feature Type:** Core Service  
**Research Reference:** `docs\scheduling-architecture\research-document-base\IMPLEMENTATION-SUMMARY.md:27-29`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

The `ScheduleConflictDetector` is the core domain service responsible for detecting all scheduling conflicts. It uses a grouping strategy to reduce O(n²) complexity — grouping by `(entity, dayOfWeek)` before running pairwise comparisons.

## Detection Capabilities

| Conflict | Grouping Key |
|----------|-------------|
| HC-01: Teacher Double-Booked | `(TeacherId, DayOfWeek)` |
| HC-02: Room Double-Booked | `(RoomId, DayOfWeek)` |
| HC-03: Section Overlap | `(SectionId, DayOfWeek)` |
| DI-05: Duplicate Subject | `(SubjectId, SectionId)` |

## Algorithm

1. Accept list of flat `ScheduleConflictDto` records
2. Group by entity + day (e.g., `(TeacherId, DayOfWeek)`)
3. Within each group (typically 2–15 rows), run O(n²) pairwise half-open interval overlap check
4. Return list of `ConflictResult` objects with type, severity, message, and affected offerings

## Configuration

The service reads from `AcademicCoreSettings` for:
- `MinimumTeacherBreakMinutes` (default: 10) — used by future SC-03 detection
- `EarliestClassStartTime` (default: 07:00) — used by future SC-06 detection
- `LatestClassEndTime` (default: 21:00) — used by future SC-06 detection

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDetector.cs` | Core detection logic |
| `Enrollify.Core/Services/ScheduleConflictDetection/ConflictResult.cs` | Domain model |
| `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDto.cs` | Input DTO |
| `Enrollify.Core/AcademicCoreSettings.cs` | Configuration |
