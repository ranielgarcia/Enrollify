# DI-01: Schedule Count Mismatch

**Feature Type:** Data Integrity Check (Tier 3)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:366-394`  
**Phase:** 2  
**Priority:** MEDIUM  
**Status:** ⏳ Not Implemented

---

## Description

`ClassSectionSubjectOffering.DaysPerWeek` declares the number of days per week the subject meets (e.g., 3 for MWF), but the actual number of active `ClassSchedules` rows for that offering differs.

**Severity:** Error  
**Block Save:** Yes

## Detection Logic

```sql
SELECT o.Id, o.DaysPerWeek, COUNT(cs.Id) AS ActualScheduledDays
FROM ClassSectionSubjectOffering o
LEFT JOIN ClassSchedules cs ON cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
WHERE o.IsActive = 1
GROUP BY o.Id, o.DaysPerWeek
HAVING COUNT(cs.Id) <> o.DaysPerWeek
   AND o.DaysPerWeek IS NOT NULL
```

## Implementation Steps

1. Add `OfferingWithoutSchedule` to `ConflictType` enum
2. Add detection method to `ScheduleConflictDetector` or `ConflictDetectionHelper`
3. Write tests

## UI Treatment

- Orange badge on `OfferingCard`: "2 / 3 days scheduled"
- Listed in Conflicts tab as an incomplete offering
