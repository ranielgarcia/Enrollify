# DI-04: Section with No Offerings

**Feature Type:** Data Integrity Check (Tier 3)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:452-473`  
**Phase:** 2  
**Priority:** MEDIUM  
**Status:** ⏳ Not Implemented

---

## Description

A `ClassSection` exists with zero active `ClassSectionSubjectOffering` records. The section has no subjects assigned for the term.

**Severity:** Info  
**Block Save:** No

## Detection Logic

```sql
SELECT s.Id, s.Name
FROM ClassSections s
WHERE s.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM ClassSectionSubjectOffering o
    WHERE o.ClassSectionId = s.Id AND o.IsActive = 1
  )
```

## Implementation Steps

1. Add detection method
2. Add to `ConflictType` enum
3. Write tests

## UI Treatment

- Empty state indicator on the Section Detail page Offerings tab
- Informational badge on the Sections list: "0 offerings"
