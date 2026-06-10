# DI-03: Offering with No Schedules

**Feature Type:** Data Integrity Check (Tier 3)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:426-448`  
**Phase:** 2  
**Priority:** MEDIUM  
**Status:** ⏳ Not Implemented

---

## Description

A `ClassSectionSubjectOffering` exists but has zero active `ClassSchedules` rows. The subject and teacher are assigned, but no actual time has been set.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

```sql
SELECT o.Id
FROM ClassSectionSubjectOffering o
WHERE o.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM ClassSchedules cs
    WHERE cs.ClassSectionSubjectOfferingId = o.Id AND cs.IsActive = 1
  )
```

## Implementation Steps

1. Add detection method to `ScheduleConflictDetector` or `ConflictDetectionHelper`
2. Add to `ConflictType` enum
3. Write tests

## UI Treatment

- Amber "No schedules yet" badge on `OfferingCard`
- Distinct empty state in the schedule row list inside the card
- Listed as incomplete in Conflicts tab
