# ClassSection State Machine & Transitions

## Overview
The `ClassSection` aggregate implements a complete 6-state lifecycle state machine that controls when class sections can be edited, when data is snapshotted, when students can enroll, and when grades become final.

## Status
✅ **FULLY IMPLEMENTED**

## State Diagram

```
Draft ──OpenForEnrollment()──► Open ──LockEnrollment()──► Locked ──Activate()──► Active ──Complete()──► Completed
  │                              │                          │                       │
  └──────────Cancel()────────────┴──────────────────────────┴───────────────────────┘
```

## States Definition

- **File:** `Enrollify.Core/Constants/ClassSectionStatusEnum.cs` (lines 1-31)
- **Values:**
  - `Draft (1)` - Section being created and configured
  - `Open (2)` - Data snapshotted; enrollment window open
  - `Locked (3)` - Enrollment closed; term about to begin
  - `Active (4)` - Term in progress; grades being recorded
  - `Completed (5)` - Term ended; grades final
  - `Cancelled (6)` - Section cancelled

## Transition Methods (All in ClassSection.cs)

### 1. OpenForEnrollment() → Draft to Open
- **File:** `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs` (lines 151-159)
- **Guard:** `StatusId == Draft`
- **Action on Guard Failure:** `Result.Invalid()`
- **Side Effects:**
  - Data snapshots are captured (subject units, titles, etc.)
  - Validates all offerings have required teacher/room assignments
  - Raises `ClassSectionOpenedForEnrollmentEvent`
- **After Transition:**
  - ✅ Students can view and enroll in the section
  - ❌ No new subject offerings can be added
  - ❌ Curriculum/Course/AcademicTerm cannot change
  - ❌ Subject offering details cannot be modified

### 2. LockEnrollment() → Open to Locked
- **File:** `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs` (lines 161-169)
- **Guard:** `StatusId == Open`
- **Action on Guard Failure:** `Result.Invalid()`
- **Business Meaning:** Enrollment window closes (no new enrollments accepted)
- **Side Effects:** Raises `ClassSectionEnrollmentLockedEvent`
- **After Transition:**
  - ❌ No new `Enrollment` records can be created
  - ✅ Existing enrollments can be processed
  - ✅ Teachers can prepare for term

### 3. Activate() → Locked to Active
- **File:** `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs` (lines 171-179)
- **Guard:** `StatusId == Locked`
- **Action on Guard Failure:** `Result.Invalid()`
- **Business Meaning:** Term has begun; classes are meeting
- **Side Effects:** Raises `ClassSectionActivatedEvent`
- **After Transition:**
  - ✅ Teachers record grades
  - ✅ Attendance tracking proceeds
  - ❌ No section structure changes allowed

### 4. Complete() → Active to Completed
- **File:** `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs` (lines 181-189)
- **Guard:** `StatusId == Active`
- **Action on Guard Failure:** `Result.Invalid()`
- **Business Meaning:** Term has ended; all grades are final
- **Side Effects:** Raises `ClassSectionCompletedEvent`
- **After Transition:**
  - ✅ Archived and immutable
  - ✅ Historical record preserved for transcripts
  - ❌ No changes allowed

### 5. Cancel() → Draft/Open/Locked to Cancelled
- **File:** `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs` (lines 191-200)
- **Guard:** `StatusId ∈ {Draft, Open, Locked}`
- **Action on Guard Failure:** `Result.Invalid()`
- **Parameter:** `reason` (string - why section was cancelled)
- **Business Meaning:** Section is withdrawn before the term starts
- **Side Effects:** Raises `ClassSectionCancelledEvent`
- **After Transition:**
  - ✅ Students are notified of cancellation
  - ✅ Enrollments can be rolled back
  - ❌ Cannot be reactivated

## Mutation Guards in Domain Entity

### UpdateAdviser()
- **Guard:** `StatusId == Draft || StatusId == Open`
- **Prevents:** Changing adviser after enrollment is locked

### UpdateCurriculum() / UpdateCourse() / UpdateAcademicTerm()
- **Guard:** `StatusId == Draft` (only)
- **Prevents:** Changing core structural data after section goes Open
- **File:** `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs`

### AddSubjectOffering() / RemoveSubjectOffering()
- **Guard:** `StatusId == Draft` (only)
- **Prevents:** Adding/removing offerings after going Open
- **Reason:** Once Open, subject structure is frozen and snapshotted

## Application Layer Commands

Each domain transition has a corresponding CQRS command handler:

| Domain Method | Command Class | File Path | Handler |
|---|---|---|---|
| `OpenForEnrollment()` | `OpenClassSectionForEnrollment` | `.../Commands/OpenClassSectionForEnrollment.cs` | Calls domain method |
| `LockEnrollment()` | `LockClassSectionEnrollment` | `.../Commands/LockClassSectionEnrollment.cs` | Calls domain method |
| `Activate()` | `ActivateClassSection` | `.../Commands/ActivateClassSection.cs` | Calls domain method |
| `Complete()` | `CompleteClassSection` | `.../Commands/CompleteClassSection.cs` | Calls domain method |
| `Cancel()` | `CancelClassSection` | `.../Commands/CancelClassSection.cs` | Calls domain method |

**Pattern:** Each command:
1. Loads the section by ID
2. Calls the domain transition method
3. Catches `InvalidOperationException` or returns `Result.Invalid`
4. Persists the updated section
5. Returns `Result<ClassSectionDto>` with new state

## WebAPI Endpoints

Each transition has a dedicated FastEndpoints endpoint:

| Endpoint | Method | Path | Input |
|---|---|---|---|
| `OpenClassSectionForEnrollmentEndpoint` | POST | `/api/class-sections/{id}/open` | `{id}` |
| `LockClassSectionEnrollmentEndpoint` | POST | `/api/class-sections/{id}/lock` | `{id}` |
| `ActivateClassSectionEndpoint` | POST | `/api/class-sections/{id}/activate` | `{id}` |
| `CompleteClassSectionEndpoint` | POST | `/api/class-sections/{id}/complete` | `{id}` |
| `CancelClassSectionEndpoint` | POST | `/api/class-sections/{id}/cancel` | `{ reason: string }` |

## Event-Driven Handlers

Each transition raises a corresponding domain event that can trigger side effects:

- **ClassSectionOpenedForEnrollmentEvent** - Could trigger email to students
- **ClassSectionEnrollmentLockedEvent** - Could trigger enrollment summary report
- **ClassSectionActivatedEvent** - Could trigger grade entry permission grants
- **ClassSectionCompletedEvent** - Could trigger transcript generation
- **ClassSectionCancelledEvent** - Could trigger enrollment rollback and refunds

## Guard Rules Summary

| Guard | Condition | Violation Result |
|-------|-----------|------------------|
| OpenForEnrollment | StatusId ≠ Draft | ❌ Invalid |
| LockEnrollment | StatusId ≠ Open | ❌ Invalid |
| Activate | StatusId ≠ Locked | ❌ Invalid |
| Complete | StatusId ≠ Active | ❌ Invalid |
| Cancel | StatusId ∉ {Draft, Open, Locked} | ❌ Invalid |
| UpdateCurriculum/Course/Term | StatusId ≥ Open | ❌ Forbidden |
| Add/RemoveOffering | StatusId ≥ Open | ❌ Forbidden |

## Snapshot Behavior on Open Transition

When a section transitions `Draft → Open`:
- All snapshot fields in `ClassSectionSubjectOffering` are populated:
  - `SnapshotUnits` - Copied from `CurriculumSubject.SubjectUnitsOverride ?? Subject.Units`
  - `SnapshotSubjectTitle` - Copied from `Subject.Title`
  - `SnapshotSubjectCode` - Copied from `Subject.Code`
  - `SnapshotIsElective` - Copied from `CurriculumSubject.IsElective`
  - `SnapshotElectiveGroupName` - Copied from `CurriculumSubject.ElectiveGroupName`

See `04-snapshot-pattern.md` for details.

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/ClassSections/Commands/OpenClassSectionForEnrollmentTests.cs` (and similarly for other transitions)
- **Integration Tests:** WebAPI endpoint tests verify state transitions and error cases
- **Test Scenarios:**
  - Invalid state transition returns error
  - Valid transition succeeds and saves state
  - Domain events are raised
  - Related data is snapshots correctly on Open transition

## Related Features

- **Enrollment Lifecycle:** Students can only submit enrollments when ClassSection status is `Open`
- **Grade Recording:** Only allowed when section status is `Active` or `Completed`
- **ClassSectionSubjectOffering Snapshot:** Populated when section transitions to `Open`
- **Curriculum Immutability:** Section cannot change curriculum once `Open`

## Notes

- ✅ All 6 states are fully reachable via commands
- ✅ Guards prevent invalid transitions
- ✅ Each transition is atomic (either succeeds fully or fails completely)
- ⚠️ Cancelled sections cannot transition to other states (they remain Cancelled)
- The state machine is the primary enforcement mechanism for enrollment data integrity
