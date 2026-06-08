# Section Detail Endpoint — Embedded Conflicts

**Feature Type:** API Endpoint — Read Path  
**Research Reference:** `docs\scheduling-architecture\research-document-base\IMPLEMENTATION-SUMMARY.md:147-171`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

The section detail endpoint `GET /class-sections/{id}` returns offerings with embedded `conflicts[]` arrays, computed on the server during section load.

## Read Path Flow

1. Load the section with all offerings + schedules via specification
2. Collect all `TeacherId` and `RoomId` values from the section's offerings
3. Query related schedules from the **same academic term** that share those teachers or rooms
4. Run `ScheduleConflictDetector.DetectConflicts()` in memory on the combined dataset
5. Group conflicts by offering ID
6. Embed `conflicts[]` in each offering's DTO

## Response Shape

```json
{
  "id": 5,
  "name": "BSCS 1-A",
  "offerings": [
    {
      "id": 42,
      "subject": { "code": "CS101", "title": "Intro to CS" },
      "schedules": [
        { "id": 1, "dayOfWeek": "MON", "startTime": "09:00", "endTime": "10:30" }
      ],
      "conflicts": [
        {
          "type": "TEACHER_DOUBLE_BOOKED",
          "severity": "error",
          "message": "..."
        }
      ]
    }
  ]
}
```

## Performance

The in-memory dataset is kept small (typically 10–80 rows) by only querying schedules sharing teachers/rooms with the section's offerings, scoped to the same term.

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Application/Features/ClassSections/Queries/GetClassSectionByIdQuery.cs` | Query handler with conflict detection |
| `Enrollify.Application/Features/ClassSectionSubjectOfferings/DTOs/ClassSectionSubjectOfferingDto.cs` | DTO with `Conflicts` property |
