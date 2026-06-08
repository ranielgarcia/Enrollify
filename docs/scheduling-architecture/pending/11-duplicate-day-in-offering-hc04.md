# HC-04: Duplicate Day Within Offering (Application-Layer Check)

**Feature Type:** Hard Conflict (Tier 1)  
**Research Reference:** `docs\scheduling-architecture\research-document-base\class-scheduling-conflicts.md:150-172`  
**Phase:** 1 (partial)  
**Priority:** HIGH (for full implementation)  
**Status: ⚠️ Partially Implemented**

---

## Description

Attempting to create two `ClassSchedules` rows for the same `ClassSectionSubjectOfferingId` with the same `DayOfWeek`. A subject cannot appear twice on the same day under the same offering record.

**Severity:** Error  
**Block Save:** Yes

## Current State

| Layer | Status |
|-------|--------|
| Database constraint (`UQ_ClassSchedules_Offering_Day`) | ✅ Implemented |
| Domain check (`ClassSectionSubjectOffering.AddClassSchedule()`) | ✅ Implemented |
| API/user-friendly error message | ❌ Not implemented |
| Disabled days in frontend dropdown | ❌ Not implemented |

## What's Missing

1. **Application-layer validation:** The check is enforced at DB + domain level but should be caught earlier in the API layer to provide a user-friendly error rather than a constraint violation exception.
2. **Frontend UX:** The `DayOfWeek` select dropdown in `ScheduleRowFormDrawer` should disable already-used days.

## Implementation Steps

1. Add FluentValidation rule to prevent duplicate day before reaching domain/DB
2. Return clear error message: "This offering already has a schedule on [day]"
3. Frontend: disable used days in the day selector
