# Duplicate Subject in Section (DI-05)

**Feature Type:** Conflict Detection (Tier 3 — Data Integrity)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:479-501`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

Detects when the same `SubjectId` is assigned to the same `ClassSectionId` more than once in active offerings. A section cannot have duplicate subject assignments — this creates ambiguity for enrollment and scheduling.

**Severity:** Error  
**Block Save:** Yes

## Detection Logic

```sql
SELECT o.SubjectId, o.ClassSectionId, COUNT(*) AS DuplicateCount
FROM ClassSectionSubjectOffering o
WHERE o.IsActive = 1
GROUP BY o.SubjectId, o.ClassSectionId
HAVING COUNT(*) > 1
```

## Implementation

Detection happens in `ScheduleConflictDetector.DetectConflicts()` by grouping schedules by `(SubjectId, SectionId)` and flagging groups with count > 1. This is computed alongside HC-01/HC-02/HC-03 conflicts during both write and read paths.

## UI Treatment

- Error toast: "CS101 is already assigned to this section"
- Listed in Conflicts tab as a data integrity error
- Application-layer validation when adding an offering

## Key Files

| File                                                                                                        | Purpose                         |
| ----------------------------------------------------------------------------------------------------------- | ------------------------------- |
| `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDetector.cs`                             | Detection logic                 |
| `Enrollify.Application/Features/ClassSectionSubjectOfferings/Commands/CreateClassSectionSubjectOffering.cs` | Prevents creation of duplicates |

## Notes

No DB-level unique constraint prevents duplicate `(SubjectId, ClassSectionId)` among active offerings. Enforcement is entirely application-layer via the conflict detector.
