# Course-Agnostic Subjects Design

## Overview

In Enrollify, **subjects are course-agnostic catalog entries**. This means a subject like "English 1" exists once in the system and can be added to multiple curricula across different programs (courses).

---

## Key Concept

### Before: Subject Tied to Course (Old Design)
```
Subjects
├── Id: 1, Code: ENG101, Title: English 1, CourseId: 1 (BSCS)
├── Id: 2, Code: ENG101, Title: English 1, CourseId: 2 (BSIT)  ← Duplicate!
├── Id: 3, Code: ENG101, Title: English 1, CourseId: 3 (BSBA)  ← Duplicate!
└── Id: 4, Code: ENG101, Title: English 1, CourseId: 4 (BSA)   ← Duplicate!
```

**Problems**:
- Same subject duplicated for every program
- Hard to maintain consistency (update title in one place, forget others)
- Difficult to query "how many sections of English 1 do we need across all programs?"
- Subject equivalence becomes complex (which ENG101 is equivalent to which?)

---

### After: Subject is Course-Agnostic (Current Design)
```
Subjects (Global Catalog)
├── Id: 1, Code: ENG101, Title: English 1
├── Id: 2, Code: MATH101, Title: College Algebra
├── Id: 3, Code: CC101, Title: Programming 1
└── Id: 4, Code: FIL101, Title: Filipino 1

CurriculumSubjects (Links subjects to curricula)
├── CurriculumId: 1 (BSCS 2024), SubjectId: 1 (ENG101), Year: 1, Sem: 1
├── CurriculumId: 2 (BSIT 2024), SubjectId: 1 (ENG101), Year: 1, Sem: 1
├── CurriculumId: 3 (BSBA 2024), SubjectId: 1 (ENG101), Year: 1, Sem: 2  ← Different placement!
└── CurriculumId: 4 (BSA 2024),  SubjectId: 1 (ENG101), Year: 1, Sem: 1
```

**Benefits**:
- Single source of truth for each subject
- Update once, reflects everywhere
- Easy cross-program queries
- Clean subject equivalence (one subject = one record)

---

## Database Schema

### Subjects Table (Global Catalog)
```sql
CREATE TABLE Subjects
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(10) NOT NULL,           -- Globally unique code
    Title VARCHAR(100) NOT NULL,
    Units DECIMAL(3,1) NULL,
    Description VARCHAR(255) NULL,
    PreferRoomTypeId INT NOT NULL,
    -- Note: NO CourseId - subjects are course-agnostic
    
    CONSTRAINT UQ_Subjects_Code UNIQUE (Code),  -- Code is globally unique
    ...
);
```

### CurriculumSubjects Table (Links Subjects to Curricula)
```sql
CREATE TABLE CurriculumSubjects
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CurriculumId INT NOT NULL,           -- Which curriculum
    SubjectId INT NOT NULL,              -- Which subject from the catalog
    YearLevel INT NOT NULL,              -- Year level placement (1-6)
    Semester INT NOT NULL,               -- Semester placement (1, 2, 3)
    IsElective BIT NOT NULL DEFAULT 0,
    ElectiveGroupName VARCHAR(50) NULL,
    ...
);

-- A subject can only appear ONCE per curriculum
CREATE UNIQUE NONCLUSTERED INDEX UIdx_CurriculumSubjects_Curriculum_Subject_IsActive
ON CurriculumSubjects(CurriculumId, SubjectId)
WHERE IsActive = 1;
```

---

## Constraints Explained

| Constraint | Table | What It Ensures |
|------------|-------|-----------------|
| `UQ_Subjects_Code` | Subjects | Only ONE "ENG101" exists globally |
| `UIdx_CurriculumSubjects_Curriculum_Subject_IsActive` | CurriculumSubjects | A subject appears only ONCE per curriculum |

### What This Allows

✅ "ENG101" in BSCS Curriculum 2024  
✅ "ENG101" in BSIT Curriculum 2024  
✅ "ENG101" in BSCS Curriculum 2025 (different curriculum version)  
❌ "ENG101" in BSCS Curriculum 2024 **twice** (blocked by unique constraint)

---

## Real-World Example

### Subject Catalog
| Id | Code | Title | Units |
|----|------|-------|-------|
| 1 | ENG101 | English 1 | 3.0 |
| 2 | MATH101 | College Algebra | 3.0 |
| 3 | CC101 | Programming 1 | 3.0 |
| 4 | FIL101 | Filipino 1 | 3.0 |
| 5 | PE101 | Physical Education 1 | 2.0 |

### Curricula
| Id | CourseId | Course | EffectiveYear | Version |
|----|----------|--------|---------------|---------|
| 1 | 1 | BSCS | 2024 | 2024-A |
| 2 | 2 | BSIT | 2024 | 2024-A |
| 3 | 3 | BSBA | 2024 | 2024-A |

### CurriculumSubjects (How Subjects are Placed)
| CurriculumId | Curriculum | SubjectId | Subject | YearLevel | Semester |
|--------------|------------|-----------|---------|-----------|----------|
| 1 | BSCS 2024 | 1 | ENG101 | 1 | 1 |
| 1 | BSCS 2024 | 2 | MATH101 | 1 | 1 |
| 1 | BSCS 2024 | 3 | CC101 | 1 | 1 |
| 2 | BSIT 2024 | 1 | ENG101 | 1 | 1 |
| 2 | BSIT 2024 | 2 | MATH101 | 1 | 1 |
| 2 | BSIT 2024 | 3 | CC101 | 1 | 2 |
| 3 | BSBA 2024 | 1 | ENG101 | 1 | 2 |
| 3 | BSBA 2024 | 2 | MATH101 | 1 | 1 |

**Notice**:
- Same subjects (ENG101, MATH101) appear in multiple curricula
- Each curriculum can place them at different year/semester
- CC101 is in Year 1 Sem 1 for BSCS, but Year 1 Sem 2 for BSIT

---

## Prerequisites Are Curriculum-Specific

Even though subjects are shared, **prerequisites are defined per curriculum** via `CurriculumSubjectPrerequisites`.

### Example: Different Prerequisites for Same Subject

| Curriculum | Subject | Prerequisites |
|------------|---------|---------------|
| BSCS 2024 | Data Structures | Programming 1 |
| BSCS 2025 | Data Structures | Programming 1 + Discrete Math |
| BSIT 2024 | Data Structures | Programming 1 |

The same "Data Structures" subject can have different prerequisites depending on:
1. Which program (BSCS vs BSIT)
2. Which curriculum year (2024 vs 2025)

---

## Common Use Cases

### Use Case 1: Add a New Subject to Multiple Programs

**Scenario**: A new GE subject "Understanding the Self" (GEC101) needs to be added to all programs.

**Steps**:
1. Create ONE subject record in `Subjects` table
2. Add to each curriculum via `CurriculumSubjects` with appropriate year/semester

```sql
-- Step 1: Create subject once
INSERT INTO Subjects (Code, Title, Units, ...) 
VALUES ('GEC101', 'Understanding the Self', 3.0, ...);

-- Step 2: Add to multiple curricula
INSERT INTO CurriculumSubjects (CurriculumId, SubjectId, YearLevel, Semester, ...)
VALUES 
    (1, @NewSubjectId, 1, 1, ...),  -- BSCS 2024
    (2, @NewSubjectId, 1, 1, ...),  -- BSIT 2024
    (3, @NewSubjectId, 1, 2, ...);  -- BSBA 2024
```

### Use Case 2: Update Subject Title

**Scenario**: "Programming 1" is renamed to "Introduction to Programming".

**Steps**: Update ONE record - change reflects in all curricula.

```sql
UPDATE Subjects 
SET Title = 'Introduction to Programming' 
WHERE Code = 'CC101';
```

All programs using CC101 now see the updated title.

### Use Case 3: Query Total Demand for a Subject

**Scenario**: How many sections of "English 1" do we need for all programs this semester?

```sql
SELECT 
    s.Code,
    s.Title,
    COUNT(DISTINCT cs.CurriculumId) AS ProgramsUsingSubject,
    SUM(sect.StudentCapacity) AS TotalStudentCapacity
FROM Subjects s
JOIN CurriculumSubjects cs ON s.Id = cs.SubjectId
JOIN Curricula c ON cs.CurriculumId = c.Id
JOIN ClassSections sect ON sect.CourseId = c.CourseId
WHERE s.Code = 'ENG101'
  AND cs.YearLevel = sect.YearLevel
  AND cs.Semester = 1  -- 1st semester
GROUP BY s.Code, s.Title;
```

---

## Edge Cases

### Edge Case 1: Program-Specific Subject

Some subjects are truly program-specific (e.g., "Capstone Project for CS").

**Solution**: Create it as a regular subject. It will only be linked to relevant curricula.

```
Subjects: CSCP - Capstone Project for Computer Science

CurriculumSubjects:
├── BSCS 2024 → CSCP (Year 4, Sem 2)
└── (Not linked to BSIT, BSBA, etc.)
```

### Edge Case 2: Subject with Same Code but Different Content

Some schools reuse codes for different content across programs (not recommended, but happens).

**Solution**: Use different codes or add a suffix.

```
Subjects:
├── MATH101-GE - College Algebra (for non-math majors)
├── MATH101-ENG - College Algebra (for engineering)
```

Or better, maintain unique codes:
```
Subjects:
├── GE-MATH101 - College Algebra (GE version)
├── ENG-MATH101 - College Algebra (Engineering version)
```

### Edge Case 3: Same Subject, Different Units Per Program

Rare, but some programs might have "English 1" as 3 units while others have it as 6 units.

**Solution**: These are actually different subjects. Create separate records.

```
Subjects:
├── ENG101 - English 1 (3 units)
├── ENG101-EXT - English 1 Extended (6 units)
```

---

## Benefits Summary

| Aspect | Course-Tied (Old) | Course-Agnostic (Current) |
|--------|-------------------|---------------------------|
| Data Duplication | High (one per program) | None |
| Maintenance | Update N times | Update once |
| Consistency | Error-prone | Guaranteed |
| Cross-Program Queries | Complex joins | Simple queries |
| Subject Equivalence | Complex | Straightforward |
| Storage Efficiency | Wasteful | Optimal |

---

## Related Documents

- [Curriculum-Based Prerequisites Design](./curriculum-based-prerequisites-design.md)
- [Subject Equivalence Design Analysis](./subject-equivalence-design-analysis.md)
- [Transferee Credit Evaluation Design](./transferee-credit-evaluation-design.md)
