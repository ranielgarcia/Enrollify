# SC-05: Year Level Mismatch

**Feature Type:** Soft Conflict (Tier 2 — Warning)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:306-338`  
**Phase:** 3  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A subject is assigned to a `ClassSection` whose `YearLevel` does not match the subject's expected `YearLevel` in the curriculum (`CurriculumSubjects.YearLevel`). For example, a 3rd-year subject offered to a 1st-year section.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

```sql
SELECT o.SubjectId, cs.IntendedYearLevel AS SectionYearLevel,
       cu.YearLevel AS CurriculumYearLevel
FROM ClassSectionSubjectOffering o
JOIN ClassSections cs ON cs.Id = o.ClassSectionId
JOIN CurriculumSubjects cu ON cu.SubjectId = o.SubjectId
WHERE cu.CurriculumId = cs.CurriculumId
  AND cu.YearLevel <> cs.IntendedYearLevel
  AND o.IsActive = 1
```

## Implementation Steps

1. Add Dapper query to repository
2. Add detection to `ScheduleConflictDetector` or `ConflictDetectionHelper`
3. Write tests

## UI Treatment

- Amber advisory in the subject picker when the year level doesn't match
- Amber `ConflictCard` in Conflicts tab
