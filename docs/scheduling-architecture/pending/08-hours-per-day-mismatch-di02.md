# DI-02: Hours Per Day Mismatch

**Feature Type:** Data Integrity Check (Tier 3)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:396-422`  
**Phase:** 2  
**Priority:** MEDIUM  
**Status:** ⏳ Not Implemented

---

## Description

`ClassSectionSubjectOffering.HoursPerDay` declares the duration of each session (e.g., 1.5 hours), but the actual duration computed from `StartTime`–`EndTime` in the `ClassSchedules` rows differs.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

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

## Implementation Steps

1. Add detection method
2. Add to `ConflictDetectionHelper`
3. Write tests

## UI Treatment

- Amber badge on schedule row: "Declared 1.5h but scheduled for 1.0h"
- Shown in Conflicts tab as data inconsistency
