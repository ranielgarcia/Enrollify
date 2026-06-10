# ConflictDetectionHelper (Application Layer)

**Feature Type:** Shared Service  
**Research Reference:** `docs\scheduling-architecture\research-document-base\IMPLEMENTATION-SUMMARY.md:40-48`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

The `ConflictDetectionHelper` is an application-layer shared service that:
1. Loads the section's `AcademicTermId`
2. Collects all `TeacherId` and `RoomId` values from the section's offerings
3. Queries related schedules from the same term (excluding the current section)
4. Combines the section's own schedules with related schedules
5. Calls `ScheduleConflictDetector.DetectConflicts()` for in-memory detection
6. Returns grouped `ConflictResultDto` lists mapped to each offering

## Usage

| Consumer | Trigger |
|----------|---------|
| `AddMultipleSchedulesToOffering` command handler | After saving schedules, detects conflicts |
| `GetOfferingsByClassSectionIdQuery` handler | On section detail page load, embeds conflicts |
| `RemoveScheduleFromOffering` command handler | After removing schedule, updates conflicts |

## Design

The helper ensures the in-memory dataset is small (typically 10–80 rows) by only querying schedules that share teachers or rooms with the current section's offerings, scoped to the same academic term.

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Application/Features/ClassSchedules/Services/ConflictDetectionHelper.cs` | Shared helper |
| `Enrollify.Application/Features/ClassSchedules/Models/ConflictResultDto.cs` | DTO for API responses |
