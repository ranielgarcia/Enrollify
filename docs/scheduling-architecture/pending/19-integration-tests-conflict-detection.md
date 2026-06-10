# Integration Tests — Conflict Detection

**Feature Type:** Testing  
**Research Reference:** `docs\scheduling-architecture\research-document-base\REMAINING-TASKS.md:138-258`  
**Phase:** 1A  
**Priority:** HIGH  
**Status:** ⏳ Not Implemented

---

## Description

Integration tests for conflict detection using real database (Testcontainers) and full application stack. Tests both Application layer (direct Mediator) and WebAPI layer (HTTP).

## Application Layer Tests

**File:** `Enrollify.IntegrationTests/_Tests/Application/ClassSchedules/ConflictDetectionTests.cs`  
**Collection:** `[Collection("Application")]`

### Test Cases

- Add schedules with teacher conflict → Returns conflicts in response
- Get section by ID with conflicts → Conflicts embedded in offerings
- ConflictDetectionHelper detects all 4 conflict types → All detected

## WebAPI Layer Tests

**File:** `Enrollify.IntegrationTests/_Tests/WebApi/ClassSchedules/AddScheduleEndpointTests.cs`  
**Collection:** `[Collection("WebApi")]`

### Test Cases

- POST schedules with conflicts → HTTP 201 + conflicts array
- POST schedules without conflicts → HTTP 201 + empty conflicts
- GET section detail with conflicts → HTTP 200 + conflicts in offerings

## Run Commands

```bash
dotnet test --filter "ConflictDetectionTests"
dotnet test --filter "AddScheduleEndpointTests"
```
