# IN-01: Room Type Mismatch

**Feature Type:** Informational Issue (Tier 4)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:512-536`  
**Phase:** Future  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A subject that is typically held in a specific room type (e.g., a Computer Laboratory for programming subjects) is assigned to a room of a different type (e.g., a regular lecture room).

**Severity:** Info  
**Block Save:** No

## Prerequisite

This conflict requires a `RequiredRoomTypeId` field on the `Subject` table, which does **not currently exist** in the schema. This is a future enhancement once room-type requirements are modeled on subjects.

## Detection Logic (Future)

```sql
SELECT o.Id
FROM ClassSectionSubjectOffering o
JOIN Subjects sub ON sub.Id = o.SubjectId
JOIN Rooms r ON r.Id = o.RoomId
WHERE sub.RequiredRoomTypeId IS NOT NULL
  AND sub.RequiredRoomTypeId <> r.RoomTypeId
  AND o.IsActive = 1
```

## Implementation Steps

1. Add `RequiredRoomTypeId` to `Subject` aggregate (schema + domain change)
2. Add detection logic
3. Write tests

## UI Treatment

- Blue info chip on the room picker: "Subject typically requires a Computer Lab"
