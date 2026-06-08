# Snapshot Pattern: Frozen Subject Data

## Overview
When a `ClassSection` is created from a curriculum, the subject data (units, title, code, elective status) is physically copied into `ClassSectionSubjectOffering` snapshot fields. This prevents silent retroactive changes if the original `Subject` or `CurriculumSubject` is later modified.

## Status
✅ **FULLY IMPLEMENTED**

## Problem It Solves

### Stale Data Scenario
Without snapshots:
1. Admin creates class section BSCS-1A with CS101 offering (3 units)
2. Admin corrects Subject.Units: CS101 from 3 → 4 units
3. **Result:** Every ClassSectionSubjectOffering for CS101 now shows 4 units retroactively
4. **Student transcript:** Shows different units depending on when queried

With snapshots:
1. Admin creates class section BSCS-1A with CS101 offering
2. **SnapshotUnits = 3** is captured and frozen
3. Admin updates Subject.Units to 4
4. **Result:** BSCS-1A offering still shows 3 units; new sections will show 4

## Snapshot Fields in ClassSectionSubjectOffering

- **File:** `Enrollify.Core/Aggregates/ClassSectionSubjectOfferingAggregate/ClassSectionSubjectOffering.cs` (lines 60-66)

### Snapshot Properties

```csharp
public string? SnapshotSubjectCode { get; private set; }          // Line 62
public string? SnapshotSubjectTitle { get; private set; }         // Line 63
public decimal? SnapshotUnits { get; private set; }               // Line 64
public bool? SnapshotIsElective { get; private set; }             // Line 65
public string? SnapshotElectiveGroupName { get; private set; }    // Line 66
```

### Snapshot Validation

- **File:** `Enrollify.Core/Aggregates/ClassSectionSubjectOfferingAggregate/ClassSectionSubjectOffering.cs` (lines 35-42)
- **Constructor Validation:**
  - `SnapshotUnits` is required (not null) if offering will be exposed to students
  - `SnapshotSubjectCode` and `SnapshotSubjectTitle` are required
  - Ensures snapshots are populated at construction time

## Data Capture at Offering Creation

### CreateClassSection Flow

**File:** `Enrollify.Application/Features/ClassSections/Commands/CreateClassSection.cs` (lines 143-158)

When a new offering is created from a `CurriculumSubject`:

```csharp
foreach (var curriculumSubject in curriculumSubjects)
{
    var offering = new ClassSectionSubjectOffering(
        classSection.Id,
        curriculumSubject.SubjectId,
        curriculumSubject.Id,                          // CurriculumSubjectId (audit ref)
        curriculumSubject.SubjectUnitsOverride,        // Curriculum-level units override
        curriculumSubject.IsElective,                  // From curriculum, not base subject
        curriculumSubject.ElectiveGroupName,           // From curriculum
        maxNumberOfStudents: command.StudentCapacity
    );
    classSection.AddSubjectOffering(offering);
}
```

### Units Priority Calculation

The `SnapshotUnits` field uses a priority order:

```
SnapshotUnits = CurriculumSubject.SubjectUnitsOverride  (if set)
              ?? Subject.Units                           (fallback)
```

**Why?** A curriculum can override a subject's base units. For example:
- Base CS101: 3 units
- BSCS Curriculum 2024: CS101 override to 4 units (increased requirement)
- Offering in BSCS-1A: SnapshotUnits = 4

**File:** Constructor in `ClassSectionSubjectOffering.cs` handles this logic

## Additional Snapshot Reference: CurriculumSubjectId

- **File:** `Enrollify.Core/Aggregates/ClassSectionSubjectOfferingAggregate/ClassSectionSubjectOffering.cs` (line 61)
- **Property:** `CurriculumSubjectId? { get; private set; }`
- **Purpose:** Audit reference linking the offering back to the exact `CurriculumSubject` row that defined it
- **Value:** Allows tracing "which curriculum version's structure was used to create this offering?"

This is not a "snapshot" in the data-copy sense, but a **reference snapshot** that documents the source.

## Immutability of Snapshots

### In Draft Status
- Snapshots exist but offering details can still be modified via `UpdateClassSectionSubjectOffering`
- Teacher, room, schedule can change
- But `SnapshotUnits`, `SnapshotSubjectTitle`, etc. are **read-only** (no setters)

### On Open Transition
- All snapshot fields become **permanently frozen**
- No further offering detail changes allowed
- Raises `ClassSectionOpenedForEnrollmentEvent`

### In Open/Locked/Active/Completed Statuses
- Snapshots are immutable
- Cannot add/remove offerings
- Cannot change curriculum or course
- See `03-class-section-status-machine.md` for full guard rules

## Post-Snapshot Override: SubjectUnitsOverride

There is a **separate field** `SubjectUnitsOverride` on `ClassSectionSubjectOffering` that allows:
- Admin to apply a **per-offering adjustment** after the section is created
- Different from the curriculum-level override captured in `SnapshotUnits`
- Use case: "CS101 is normally 3 units, but this offering is lab-intensive and counts as 4"

**Flow:**
1. `SnapshotUnits = 3` (from Subject.Units or CurriculumSubject override)
2. Admin adjusts `SubjectUnitsOverride = 1` (adds 1 additional unit)
3. **Effective units for students = 4** (snapshot + override)

## Real-World Example

### Scenario: Creating BSCS-1A for AY 2024-2025

**Input Data:**
- `CurriculumSubject` for CS101 in BSCS 2024:
  - SubjectId = 10 (CS101)
  - YearLevel = 1
  - TermNumber = 1
  - IsElective = false
  - SubjectUnitsOverride = 4 (curriculum-level increase)
- `Subject` CS101:
  - Code = "CS101"
  - Title = "Introduction to Computer Science"
  - Units = 3

**Offering Created:**
```
ClassSectionSubjectOffering
├── ClassSectionId = 105 (BSCS-1A)
├── SubjectId = 10 (reference to Subject)
├── CurriculumSubjectId = 42 (reference to CurriculumSubject in BSCS 2024)
├── SnapshotSubjectCode = "CS101" ◄─── frozen at creation
├── SnapshotSubjectTitle = "Introduction to Computer Science" ◄─── frozen
├── SnapshotUnits = 4 ◄─── curriculum override used (not base 3)
├── SnapshotIsElective = false ◄─── from curriculum
├── SnapshotElectiveGroupName = null
└── SubjectUnitsOverride = null (can be set later by admin if needed)
```

**Later Events:**
- Subject CS101 units updated to 5 → BSCS-1A still shows 4
- Curriculum BSCS 2024 CS101 override changed → BSCS-1A unaffected
- New section BSCS-1B created → will capture current values (potentially 5)

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/ClassSectionSubjectOfferings/Commands/` - verify snapshot fields are populated
- **Integration Tests:** Create section → verify snapshots exist in database
- **Test Scenarios:**
  - Offering created with correct subject data snapshot
  - Curriculum override is captured (not base subject units)
  - Snapshot is immutable after section opens
  - Later subject modifications don't affect existing snapshots

## Related Features

- **ClassSection Transitions:** Snapshots are locked at Open (see `03-class-section-status-machine.md`)
- **Subject Guards:** Updates blocked if used in Open/Active sections (see `05-subject-guards.md`)
- **CurriculumSubject:** Source of snapshot data
- **Subject Catalog:** Base subject data (used if no curriculum override)

## Design Reference

This pattern follows the **eShopOnContainers canonical DDD example** from Microsoft:
- `OrderItem` copies `ProductName`, `UnitPrice`, `PictureUrl` from the product catalog
- Similarly, `ClassSectionSubjectOffering` copies subject/curriculum data
- Only FK references are kept for audit trails

**Citation:** `dotnet-architecture/eShopOnContainers` → `src/Services/Ordering/Ordering.Domain/AggregatesModel/OrderAggregate/OrderItem.cs`

## Notes

- ✅ Snapshots are complete and frozen
- ✅ Audit references (`CurriculumSubjectId`, `SubjectId`) support traceability
- ✅ Post-snapshot override allows fine-tuning without breaking immutability
- ⚠️ Snapshot fields are currently nullable in the domain model but validated in constructor
