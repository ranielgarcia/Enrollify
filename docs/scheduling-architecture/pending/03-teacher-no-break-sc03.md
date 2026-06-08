# SC-03: Teacher No Break (Insufficient Turnaround)

**Feature Type:** Soft Conflict (Tier 2 — Warning)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:249-282`  
**Phase:** 3  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A teacher has two offerings scheduled consecutively on the same day with zero (or insufficient) break time between them. For example, a class ending at 10:30 and the next starting at 10:30 in a different building.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

Uses `AcademicCoreSettings.MinimumTeacherBreakMinutes` (default: 10).

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

## Implementation Steps

1. Add detection method to `ScheduleConflictDetector` (in-memory: group by `(TeacherId, DayOfWeek)`, sort by time, check gaps)
2. Add to `ConflictDetectionHelper`
3. Write tests

## UI Treatment

- Amber warning in teacher availability hint
- Informational note in Conflicts tab
