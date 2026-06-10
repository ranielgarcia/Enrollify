# SC-01: Teacher Workload Overload

**Feature Type:** Soft Conflict (Tier 2 — Warning)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:181-223`  
**Phase:** 3  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A teacher's total scheduled teaching hours across all offerings in the same `AcademicTermId` exceeds the institutional maximum teaching load. This doesn't prevent scheduling but is a policy violation that should be flagged.

**Severity:** Warning  
**Block Save:** No

## Prerequisites

- Add `MaxTeacherHoursPerWeek` to `AcademicCoreSettings` (default: 18 hours)

## Detection Logic (SQL)

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

## Implementation Steps

1. Add `MaxTeacherHoursPerWeek` property to `AcademicCoreSettings`
2. Add Dapper query to `IClassSectionSubjectOfferingRepository`
3. Add detection logic to `ScheduleConflictDetector`
4. Add to `ConflictDetectionHelper`
5. Write unit + integration tests

## UI Treatment

- Amber badge on the teacher picker: "Current load: 22 / 18 hrs"
- Amber `ConflictCard` in Conflicts tab
