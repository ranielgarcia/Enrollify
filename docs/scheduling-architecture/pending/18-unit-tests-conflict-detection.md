# Unit Tests — ScheduleConflictDetector

**Feature Type:** Testing  
**Research Reference:** `docs\scheduling-architecture\research-document-base\REMAINING-TASKS.md:74-135`  
**Phase:** 1A  
**Priority:** HIGH  
**Status:** ⏳ Not Implemented

---

## Description

Unit tests for the `ScheduleConflictDetector` domain service to verify conflict detection logic in isolation. The service has no dependencies, so tests instantiate it directly without a DI container.

## Test File

`Enrollify.UnitTests/Core/Services/ScheduleConflictDetectorTests.cs`

## Test Cases (12+)

### HC-01: Teacher Double-Booked
- Overlapping schedules for same teacher → Error
- Back-to-back schedules (no overlap) → No conflict
- Teacher across different sections → Error

### HC-02: Room Double-Booked
- Overlapping schedules for same room → Error
- Room used in different time slots → No conflict

### HC-03: Section Overlap
- Overlapping schedules in same section → Warning
- Non-overlapping schedules in same section → No conflict

### DI-05: Duplicate Subject
- Same subject twice in same section → Error
- Same subject in different sections → No conflict

### Edge Cases
- Empty schedule list → Empty result
- Single schedule → No conflict
- Multiple simultaneous conflicts → All detected

## Run Command

```bash
dotnet test --filter "ScheduleConflictDetectorTests"
```
