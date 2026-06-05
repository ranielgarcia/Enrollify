# Curriculum-Based Prerequisites Design Plan

## Overview

This document outlines the design for implementing a **curriculum-based approach** for managing subject prerequisites in the Enrollify enrollment system. Instead of attaching prerequisites directly to subjects, prerequisites are versioned and managed as part of the curriculum structure.

---

## Why Curriculum-Based Modeling?

### The Problem with Subject-Level Prerequisites

When prerequisites are directly attached to subjects:
- All students see the same prerequisites regardless of when they enrolled
- Changing prerequisites affects historical records
- No way to "grandfather" existing students under old rules
- Cross-program prerequisite differences are hard to model

### The Solution: Curriculum Versioning

By attaching prerequisites to **curriculum versions**, we gain:
- **Version Control**: Each curriculum revision has its own prerequisite rules
- **Student Grandfathering**: Students follow the curriculum they enrolled under. See [Definition](https://chatgpt.com/c/69590188-1758-8322-b913-b4e73516d246)
- **Audit Trail**: Historical curricula are preserved for accreditation
- **Flexibility**: Same subject can have different prerequisites per curriculum

---

## Real-World Scenarios That Favor Curriculum-Based Modeling

### Scenario 1: CHED Memorandum Order Compliance (Philippines)

**Context**: The Commission on Higher Education (CHED) periodically issues new curriculum guidelines (CMO) that programs must adopt.

**Example**: 
- In 2018, CHED released CMO 20 for IT programs, requiring "Data Structures" to have both "Programming 1" AND "Discrete Mathematics" as prerequisites.
- Previously, only "Programming 1" was required.
- Students who enrolled before 2018 should still follow the old curriculum.

**Without Curriculum Versioning**: The registrar would need to manually track which students use old vs. new prerequisites—error-prone and unscalable.

**With Curriculum Versioning**: 
- Curriculum 2017 → "Data Structures" requires only "Programming 1"
- Curriculum 2018 → "Data Structures" requires "Programming 1" + "Discrete Mathematics"
- Students are tagged with their curriculum year at enrollment.

---

### Scenario 2: Program Accreditation and Historical Records

**Context**: Accrediting bodies (PACUCOA, AACCUP, etc.) require institutions to demonstrate what prerequisites were in effect during specific academic periods.

**Example**:
- During a 2025 accreditation visit, assessors ask: "What were the prerequisites for 'Capstone Project' for students who graduated in 2022?"
- The institution must produce the exact curriculum that was in effect.

**Without Curriculum Versioning**: Prerequisites might have been overwritten; no historical record exists.

**With Curriculum Versioning**: Query the 2020 curriculum (when 2022 graduates enrolled) to show exact prerequisites.

---

### Scenario 3: Shifting/Transferee Students with Credit Evaluation

**Context**: A student transfers from one program to another or from another institution.

**Example**:
- A BS Accountancy student shifts to BSIT in their 3rd year.
- They already passed "Financial Accounting 1" which is equivalent to "IT Elective: Accounting Basics" in BSIT.
- The BSIT 2023 curriculum might have different equivalence rules than the 2025 curriculum.

**Without Curriculum Versioning**: Equivalence rules might change, invalidating the student's credited subjects.

**With Curriculum Versioning**: The student is placed under the curriculum effective at their shift date, preserving credit decisions.

---

### Scenario 4: K-12 Transition (Philippines, 2016-2018)

**Context**: The Philippines transitioned from 10-year to 12-year basic education, affecting college readiness.

**Example**:
- Pre-K12 students (enrolled before 2018) took certain bridge subjects.
- K-12 graduates (2018 onwards) didn't need those bridge subjects.
- "English 1" for pre-K12 had prerequisite "Remedial English"; K-12 students had none.

**Without Curriculum Versioning**: Impossible to maintain two parallel prerequisite structures.

**With Curriculum Versioning**: 
- Curriculum 2017 (Pre-K12) → "English 1" requires "Remedial English"
- Curriculum 2018 (K-12) → "English 1" has no prerequisite

---

### Scenario 5: Industry-Driven Curriculum Updates

**Context**: Industry partnerships require curriculum updates to stay relevant.

**Example**:
- A partnership with a tech company requires adding "Cloud Computing" as a prerequisite for "Systems Integration" starting 2024.
- Existing students in their 3rd year (enrolled 2022) should not be blocked from "Systems Integration."

**Without Curriculum Versioning**: Adding the prerequisite would block existing students.

**With Curriculum Versioning**: Only the 2024 curriculum includes the new prerequisite; 2022 enrollees continue unaffected.

---

### Scenario 6: COVID-19 Academic Adjustments

**Context**: During the pandemic, many institutions relaxed prerequisites due to learning gaps.

**Example**:
- For the 2020-2021 academic year, "Calculus 2" prerequisite was changed from "Calculus 1 with grade of 2.0 or better" to just "Calculus 1 (any passing grade)."
- This exception should only apply to students enrolled during that period.

**Without Curriculum Versioning**: The exception would need manual tracking or would apply to all students.

**With Curriculum Versioning**: Create a special "Curriculum 2020-COVID" version with relaxed prerequisites.

---

## Database Schema Changes

### New Tables

#### 1. Curricula (Curriculum Versions)
```
Curricula
├── Id (PK)
├── CourseId (FK → Courses) - Which program this curriculum belongs to
├── EffectiveYear - Academic year when this curriculum takes effect
├── Version - Version identifier (e.g., "2024-A", "2024-B")
├── Status - DRAFT, ACTIVE, PHASED_OUT, ARCHIVED
├── Description - Notes about this curriculum version
├── ApprovedDate - When the curriculum was approved
├── Audit fields (CreatedAt, CreatedBy, etc.)
```

#### 2. CurriculumSubjects (Subject Placement in Curriculum)
```
CurriculumSubjects
├── Id (PK)
├── CurriculumId (FK → Curricula)
├── SubjectId (FK → Subjects)
├── YearLevel - Which year this subject is typically taken
├── Semester - Which semester (1, 2, or 3 for summer)
├── IsElective - Whether this is an elective slot
├── Audit fields
```

#### 3. CurriculumSubjectPrerequisites (Prerequisites Scoped to Curriculum)
```
CurriculumSubjectPrerequisites
├── CurriculumSubjectId (FK → CurriculumSubjects) - The subject requiring prerequisites
├── PrerequisiteCurriculumSubjectId (FK → CurriculumSubjects) - The prerequisite subject
├── Audit fields
```

### Modified Tables

#### Students
- Add: `CurriculumId` (FK → Curricula) - Which curriculum the student follows
- This determines which prerequisite rules apply to the student

### Tables to Remove/Deprecate

#### SubjectPrerequisites
- This table becomes obsolete as prerequisites are now in `CurriculumSubjectPrerequisites`
- Consider keeping for backward compatibility during migration, then deprecate

---

## Entity Relationships

```
Courses (Programs)
    │
    └── Curricula (1:N) - A course has multiple curriculum versions
            │
            └── CurriculumSubjects (1:N) - A curriculum has many subject placements
                    │
                    ├── Subjects (N:1) - Each placement references a subject
                    │
                    └── CurriculumSubjectPrerequisites (1:N) - Prerequisites per curriculum
                            │
                            └── CurriculumSubjects (N:1) - Points to prerequisite subjects

Students
    │
    └── Curricula (N:1) - A student follows one curriculum
```

---

## Key Business Rules

1. **One Active Curriculum per Course**: At any time, only one curriculum can have `Status = ACTIVE` per course.

2. **Student Curriculum Assignment**: When a student enrolls, they are assigned to the current active curriculum. This assignment typically doesn't change.

3. **Prerequisite Validation**: When validating enrollment, check prerequisites against the **student's assigned curriculum**, not the currently active curriculum.

4. **Curriculum Status Transitions**:
   - DRAFT → ACTIVE (when approved and effective date is reached)
   - ACTIVE → PHASED_OUT (when a new curriculum becomes active)
   - PHASED_OUT → ARCHIVED (when no students remain under this curriculum)

5. **Cross-Curriculum Subject References**: Prerequisites must reference subjects within the SAME curriculum. Cross-curriculum prerequisites are not allowed.

---

## Migration Strategy

### Phase 1: Schema Addition
- Create new tables (Curricula, CurriculumSubjects, CurriculumSubjectPrerequisites)
- Add CurriculumId to Students table (nullable initially)

### Phase 2: Data Migration
- Create default curriculum for each course (e.g., "Curriculum 2025-Initial")
- Migrate existing SubjectPrerequisites to CurriculumSubjectPrerequisites
- Assign all existing students to the default curriculum

### Phase 3: Application Updates
- Update enrollment validation to use curriculum-based prerequisites
- Update student registration to assign curriculum
- Update admin UI for curriculum management

### Phase 4: Cleanup
- Make Students.CurriculumId NOT NULL
- Deprecate SubjectPrerequisites table (keep for reference, mark as obsolete)

---

## Benefits Summary

| Aspect | Before (Subject-Level) | After (Curriculum-Level) |
|--------|------------------------|--------------------------|
| Prerequisites Change | Affects all students | Only affects new curriculum |
| Historical Records | Lost on update | Preserved in old curriculum |
| Student Grandfathering | Manual tracking | Automatic by curriculum |
| Accreditation Audit | Difficult to prove | Query by curriculum year |
| Program Flexibility | One-size-fits-all | Versioned per curriculum |
| Regulatory Compliance | Manual adjustments | Create new curriculum version |

---

## Open Questions / Future Considerations

1. **Curriculum Copying**: Should there be a "copy curriculum" feature to create new versions from existing ones?

2. **Subject Equivalence**: Should SubjectEquivalence also be curriculum-scoped? (Currently kept at subject level for cross-program recognition)

3. **Grade Requirements**: Should CurriculumSubjectPrerequisites include minimum grade requirements? (e.g., must pass prerequisite with 2.0 or better)

4. **Co-requisites**: Should we also model co-requisites (subjects that must be taken together) at the curriculum level?

5. **Elective Groups**: How to model "choose 3 from this group of 5 electives" within a curriculum?

---

## Related Documents

- [User Story: Subject Prerequisites](../docs/user-stories/07-medium-subject-prerequisites.md)
- [Core Domain Models](../docs/requirements/Core%20Domain%20Models%20in%20a%20College%20Enrollment%20System.md)
