# Academic Year Mutation Guards

## Overview
The `AcademicYear` and `AcademicTerm` entities are protected by guards that prevent structural changes (date updates, term number modifications) when class sections are actively using those terms in advanced lifecycle states.

## Status
✅ **FULLY IMPLEMENTED**

## Guard: UpdateAcademicYear Prevention

### Purpose
Prevents changing an academic year's dates or associated terms while those terms are being used by class sections in Active or Completed status. This protects the term schedule that students are enrolled under.

### Implementation

**File:** `Enrollify.Application/Features/AcademicYearAndTerm/Commands/UpdateAcademicYearAndTerms.cs` (lines 77-90)

```csharp
// Check if academic year has class sections in Active or Completed status
var activeOrCompletedSections = await _classSubscriptionRepository
    .ListAsync(
        new GetClassSectionsByAcademicYearAndStatusSpec(
            command.AcademicYearId,
            statusThreshold: ClassSectionStatusEnum.Active),
        ct);

if (activeOrCompletedSections.Any())
{
    return Result.Forbidden(
        $"Cannot update academic year because {activeOrCompletedSections.Count} class section(s) " +
        $"are in Active or Completed status. Complete those sections before updating the year.");
}
```

### Guard Condition

| Condition | Allowed | Reason |
|-----------|---------|--------|
| Year has NO sections | ✅ Yes | No dependencies |
| Year has Draft/Open/Locked sections | ✅ Yes | Sections not yet started |
| Year has Active sections | ❌ No | Term dates affect running sections |
| Year has Completed sections | ❌ No | Could retroactively change historical data |

### What "Update" Includes

When updating an academic year, the following may change:
- **StartDate / EndDate** - Overall year duration
- **Associated AcademicTerms** - Term dates, numbers, durations
- **Term Start/End dates** - Individual term schedule

### Why Active Sections Block Updates

If a class section is currently running (Active status):
- Term dates define when students attend classes
- Teachers and students rely on the term schedule
- Changing dates mid-term would cause confusion and scheduling conflicts
- Snapshot principle: term definition is frozen once sections are Active

### Why Completed Sections Block Updates

If a class section is Completed:
- Grade books have been finalized with reference to those term dates
- Historical records depend on the term dates for transcript accuracy
- Retroactively changing dates could break audit trails

## Guard Scope: By Section Status

### ✅ Allowed (Update AcademicYear)

**Sections in Draft Status:**
- Section is still being set up
- Changes won't affect students
- Sections can be canceled if needed

**Sections in Open Status:**
- Enrollment is open but term hasn't started
- Safe to update (though ideally shouldn't be needed)
- Section can be cancelled before Locked if needed

**Sections in Locked Status:**
- Enrollment window closed but term not yet started
- Safe to update (though not recommended)
- Section can be cancelled before term begins

### ❌ Blocked (Update Rejected)

**Sections in Active Status:**
- Classes are in session
- Students are attending
- Grades may be partially recorded
- **Result:** `Result.Forbidden()`

**Sections in Completed Status:**
- Term ended, grades final
- Historical record is locked
- **Result:** `Result.Forbidden()`

**Sections in Cancelled Status:**
- Does NOT block the update
- Cancelled sections are already outside the main flow

## Related Guard: DeleteAcademicYear

### Current Status
- **Location:** `Enrollify.Application/Features/AcademicYearAndTerm/Commands/DeleteAcademicYearAndTerms.cs`
- **Status:** ⚠️ **Partially Implemented**
- **Note:** Contains `// TODO: GetClassSectionsByAcademicTermIdsSpec` comment (line 37-40)

### Recommended Guard
```csharp
// Before deletion, check if year has ANY class sections
var existingSections = await _classSubscriptionRepository
    .ListAsync(new GetClassSectionsByAcademicYearSpec(command.AcademicYearId), ct);

if (existingSections.Any())
{
    return Result.Forbidden(
        $"Cannot delete academic year because {existingSections.Count} class section(s) exist. " +
        $"Delete those sections before deleting the year.");
}
```

This would be stricter than the update guard (prevents deletion if ANY sections exist, not just Active/Completed).

## Real-World Scenarios

### Scenario 1: Extend Academic Year End Date
**Current State:**
- AY 2024-2025: Jan 1, 2024 - Dec 31, 2024
- BSCS-1A (Spring 2025) in Open status
- MATH-2A (Fall 2024) in Completed status

**Action:** Admin tries to change AY end date to Jan 15, 2025

**Result:** ❌ Forbidden - "1 class section is in Completed status"

**Reason:** MATH-2A is already Completed; changing year dates could affect grade records

**Workaround:** None - simply keep the year as-is, or wait for full cleanup

### Scenario 2: Correct Term Dates Before Term Starts
**Current State:**
- AY 2025-2026: Spring Term 2025 (Mar 1 - May 30) has typo → should be Jun 1
- BSCS-1A section assigned to Spring Term 2025, currently in Open status

**Action:** Admin updates Spring Term end date to Jun 1, 2025

**Result:** ✅ Allowed - Section is in Open status (enrollment window), term hasn't started yet

**User Impact:** Students see updated dates in enrollment system before classes start

### Scenario 3: Fix Academic Year While No Sections Exist
**Current State:**
- AY 2025-2026 created with incorrect dates
- No class sections created yet

**Action:** Admin updates AY start/end dates

**Result:** ✅ Allowed - No dependencies

### Scenario 4: Cannot Shorten Term While Section Is Active
**Current State:**
- AY 2024-2025: Spring Term (Mar 1 - May 31)
- BSCS-1A in Active status (classes meeting daily)

**Action:** Admin tries to change term end date to May 15

**Result:** ❌ Forbidden - "1 class section is in Active status"

**Reason:** Shortening the term mid-session would break the academic calendar

## Guard Enforcement Points

| Operation | Guard Location | Status Check | Result |
|-----------|---|---|---|
| Update AcademicYear | `UpdateAcademicYearAndTerms.Handler` | Active/Completed sections | Forbidden |
| Delete AcademicYear | `DeleteAcademicYearAndTerms.Handler` | ⚠️ TODO (partially impl) | Should block any sections |

## Affected Operations

### Updatable Fields in UpdateAcademicYearAndTerms

- **AcademicYear.StartDate** - Guarded
- **AcademicYear.EndDate** - Guarded
- **Associated AcademicTerms:**
  - TermNumber - Guarded (via year check)
  - StartDate - Guarded (via year check)
  - EndDate - Guarded (via year check)

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/AcademicYearAndTerm/Commands/UpdateAcademicYearAndTermsTests.cs`
- **Integration Tests:** Verify guards across year/term update operations
- **Test Scenarios:**
  - Update allowed when year has no sections
  - Update allowed when year has Draft/Open/Locked sections
  - Update blocked when year has Active sections
  - Update blocked when year has Completed sections
  - Proper error message includes affected section count

## Implementation Details

### Specification Pattern

- **`GetClassSectionsByAcademicYearAndStatusSpec`** - Retrieves sections for a year with status >= threshold
  - Parameters: `academicYearId`, `statusThreshold: ClassSectionStatusEnum`
  - Returns: `IQueryable<ClassSection>` for sections >= the threshold status

**File:** `Enrollify.Application/Specifications/` - Implements `ISpecification<ClassSection>`

## Error Messages

### UpdateAcademicYear Failure
```
Cannot update academic year because 3 class section(s) are in Active or Completed status.
Complete those sections before updating the year.
```

### DeleteAcademicYear Failure (Recommended)
```
Cannot delete academic year because 5 class section(s) exist.
Delete those sections before deleting the year.
```

## Related Guards

- **Subject Updates:** Blocked if subject used in Open/Active sections (see `05-subject-guards.md`)
- **Course Updates:** Partially implemented (see `pending/01-update-course-guards.md`)
- **Curriculum Approval:** Triggers auto-sync of course-curriculum assignments (see `02-course-curriculum-assignment.md`)

## Notes

- ✅ UpdateAcademicYear guard is fully implemented with specification pattern
- ✅ Error messages clearly indicate which sections are blocking the operation
- ⚠️ DeleteAcademicYear guard is only partially implemented (has TODO comment)
- ✅ Guards respect the timeline: only blocks when term is Active or Completed
- The principle: **once a term is Active, its dates are frozen**
