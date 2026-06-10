# SC-06: Outside Operating Hours

**Feature Type:** Soft Conflict (Tier 2 — Warning)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:340-358`  
**Phase:** 3  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A `ClassSchedules` row has a `StartTime` before the institution's earliest allowed time (e.g., before 07:00) or an `EndTime` after the latest allowed time (e.g., after 21:00). The Weekly Grid is designed with slots from 07:00–21:00; schedules outside this range will not render properly.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

Configuration already exists in `AcademicCoreSettings`:
- `EarliestClassStartTime` (default: 07:00)
- `LatestClassEndTime` (default: 21:00)

```csharp
if (schedule.StartTime < settings.EarliestClassStartTime ||
    schedule.EndTime > settings.LatestClassEndTime)
    → OUTSIDE_OPERATING_HOURS
```

## Implementation Steps

1. Add detection to `ScheduleConflictDetector` (simple comparison, no DB query needed)
2. Add to `ConflictDetectionHelper`
3. Write tests

## UI Treatment

- Amber warning in `ScheduleRowFormDrawer` time fields
- Note in Conflicts tab if existing data violates this
