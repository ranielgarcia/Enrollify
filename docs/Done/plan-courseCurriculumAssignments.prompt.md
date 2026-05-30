## Plan: `CourseCurriculumAssignments` Table

**TL;DR:** Add a new `CourseCurriculumAssignments` table that locks a curriculum to a _cohort_ — identified by Course + Entry Academic Year. When creating class sections for Year N in AY X, the system derives the cohort's entry year as the AY that is (N-1) steps before X (by `StartDate` ordering), then looks up this table to get the correct `CurriculumId`.

---

**Steps**

1. Create `Script0015__CourseCurriculumAssignments.sql` in `Scripts/` — defines the table, FK constraints, check constraint, unique filtered index, and non-clustered indexes
2. No seed data required — records are created by admin when setting up a new cohort/academic year intake

---

**New table: `CourseCurriculumAssignments`**

| Column                | Type                   | Notes                                                                       |
| --------------------- | ---------------------- | --------------------------------------------------------------------------- |
| `Id`                  | INT IDENTITY(1,1) PK   |                                                                             |
| `CourseId`            | INT NOT NULL           | FK → `Courses(Id)`                                                          |
| `EntryAcademicYearId` | INT NOT NULL           | FK → `AcademicYears(Id)` — the AY when Year 1 students of this cohort start |
| `CurriculumId`        | INT NOT NULL           | FK → `Curriculums(Id)` — the locked curriculum for the cohort               |
| `CreatedAt`           | DATETIMEOFFSET         | default `SYSDATETIMEOFFSET()`                                               |
| `CreatedBy`           | INT NOT NULL           | FK → `Users(Id)`                                                            |
| `UpdatedAt`           | DATETIMEOFFSET NULL    |                                                                             |
| `UpdatedBy`           | INT NULL               | FK → `Users(Id)`                                                            |
| `DeletedAt`           | DATETIMEOFFSET NULL    |                                                                             |
| `DeletedBy`           | INT NULL               | FK → `Users(Id)`                                                            |
| `IsActive`            | BIT NOT NULL DEFAULT 1 | soft-delete flag                                                            |

**Constraints & indexes:**

- `FK_CourseCurriculumAssignments_Course` → `Courses(Id)`
- `FK_CourseCurriculumAssignments_AcademicYear` → `AcademicYears(Id)`
- `FK_CourseCurriculumAssignments_Curriculum` → `Curriculums(Id)`
- `FK_CourseCurriculumAssignments_CreatedBy/UpdatedBy/DeletedBy` → `Users(Id)`
- `UIdx_CourseCurriculumAssignments_Course_Year_IsActive` ON `(CourseId, EntryAcademicYearId) WHERE IsActive = 1` — ensures only one active curriculum lock per cohort
- `IX_CourseCurriculumAssignments_CourseId`
- `IX_CourseCurriculumAssignments_EntryAcademicYearId`
- `IX_CourseCurriculumAssignments_CurriculumId`

---

**Usage walkthrough**

> Creating class sections for BSCS in AY 2025-2026:

| Year Level | Entry AY     | Lookup in `CourseCurriculumAssignments`  |
| ---------- | ------------ | ---------------------------------------- |
| Year 1     | AY 2025-2026 | (BSCS, AY 2025-2026) → Curriculum 2025-A |
| Year 2     | AY 2024-2025 | (BSCS, AY 2024-2025) → Curriculum 2024-A |
| Year 3     | AY 2023-2024 | (BSCS, AY 2023-2024) → Curriculum 2023-X |
| Year 4     | AY 2022-2023 | (BSCS, AY 2022-2023) → Curriculum 2023-X |

The entry AY derivation lives in application code: find the AcademicYear with `StartDate.Year = currentAY.StartDate.Year - (YearLevel - 1)`.

---

**Relevant files**

- `Scripts/Script0014__InitialCoreTables.sql` — last script; new file comes after this
- `Scripts/Script0009__Curriculums.sql` — `UIdx_Curriculums_Course_Version_IsActive` pattern to reuse for the filtered unique index
- `Scripts/Script0013__AcademicYears.sql` — `AcademicYears` table being referenced

---

**Verification**

1. Run `dotnet build` under `Enrollify.DatabaseMigration` to verify SQL syntax
2. Run `dotnet run --project Enrollify.DatabaseMigration` (or the migration runner) — confirm table is created and indexes exist
3. Manually insert a lock: `(BSCS, AY2024, Curriculum2024-A)` and verify a duplicate insert for the same `(CourseId, EntryAcademicYearId)` is rejected by the unique index while a soft-deleted then re-created record is allowed
4. Verify `CurriculumId` FK enforcement: try inserting a `CurriculumId` belonging to a different course and ensure application-layer validation catches it (SQL FK only enforces existence, not course-match)

---

**Decisions**

- Cohort model: one lock per `(Course, EntryAcademicYear)` — curriculum applies across all year levels for that cohort
- FK to `AcademicYears.Id` (not an INT year like `Curriculums.EffectiveYear`) for referential integrity
- `EntryAcademicYearId` naming is explicit — avoids ambiguity vs. "current AY"
- No status column — `IsActive` (soft-delete) is sufficient; the lock is either active or deleted

---

**Further Considerations**

1. **Curriculum-Course mismatch guard:** SQL FK cannot enforce that `CurriculumId` belongs to the same `CourseId`. Consider an application-layer validation in the command handler (or a DB trigger if preferred).
2. **Backfilling existing data:** `ClassSections` already has `CurriculumId` set directly. If you want historical class sections to be retroactively traceable through this table, existing cohort locks would need to be backfilled — likely a one-time migration script.
