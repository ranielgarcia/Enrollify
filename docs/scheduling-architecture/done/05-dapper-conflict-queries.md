# Dapper Conflict Detection Queries

**Feature Type:** Infrastructure — Repository  
**Research Reference:** `docs\scheduling-architecture\research-document-base\DAPPER-MIGRATION-SUMMARY.md`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

Five repository methods in `ClassSectionSubjectOfferingRepository` were converted from EF Core LINQ to Dapper raw SQL to overcome EF Core's restriction on querying owned entity types (`ClassSchedule` is configured as `OwnsMany` via `ClassSectionSubjectOffering`).

## Methods Implemented

| Method | Purpose | Conflict |
|--------|---------|----------|
| `HasTeacherScheduleConflictAsync()` | Checks if teacher is double-booked on given day/time | HC-01 |
| `HasRoomScheduleConflictAsync()` | Checks if room is double-booked on given day/time | HC-02 |
| `HasSectionScheduleOverlapAsync()` | Checks if section has overlapping schedules | HC-03 |
| `GetRelatedSchedulesForConflictDetectionAsync()` | Loads related schedules sharing teachers/rooms in same term | Helper |
| `GetSectionSchedulesForConflictDetectionAsync()` | Loads all schedules for a given section | Helper |

## Key Design

- Uses `IDbConnectionFactory` for connection management (following `UserContextService` pattern)
- Dapper auto-maps query results to `ScheduleConflictDto` by property name
- `TimeOnly` is handled natively by Dapper 2.x+
- Vogen value objects explicitly cast to `int` in parameter binding
- Empty list guard: passes `[-1]` for empty teacher/room lists to avoid SQL IN clause errors

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Infrastructure/Repositories/ClassSectionSubjectOfferingRepository.cs` | Dapper query implementations |
| `Enrollify.Application/Features/ClassSectionSubjectOfferings/IClassSectionSubjectOfferingRepository.cs` | Interface with 5 new methods |
