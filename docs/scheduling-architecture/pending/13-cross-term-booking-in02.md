# IN-02: Cross-Term Teacher Booking

**Feature Type:** Informational Issue (Tier 4)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:538-560`  
**Phase:** Future  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

If two `AcademicTerms` have overlapping date ranges (i.e., `Term1.StartDate < Term2.EndDate AND Term1.EndDate > Term2.StartDate`), a teacher could be double-booked across sections belonging to different terms on the same day/time. This is an edge case that should not occur with proper term configuration but is worth detecting.

**Severity:** Info  
**Block Save:** No

## Detection Logic

```sql
-- First find overlapping terms
SELECT t1.Id AS Term1Id, t2.Id AS Term2Id
FROM AcademicTerms t1
JOIN AcademicTerms t2 ON t1.Id < t2.Id
WHERE t1.StartDate < t2.EndDate
  AND t1.EndDate > t2.StartDate
-- Then apply teacher conflict check scoped to those term pairs
```

## Design Note

Cross-term conflicts are explicitly excluded from the current design scope. The system assumes terms never overlap. This detection is only needed if the institution runs overlapping terms (e.g., regular term + intersession with overlapping dates).

## Implementation Steps

1. Only if overlapping terms are institutionally possible
2. Add term overlap query
3. Extend conflict detection to cover cross-term scenarios
