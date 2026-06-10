# SC-04: Adviser as Teacher

**Feature Type:** Soft Conflict (Tier 2 — Warning)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:284-304`  
**Phase:** 3  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

The teacher assigned as the `ClassSection.AdviserId` is also assigned as the `TeacherId` of one or more offerings within that same section. This may be institutionally allowed (a homeroom adviser who also teaches) but is unusual enough to warrant a warning.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

```sql
SELECT s.AdviserId, o.TeacherId
FROM ClassSectionSubjectOffering o
JOIN ClassSections s ON s.Id = o.ClassSectionId
WHERE o.TeacherId = s.AdviserId AND o.IsActive = 1
```

## Implementation Steps

1. Add detection to `ScheduleConflictDetector` or `ConflictDetectionHelper`
2. Write tests

## Unresolved Question

Is this actually a problem or is it standard practice? Requires stakeholder confirmation. Some institutions expect advisers to teach their own sections.

## UI Treatment

- Amber info chip on the teacher picker when the selected teacher matches the section adviser
- Not shown in Conflicts tab by default (informational only)
