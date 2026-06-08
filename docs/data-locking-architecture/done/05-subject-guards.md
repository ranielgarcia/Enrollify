# Subject Mutation Guards

## Overview
The `Subject` entity is protected by guards that prevent mutations when the subject is actively used in class sections, and prevent deletion if referenced by any offering. These guards enforce data consistency for enrolled students.

## Status
✅ **FULLY IMPLEMENTED**

## Guard 1: UpdateSubject Prevention

### Purpose
Prevents changing a subject's critical properties (code, units, title) while that subject is being used in open, locked, or active class sections.

### Implementation

**File:** `Enrollify.Application/Features/Subjects/Commands/UpdateSubject.cs` (lines 64-76)

```csharp
// Check if subject is used in any active class sections
var activeOfferings = await _classSubscriptionOfferingRepository
    .ListAsync(new GetOfferingsBySubjectAndStatusSpec(command.Id, statusThreshold: ClassSectionStatusEnum.Open), ct);

if (activeOfferings.Any())
{
    return Result.Forbidden(
        $"Subject '{existing.Code}' is currently used in {activeOfferings.Count} open or active class section(s). " +
        $"Complete or cancel those sections before updating the subject.");
}
```

### Guard Condition

| Condition | Violation Result |
|-----------|------------------|
| Subject is referenced by ≥1 offering in Open/Locked/Active sections | ❌ `Result.Forbidden()` |
| Subject is referenced by offerings in Draft or Completed sections | ✅ Allowed to update |
| Subject is not used in any section | ✅ Allowed to update |

### Why Draft is Allowed

- Draft sections are still being configured
- Changes can be made before finalizing
- Snapshotting hasn't happened yet

### Why Completed is Allowed

- Term has ended, grades are final
- Updating subject details won't affect historical records
- Snapshot fields preserve what was originally used

### Error Handling

- **Result Type:** `Result.Forbidden()`
- **HTTP Status:** 403 Forbidden
- **Message Format:** Includes subject code, count of affected sections
- **User Action:** Must complete or cancel those sections first

## Guard 2: DeleteSubject Prevention

### Purpose
Prevents deletion of subjects that are:
1. Used in any active curriculum, OR
2. Referenced by any class section subject offering (regardless of section status)

### Implementation

**File:** `Enrollify.Application/Features/Subjects/Commands/DeleteSubject.cs` (lines 35-49)

```csharp
// Check if subject is in any curriculum
var curriculumUsage = await _curriculumSubjectRepository
    .ListAsync(new GetCurriculumBySubjectIdSpec(command.Id), ct);

if (curriculumUsage.Any())
{
    return Result.Forbidden(
        $"Cannot delete subject because it is used in {curriculumUsage.Count} curriculum(s).");
}

// Check if subject is in any class section offering
var offeringUsage = await _classSubscriptionOfferingRepository
    .ListAsync(new GetOfferingsBySubjectIdSpec(command.Id), ct);

if (offeringUsage.Any())
{
    return Result.Forbidden(
        $"Cannot delete subject because it is referenced in {offeringUsage.Count} class section subject offering(s).");
}
```

### Guard Conditions

| Condition | Violation Result |
|-----------|------------------|
| Subject used in ≥1 `CurriculumSubject` rows | ❌ `Result.Forbidden()` |
| Subject used in ≥1 `ClassSectionSubjectOffering` rows | ❌ `Result.Forbidden()` |
| Subject has no curriculum or offering usage | ✅ Allowed to delete |

### Why Both Checks Matter

**Curriculum Check:**
- Ensures no curriculum references the subject
- Prevents dangling FK on `CurriculumSubject.SubjectId`

**Offering Check:**
- Ensures no class section offering references the subject
- Prevents DB constraint violation on `ClassSectionSubjectOffering.SubjectId`
- Protects snapshot audit trail (offering has `SubjectId` reference)

### Error Handling

- **Result Type:** `Result.Forbidden()`
- **HTTP Status:** 403 Forbidden
- **Message:** Counts offerings/curriculums that reference the subject
- **Prevention:** Application-level guard prevents DB exception

## Real-World Scenarios

### Scenario 1: Update Subject Units During Active Term
**Action:** Admin tries to update CS101 units from 3 → 4 while BSCS-1A section is Open
**Result:** ❌ Forbidden - "CS101 is used in 1 open section"
**Workaround:** Wait until BSCS-1A moves to Completed status, then update

### Scenario 2: Delete Unused Subject
**Action:** Admin deletes a subject that was removed from all curriculums and no sections
**Result:** ✅ Allowed - subject is completely unused

### Scenario 3: Delete Subject Used in Draft Section Only
**Action:** Admin deletes subject while only a Draft section references it
**Result:** ❌ Forbidden - "referenced in 1 class section offering"
**Reason:** Even Draft sections hold a snapshot reference; can't delete
**Workaround:** Delete the section first, then delete the subject

### Scenario 4: Delete Subject from Curriculum But Active in Section
**Action:** Admin removes subject from curriculum but tries to delete
**Result:** ❌ Forbidden - "referenced in 1 class section offering"
**Reason:** Section snapshots still reference the subject
**Workaround:** Complete the section, then delete subject

## Guard Enforcement Points

| Operation | Guard Location | Status Check |
|-----------|---|---|
| Update units/code/title | `UpdateSubject.Handler` | ≥ Open sections |
| Delete subject | `DeleteSubject.Handler` | Any curriculum or offering |

## Related Protections

See also:
- **UpdateCourse Guards:** Similar protection for course code changes (not yet fully implemented)
- **UpdateAcademicYear Guards:** Prevents date changes while active sections exist
- **ClassSectionStatusEnum:** Open status prevents subject offering additions (see `03-class-section-status-machine.md`)

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/Subjects/Commands/UpdateSubjectTests.cs` and `DeleteSubjectTests.cs`
- **Integration Tests:** Verify guards prevent DB constraint violations
- **Test Scenarios:**
  - Update blocked when section Open
  - Update allowed when section Completed
  - Delete blocked if curriculum references it
  - Delete blocked if offering references it
  - Delete allowed when subject is unused

## Implementation Details

### Specification Patterns Used

- **`GetOfferingsBySubjectAndStatusSpec`** - Retrieves offerings for a subject with status >= threshold
- **`GetCurriculumBySubjectIdSpec`** - Retrieves all curriculum subjects using this subject
- **`GetOfferingsBySubjectIdSpec`** - Retrieves all offerings referencing this subject

**File:** `Enrollify.Application/Specifications/` - Each spec is an `ISpecification<T>` implementation

## Error Messages

### UpdateSubject Failure
```
Subject 'CS101' is currently used in 2 open or active class section(s).
Complete or cancel those sections before updating the subject.
```

### DeleteSubject Failure - Curriculum
```
Cannot delete subject because it is used in 3 curriculum(s).
```

### DeleteSubject Failure - Offering
```
Cannot delete subject because it is referenced in 5 class section subject offering(s).
```

## Notes

- ✅ Both guards are fully implemented with specification patterns
- ✅ Error messages are user-friendly and actionable
- ✅ Application-level guards prevent database constraint violations
- ✅ Guards respect the snapshot pattern - offerings are protected even after completion
- ⚠️ Subjects can still be updated in Draft sections (intentional - allows section editing before Open)
