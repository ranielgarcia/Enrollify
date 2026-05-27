# Enrollment Data Locking — Implementation Plan

> Based on research report: `research/how-to-properly-locked-the-important-data-like-cur.md`

---

## Goal

Prevent stale data issues in `ClassSection` and `ClassSectionSubjectOffering` by:
1. Snapshotting `CurriculumSubject` data into offerings at creation time
2. Implementing the `ClassSection` status lifecycle (Draft → Open → Locked → Active → Completed)
3. Adding guards to prevent mutations to referenced data when sections are active

---

## Dependency Graph (High Level)

```
Phase 1: DB Migration
    └─► Phase 2: Domain snapshot properties
            └─► Phase 4: Fix CreateClassSection / BulkInit
Phase 3: Domain transition methods (independent)
    └─► Phase 5: Transition commands
            └─► Phase 7: WebAPI endpoints
                    └─► Phase 8: Frontend
Phase 6: Guards on Subject / AcademicYear / Course  (independent)
Phase 9: Integration tests (depends on Phase 4 + Phase 5)
```

---

## Phase 1 — Database Migration (1 task)

### `db-snapshot-columns` ✏️ Must do first
**File**: `Enrollify.DatabaseMigration/Scripts/Script0019__ClassSectionSubjectOfferingSnapshot.sql`

Add to `ClassSectionSubjectOffering` table:
```sql
ALTER TABLE ClassSectionSubjectOffering
ADD
    CurriculumSubjectId      INT NULL,          -- FK → CurriculumSubjects (audit reference)
    SnapshotSubjectCode      VARCHAR(20) NULL,  -- copied at section creation
    SnapshotSubjectTitle     VARCHAR(100) NULL, -- copied at section creation
    SnapshotUnits            DECIMAL(3,1) NULL, -- CurriculumSubject.Override ?? Subject.Units
    SnapshotIsElective       BIT NULL,          -- from CurriculumSubject.IsElective
    SnapshotElectiveGroupName VARCHAR(100) NULL; -- from CurriculumSubject.ElectiveGroupName

ALTER TABLE ClassSectionSubjectOffering
ADD CONSTRAINT FK_CSSO_CurriculumSubject
    FOREIGN KEY (CurriculumSubjectId)
    REFERENCES CurriculumSubjects(Id)
    ON DELETE SET NULL;     -- if CurriculumSubject is soft-deleted, keep snapshot intact
```

> **All new columns are nullable** so existing records are not broken.

---

## Phase 2 — Domain Layer: ClassSectionSubjectOffering (2 tasks)

### `domain-csso-snapshot-props`
**File**: `Enrollify.Core/Aggregates/ClassSectionSubjectOfferingAggregate/ClassSectionSubjectOffering.cs`

Add new properties and update constructor:
```csharp
// New properties
public CurriculumSubjectId? CurriculumSubjectId { get; private set; }
public string? SnapshotSubjectCode { get; private set; }
public string? SnapshotSubjectTitle { get; private set; }
public decimal? SnapshotUnits { get; private set; }
public bool? SnapshotIsElective { get; private set; }
public string? SnapshotElectiveGroupName { get; private set; }

// Convenience: effective units = per-offering override ?? snapshot ?? null
public decimal? EffectiveUnits => SubjectUnitsOverride ?? SnapshotUnits;
```

Update constructor signature to accept these fields.

### `domain-csso-ef-config`
**File**: `Enrollify.Infrastructure/Data/Configurations/ClassSectionSubjectOfferingConfiguration.cs`

Map the new columns; add optional navigation property to `CurriculumSubject`.

---

## Phase 3 — Domain Layer: ClassSection Status Transitions (6 tasks)

All in `Enrollify.Core/Aggregates/ClassSectionAggregate/ClassSection.cs`.

### Transition Methods

| Task ID | Method | Guard |
|---|---|---|
| `domain-cs-open-method` | `OpenForEnrollment()` | StatusId == Draft |
| `domain-cs-lock-method` | `LockEnrollment()` | StatusId == Open |
| `domain-cs-activate-method` | `Activate()` | StatusId == Locked |
| `domain-cs-complete-method` | `Complete()` | StatusId == Active |
| `domain-cs-cancel-method` | `Cancel()` | StatusId == Draft, Open, or Locked |

```csharp
// Example pattern for all methods:
public ClassSection OpenForEnrollment()
{
    Guard.Against.InvalidInput(StatusId, nameof(StatusId),
        s => s == ClassSectionStatusEnum.Draft,
        "Section must be in Draft status to open for enrollment.");
    StatusId = ClassSectionStatusEnum.Open;
    AddDomainEvent(new ClassSectionOpenedForEnrollmentEvent(Id));
    return this;
}
```

### `domain-cs-mutation-guards`
Add status checks to all existing `Update*` methods:

| Method | Allowed Statuses |
|---|---|
| `UpdateAdviser` | Draft, Open |
| `UpdateCurriculum` | Draft only |
| `UpdateCourse` | Draft only |
| `UpdateAcademicTerm` | Draft only |
| `UpdateCohortAcademicYearId` | Draft only |
| `UpdateYearLevel` | Draft only |
| `UpdateName` | Draft only |

---

## Phase 4 — Application Layer: Fix Snapshot Data in CreateClassSection (2 tasks)

### `app-create-cs-snapshot`
**File**: `Enrollify.Application/Features/ClassSections/Commands/CreateClassSection.cs`

**Before (current — loses data)**:
```csharp
new ClassSectionSubjectOffering(
    classSectionId,
    curriculumSubject.SubjectId,
    maxNumberOfStudents: command.StudentCapacity)
```

**After (correct)**:
```csharp
// Ensure Subject navigation is loaded on curriculumSubjects (update the spec)
var snapshotUnits = curriculumSubject.SubjectUnitsOverride
                    ?? curriculumSubject.Subject?.Units
                    ?? throw new InvalidOperationException("Subject units not found");

new ClassSectionSubjectOffering(
    classSectionId,
    curriculumSubject.SubjectId,
    curriculumSubjectId: curriculumSubject.Id,
    snapshotSubjectCode: curriculumSubject.Subject!.Code.Value,
    snapshotSubjectTitle: curriculumSubject.Subject!.Title,
    snapshotUnits: snapshotUnits,
    snapshotIsElective: curriculumSubject.IsElective,
    snapshotElectiveGroupName: curriculumSubject.ElectiveGroupName,
    maxNumberOfStudents: command.StudentCapacity)
```

**Spec update needed**: The spec used to load `CurriculumSubjects` must `.Include(cs => cs.Subject)`.

### `app-bulk-init-snapshot`
Same fix for `BulkInitializeClassSectionsForAcademicYear.cs`.

---

## Phase 5 — Application Layer: Status Transition Commands (5 tasks)

Create one command file per transition in `Enrollify.Application/Features/ClassSections/Commands/`:

| Task ID | File | Command |
|---|---|---|
| `app-cmd-open-section` | `OpenClassSectionForEnrollment.cs` | Validates ≥1 offering exists; calls `section.OpenForEnrollment()` |
| `app-cmd-lock-section` | `LockClassSectionEnrollment.cs` | Calls `section.LockEnrollment()` |
| `app-cmd-activate-section` | `ActivateClassSection.cs` | Optionally checks term start date; calls `section.Activate()` |
| `app-cmd-complete-section` | `CompleteClassSection.cs` | Calls `section.Complete()` |
| `app-cmd-cancel-section` | `CancelClassSection.cs` | Command includes `Reason` string; calls `section.Cancel()` |

Follow existing command pattern (see `ApproveCurriculum.cs` as model).

---

## Phase 6 — Application Layer: Guards on Related Entities (5 tasks, independent)

### `app-update-subject-guard`
`UpdateSubject.cs` — Before updating, check:
```csharp
// New spec needed: GetActiveOfferingsBySubjectIdSpec (returns CSSO with StatusId >= Open)
var activeOfferings = await _repo.ListAsync(new GetActiveOfferingsBySubjectIdSpec(command.Id), ct);
if (activeOfferings.Any())
    return Result.Forbidden($"Subject is referenced in {activeOfferings.Count} open/active section(s).");
```

### `app-delete-subject-guard`
`DeleteSubject.cs` — Add after the existing Curriculum check:
```csharp
var offerings = await _repo.ListAsync(new GetOfferingsBySubjectIdSpec(command.id), ct);
if (offerings.Any())
    return Result.Forbidden($"Subject is referenced in {offerings.Count} class section offering(s).");
```

### `app-delete-ay-guard`
`DeleteAcademicYearAndTerms.cs` — Complete the existing TODO comment:
```csharp
// TODO: currently just a comment — implement this
var termIds = existing.AcademicTerms.Select(t => t.Id).ToList();
var sections = await _sectionRepo.ListAsync(new GetClassSectionsByAcademicTermIdsSpec(termIds), ct);
if (sections.Any())
    return Result.Forbidden($"Cannot delete — {sections.Count} class section(s) exist for this academic year.");
```

### `app-update-ay-guard`
`UpdateAcademicYearAndTerms.cs` — Add check before update:
```csharp
var activeSections = sections.Where(s =>
    s.StatusId == ClassSectionStatusEnum.Active ||
    s.StatusId == ClassSectionStatusEnum.Completed).ToList();
if (activeSections.Any())
    return Result.Forbidden("Cannot update academic year with active or completed sections.");
```

### `app-delete-course-guard`
`DeleteCourse.cs` — Add before delete:
```csharp
var sections = await _sectionRepo.ListAsync(new GetSectionsByCourseIdSpec(command.id), ct);
if (sections.Any())
    return Result.Forbidden($"Cannot delete — course is referenced by {sections.Count} class section(s).");
```

---

## Phase 7 — WebAPI Layer: Status Transition Endpoints (5 tasks)

Create one endpoint file per transition in `Enrollify.WebAPI/Features/ClassSections/`:

| Task ID | File | HTTP |
|---|---|---|
| `webapi-open-section-endpoint` | `OpenClassSectionEndpoint.cs` | `PUT /class-sections/{id}/open` |
| `webapi-lock-section-endpoint` | `LockClassSectionEndpoint.cs` | `PUT /class-sections/{id}/lock` |
| `webapi-activate-section-endpoint` | `ActivateClassSectionEndpoint.cs` | `PUT /class-sections/{id}/activate` |
| `webapi-complete-section-endpoint` | `CompleteClassSectionEndpoint.cs` | `PUT /class-sections/{id}/complete` |
| `webapi-cancel-section-endpoint` | `CancelClassSectionEndpoint.cs` | `PUT /class-sections/{id}/cancel` + body `{ reason }` |

Follow existing `ApproveCurriculumEndpoint.cs` as pattern. All use `HasUpdateClassSectionPermission` policy.

---

## Phase 8 — Frontend (4 tasks, after API types regenerated)

### `fe-regenerate-api-types`
With backend running: `cd src/system/enrollify-frontend && npm run generate:api:win`

### `fe-section-status-badge`
Add to sections list and section detail pages.

Status → color mapping:
| Status | Badge Color |
|---|---|
| Draft | gray |
| Open | green |
| Locked | amber/orange |
| Active | blue |
| Completed | purple |
| Cancelled | red |

### `fe-section-transition-actions`
In `section-detail-page/`, add contextual action buttons based on `StatusId`:
- Draft → **"Open for Enrollment"** (primary action)
- Open → **"Lock Enrollment"** (warning action) + **"Cancel Section"** (destructive)
- Locked → **"Activate Section"** (primary action)
- Active → **"Complete Section"** (primary action)

Each button calls the respective `PUT /class-sections/{id}/{action}` endpoint and invalidates the section query.

### `fe-snapshot-fields-display`
In the offerings tab of section detail, show:
- `SnapshotUnits` prominently (with tooltip: "Units as defined when this section was created")
- Warning icon if `SnapshotUnits ≠ Subject.Units` (data drift indicator)
- `IsElective` badge if `SnapshotIsElective = true`

---

## Phase 9 — Integration Tests (2 tasks)

### `test-snapshot-integration`
`Enrollify.IntegrationTests/_Tests/Application/ClassSections/CreateClassSectionSnapshotTests.cs`

Key test cases:
1. `CreateClassSection_WithCurriculumUnitOverride_SnapshotsOverrideUnits`
2. `CreateClassSection_WithBaseSubjectUnits_SnapshotsBaseUnits`
3. `CreateClassSection_StoresCurriculumSubjectId`
4. `UpdateSubject_WhenSectionIsOpen_ReturnsForbidden`
5. `UpdateSubject_WhenSectionIsDraft_Succeeds`

### `test-status-transitions-integration`
`Enrollify.IntegrationTests/_Tests/Application/ClassSections/ClassSectionStatusTransitionTests.cs`

Key test cases:
1. `OpenClassSection_WhenDraft_ChangesStatusToOpen`
2. `OpenClassSection_WhenAlreadyOpen_ReturnsForbidden`
3. `LockClassSection_WhenOpen_ChangesStatusToLocked`
4. `ActivateClassSection_WhenLocked_ChangesStatusToActive`
5. `CompleteClassSection_WhenActive_ChangesStatusToCompleted`
6. `CancelClassSection_WhenDraft_ChangesStatusToCancelled`
7. `CancelClassSection_WhenCompleted_ReturnsForbidden`

---

## Implementation Order Recommendation

```
Day 1:  Phase 1 (Migration) + Phase 2 (Domain CSSO props) + Phase 3 (ClassSection methods)
Day 2:  Phase 4 (Fix CreateClassSection/BulkInit) + Phase 5 (Transition commands) + Phase 6 (Guards)
Day 3:  Phase 7 (WebAPI endpoints) + Phase 8 (Frontend)
Day 4:  Phase 9 (Tests) + Run full test suite
```

---

## Files to Create/Edit Summary

| Phase | Action | File |
|---|---|---|
| 1 | CREATE | `DatabaseMigration/Scripts/Script0019__ClassSectionSubjectOfferingSnapshot.sql` |
| 2 | EDIT | `Core/Aggregates/ClassSectionSubjectOfferingAggregate/ClassSectionSubjectOffering.cs` |
| 2 | EDIT/CREATE | `Infrastructure/Data/Configurations/ClassSectionSubjectOfferingConfiguration.cs` |
| 3 | EDIT | `Core/Aggregates/ClassSectionAggregate/ClassSection.cs` |
| 3 | CREATE | `Application/Features/ClassSections/Events/ClassSectionOpenedForEnrollmentEvent.cs` |
| 4 | EDIT | `Application/Features/ClassSections/Commands/CreateClassSection.cs` |
| 4 | EDIT | `Application/Features/ClassSections/Commands/BulkInitializeClassSectionsForAcademicYear.cs` |
| 5 | CREATE ×5 | `Application/Features/ClassSections/Commands/Open/Lock/Activate/Complete/CancelClassSection.cs` |
| 6 | EDIT | `Application/Features/Subjects/Commands/UpdateSubject.cs` |
| 6 | EDIT | `Application/Features/Subjects/Commands/DeleteSubject.cs` |
| 6 | EDIT | `Application/Features/AcademicYearAndTerm/Commands/DeleteAcademicYearAndTerms.cs` |
| 6 | EDIT | `Application/Features/AcademicYearAndTerm/Commands/UpdateAcademicYearAndTerms.cs` |
| 6 | EDIT | `Application/Features/Courses/Commands/DeleteCourse.cs` |
| 7 | CREATE ×5 | `WebAPI/Features/ClassSections/Open/Lock/Activate/Complete/CancelClassSectionEndpoint.cs` |
| 8 | EDIT | `enrollify-frontend/src/page-components/section-detail-page/` (multiple files) |
| 9 | CREATE ×2 | `IntegrationTests/_Tests/Application/ClassSections/` (2 test files) |
