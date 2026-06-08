# Curriculum Status Enum & Active Lock

## Overview
The `CurriculumStatusEnum` defines a 4-state lifecycle for curriculum records: **Draft → Active → PhaseOut → Archived**. Once a curriculum transitions to **Active**, all mutations are blocked, effectively freezing the curriculum content.

## Status
✅ **FULLY IMPLEMENTED**

## Current Implementation

### Enum Definition
- **File:** `Enrollify.Core/Constants/CurriculumStatusEnum.cs` (lines 1-13)
- **States:**
  - `Draft (1)` - Initial state, all mutations allowed
  - `Active (2)` - Frozen, no updates permitted
  - `PhaseOut (3)` - Defined but unreachable (no command exists)
  - `Archived (4)` - Defined but unreachable (no command exists)

### Guard Enforcement

#### 1. UpdateCurriculum Guard
- **File:** `Enrollify.Application/Features/Curriculums/Commands/UpdateCurriculum.cs` (lines 41-44)
- **Guard:** Blocks all updates when `StatusId == Active`
- **Result Type:** `Result.Forbidden()`
- **Error Message:** "Cannot update an active curriculum"

#### 2. SaveCurriculumContent Guard
- **File:** `Enrollify.Application/Features/Curriculums/Commands/SaveCurriculumContent.cs` (lines 56-59)
- **Guard:** Blocks content modifications when `StatusId == Active`
- **Result Type:** `Result.Forbidden()`
- **Prevents:** Adding/removing/updating CurriculumSubjects within an active curriculum

#### 3. ApproveCurriculum Guard
- **File:** `Enrollify.Application/Features/Curriculums/Commands/ApproveCurriculum.cs` (lines 36-39)
- **Guard:** Blocks re-approval if already `Active`
- **Result Type:** `Result.Forbidden()`
- **Prevents:** Double-transition or state inconsistency

## Event-Driven Sync

### CurriculumApprovedEvent
When a curriculum transitions to `Active`, it raises `CurriculumApprovedEvent`, which automatically triggers:
- **Event Handler:** `SyncCourseCurriculumAssignmentsForCurrentAcademicYear`
- **Effect:** Automatically creates `CourseCurriculumAssignment` records binding the course → curriculum for the current academic year
- **File:** `Enrollify.Application/Features/Curriculums/Events/CurriculumApprovedEventHandler.cs`

## Guard Rules Summary

| Operation | Condition | Result |
|-----------|-----------|--------|
| `UpdateCurriculum` | StatusId == Active | ❌ Forbidden |
| `SaveCurriculumContent` | StatusId == Active | ❌ Forbidden |
| `ApproveCurriculum` | StatusId == Active | ❌ Forbidden |

## Why This Works

1. **Content Freezing:** Once a curriculum is approved and in use by class sections, its structure (subjects, units, prerequisites) cannot be changed retroactively.
2. **CourseCurriculumAssignment Binding:** The `Active` status signals that the curriculum is ready for use, and the event-driven sync ensures the course-curriculum mapping is established.
3. **Prevents Stale Data:** Students enrolled under an `Active` curriculum will always see the exact subject/unit structure that was approved.

## Related Entities

- **CourseCurriculumAssignment:** Automatically synced when curriculum becomes Active
- **ClassSection:** Can only be created using Active curriculums
- **CurriculumSubject:** Content frozen when parent curriculum is Active

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/Curriculums/Commands/UpdateCurriculumTests.cs`
- **Integration Tests:** WebAPI endpoint tests for curriculum approval flow

## Notes

- ⚠️ **PhaseOut** and **Archived** states are defined but have no commands to reach them (see `02-curriculum-phase-out-and-archive.md`)
- The Active lock is the primary data-integrity mechanism for the curriculum lifecycle
