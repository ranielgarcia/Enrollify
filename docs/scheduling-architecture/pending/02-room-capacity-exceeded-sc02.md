# SC-02: Room Capacity Exceeded

**Feature Type:** Soft Conflict (Tier 2 — Warning)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:226-247`  
**Phase:** 3  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

The number of students expected in an offering exceeds the assigned room's seating capacity. `MaxNumberOfStudents` (or `ClassSection.StudentCapacity` as fallback) > `Room.Capacity`.

**Severity:** Warning  
**Block Save:** No

## Detection Logic

```csharp
var effectiveStudents = offering.MaxNumberOfStudents ?? section.StudentCapacity;
if (effectiveStudents > room.Capacity) → ROOM_CAPACITY_EXCEEDED
```

## Why Warning (Not Error)

The `MaxNumberOfStudents` field is intentionally a soft rule — it allows overriding the room's stated capacity for exceptional cases.

## Implementation Steps

1. Add room capacity check in `ConflictDetectionHelper`
2. Include room capacity data in `GetRelatedSchedulesForConflictDetectionAsync` query
3. Add detection to `ScheduleConflictDetector`
4. Write tests

## UI Treatment

- Amber inline hint below the Room picker: "Room capacity: 30 · Expected students: 45"
- Amber `ConflictCard` in Conflicts tab
