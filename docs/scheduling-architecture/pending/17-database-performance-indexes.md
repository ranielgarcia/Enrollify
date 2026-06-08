# Database Performance Indexes

**Feature Type:** Infrastructure — Database  
**Research Reference:** `docs\scheduling-architecture\research-document-base\database-index-recommendations.md`  
**Phase:** 5 (Conditional)  
**Priority:** CONDITIONAL  
**Status:** ⏸️ On Hold

---

## Description

Recommended database indexes to optimize conflict detection queries. Implementation is conditional on measured query performance exceeding 50ms.

## Trigger

Add indexes if conflict detection queries on 10,000+ schedule rows exceed 50ms.

## Recommended Indexes

### Index 1: ClassSchedules — DayOfWeek + Time Range

```sql
CREATE INDEX IX_ClassSchedules_DayOfWeek_Time
ON ClassSchedules (DayOfWeek, StartTime, EndTime)
WHERE IsActive = 1;
```

### Index 2: ClassSectionSubjectOffering — TeacherId

```sql
CREATE INDEX IX_ClassSectionSubjectOffering_TeacherId
ON ClassSectionSubjectOffering (TeacherId, ClassSectionId)
WHERE IsActive = 1 AND TeacherId IS NOT NULL;
```

### Index 3: ClassSectionSubjectOffering — RoomId

```sql
CREATE INDEX IX_ClassSectionSubjectOffering_RoomId
ON ClassSectionSubjectOffering (RoomId, ClassSectionId)
WHERE IsActive = 1 AND RoomId IS NOT NULL;
```

## Implementation Checklist

- [ ] Generate 15,000+ test schedule rows with realistic teacher/room distribution
- [ ] Measure baseline query time for HC-01, HC-02, HC-03 queries
- [ ] If any query exceeds 50ms: add indexes incrementally
- [ ] Re-measure and document improvement
