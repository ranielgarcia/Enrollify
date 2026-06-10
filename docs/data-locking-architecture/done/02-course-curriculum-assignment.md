# Course-Curriculum Assignment & Binding

## Overview
The `CourseCurriculumAssignment` entity establishes an immutable many-to-one binding between a course and a curriculum for a specific academic year entry point. This ensures that all students entering a course in a given year follow the same curriculum, even if newer curriculum versions are later approved.

## Status
✅ **FULLY IMPLEMENTED**

## Current Implementation

### Entity Definition
- **File:** `Enrollify.Core/Aggregates/CourseCurriculumAssignmentAggregate/CourseCurriculumAssignment.cs` (lines 1-55)
- **Key Properties:**
  - `CourseId` - The program/course
  - `EntryAcademicYearId` - The year students entered the course
  - `CurriculumId` - The curriculum version assigned
  - `IsActive` - Boolean flag
  - `CreatedDate` - Timestamp

### Design Principle: Immutable
- **Lines 11-14:** Explicit comment: "DO NOT ALLOW ANY UPDATES"
- No `Update()` methods exist on the entity
- Once created, the binding is permanent
- The record can be deactivated via `IsActive = false`, but the reference is never changed

## Automatic Synchronization

### Primary Sync Command
- **File:** `Enrollify.Application/Features/CourseCurriculumAssignments/Commands/SyncCourseCurriculumAssignments.cs` (lines 1-144)
- **Entry Point:** Called by both:
  1. `SyncCourseCurriculumAssignmentsForCurrentAcademicYear` (40-line wrapper)
  2. Manual invocation via WebAPI endpoint (if needed)

### Sync Logic (Lines 40-129)

#### Step 1: Retrieve Active Curriculums (Line 63-67)
```csharp
var curricula = await _curriculumRepository.ListAsync(
    new GetActiveCurriculumsByCourseAndAcademicYearSpec(courseId, academicYearId),
    cancellationToken);
```
- Only `Active` curriculums are included
- Filters by course and academic year

#### Step 2: Skip Courses with Existing Active Assignments (Lines 76-82)
```csharp
if (existing?.Curriculum?.StatusId == CurriculumStatusEnum.Active)
{
    _logger.LogInformation("Skipping assignment for {CourseId} - {CurriculumId}. Active curriculum already assigned.", courseId, existing.Curriculum.Id);
    continue;
}
```
- **Critical Guard:** If a curriculum is already assigned and active, the sync does NOT update it
- Prevents retroactive curriculum changes to existing cohorts
- Ensures stability: once a cohort is bound to a curriculum, they stay with it

#### Step 3: Create or Update Assignment (Lines 85-104)
- Creates new assignment if none exists
- Updates inactive assignment if it exists
- Sets `IsActive = true`

### Event-Driven Trigger

When `ApproveCurriculum` transitions a curriculum to `Active`:
- **Event Raised:** `CurriculumApprovedEvent`
- **Handler:** `CurriculumApprovedEventHandler` → `SyncCourseCurriculumAssignmentsForCurrentAcademicYear`
- **Result:** Course-curriculum binding is automatically established
- **File:** `Enrollify.Application/Features/Curriculums/Events/CurriculumApprovedEventHandler.cs`

## Guard Rules

| Scenario | Behavior |
|----------|----------|
| New Active curriculum approved for course X in AY 2024 | Sync creates `CourseCurriculumAssignment(X, 2024, NewCurriculum)` |
| Another curriculum becomes Active for same course | Sync skips update; existing binding unchanged |
| New cohort enters in AY 2025 | New assignment created for AY 2025 (different from AY 2024 cohort) |
| Class sections created after assignment | They reference the assigned curriculum for that cohort |

## Why This Design Works

### 1. Cohort Isolation
- Each cohort (defined by course + entry year) gets exactly one curriculum
- Students entering in 2024 follow 2024 curriculum
- Students entering in 2025 follow 2025 curriculum (if updated)

### 2. Immutability & Consistency
- Once bound, a cohort's curriculum never changes
- Retroactive changes are prevented by the "skip if active" guard
- Student transcripts remain stable

### 3. Event-Driven Automation
- Admin approves a curriculum
- Event fires automatically
- Sync creates the binding
- No manual steps required

## Related Concepts

- **ClassSection Creation:** Uses the binding to select which curriculum to reference
- **Curriculum Approval:** Triggers the sync event
- **CourseCurriculumAssignmentAggregate Root:** Is the aggregate; the repository is `ICourseCurriculumAssignmentRepository`

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/CourseCurriculumAssignments/Commands/SyncCourseCurriculumAssignmentsTests.cs`
- **Integration Tests:** Verify event handler triggers correctly on curriculum approval
- **Test Scenarios:**
  - Sync creates new assignment for inactive course
  - Sync skips update when active curriculum already assigned
  - Multiple curricula for same course handled correctly

## Endpoints

- **POST** `/api/course-curriculum-assignments/sync` - Manual sync trigger (rare)
- Typically invoked automatically via event handler, not manually

## Notes

- ⚠️ The immutability is enforced by design (no update methods), not by database constraints
- Records marked `IsActive = false` are soft-deactivated but retained for audit
- The "skip if active" logic (lines 76-82) is the critical safety mechanism
