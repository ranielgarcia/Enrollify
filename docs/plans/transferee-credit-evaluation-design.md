# Transferee and Shifter Credit Evaluation Design Plan

## Overview

This document discusses how universities typically handle credit evaluation for **transferee students** (from other institutions) and **shifters** (students changing programs within the same institution). It also outlines how the Enrollify system can support these processes.

---

## Definitions

| Term | Definition |
|------|------------|
| **Transferee** | A student coming from another institution who wants to continue their studies |
| **Shifter** | A student who changes from one program to another within the same institution |
| **Credit Evaluation** | The process of determining which previously completed subjects can be credited toward the new program |
| **Credited Subject** | A subject from the old program/institution recognized as equivalent to a subject in the new program |
| **Deficiency** | Subjects the student still needs to take to complete the new program |

---

## How Universities Typically Handle Transferee Credit Evaluation

### Step 1: Document Submission

The transferee submits:
- **Transcript of Records (TOR)** - Official grade record from previous institution
- **Course/Subject Descriptions** - Syllabi or catalog descriptions of subjects taken
- **Certificate of Good Moral Character**
- **Honorable Dismissal** - Proof of good standing from previous school

### Step 2: Initial Screening by Admissions Office

The Admissions Office checks:
- Is the previous institution accredited/recognized?
- Does the student meet minimum GPA requirements for transfer?
- Are there available slots in the target program?

### Step 3: Credit Evaluation by the Department

The **Department Chair** or a designated **Credit Evaluation Committee** reviews each subject:

#### Evaluation Criteria (Typical)

| Criterion | Typical Requirement |
|-----------|---------------------|
| **Subject Title/Content Match** | At least 75-80% content similarity |
| **Unit/Credit Hours** | Must be equal or greater than the equivalent subject |
| **Grade Obtained** | Minimum passing grade (often 2.5 or 75% or higher) |
| **Recency** | Some institutions require subjects taken within 5-10 years |
| **Accreditation Status** | Subjects from non-accredited schools may not be credited |

#### Common Evaluation Outcomes

1. **Full Credit** - Subject is fully equivalent; no need to retake
2. **Partial Credit** - Subject covers some content; student may need to take a bridging course
3. **No Credit** - Subject is not equivalent; student must take the subject
4. **Conditional Credit** - Credit granted pending validation exam

### Step 4: Credit Evaluation Report Generation

The evaluator produces a **Credit Evaluation Report** containing:
- List of subjects credited (with equivalent subjects in the new program)
- List of deficiencies (subjects the student must still take)
- Recommended year level placement
- Total units credited vs. total units required

### Step 5: Curriculum Assignment

The student is assigned to a specific **curriculum version**:
- Usually the **current active curriculum** at the time of admission
- In some cases, an older curriculum if it allows for a smoother transition

### Step 6: Student Advising

The student meets with an **academic adviser** to:
- Understand their remaining subjects
- Plan their course load per semester
- Address any prerequisite concerns (credited subjects may satisfy prerequisites)

---

## Real-World Variations in Credit Evaluation

### Variation 1: Subject-by-Subject vs. Block Credit

| Approach | Description | When Used |
|----------|-------------|-----------|
| **Subject-by-Subject** | Each subject is individually evaluated | Most common; provides precise mapping |
| **Block Credit** | A group of subjects is credited as a whole (e.g., "General Education Block") | Used for large transfers or articulation agreements |

### Variation 2: Articulation Agreements

Some institutions have **pre-negotiated agreements** with partner schools:
- Subjects from School A are automatically credited at School B
- Reduces evaluation time for common transfer pathways
- Example: Community colleges with university transfer agreements

### Variation 3: Validation/Challenge Exams

If credit evaluation is uncertain:
- Student may take a **validation exam** to prove competency
- Passing the exam grants credit; failing requires taking the subject

### Variation 4: Maximum Transferable Units

Many institutions cap the number of units that can be transferred:
- Example: "Maximum of 50% of total program units can be credited"
- Ensures students complete a significant portion at the degree-granting institution

---

## Challenges in Credit Evaluation

### Challenge 1: Subjective Evaluation

- Different evaluators may reach different conclusions
- Lack of standardized criteria across departments

**Solution**: Maintain an **equivalence database** that records past evaluation decisions for consistency.

### Challenge 2: Outdated Subjects

- Technology subjects from 10 years ago may no longer be relevant
- E.g., "COBOL Programming" credited for "Programming 1"?

**Solution**: Implement **recency rules** (e.g., programming subjects must be taken within 5 years).

### Challenge 3: Cross-Curriculum Complexity

- A shifter's old curriculum may no longer exist
- Subjects may have been renamed or restructured

**Solution**: Use **Subject Equivalence Groups** (already in Enrollify schema) to map old subjects to new ones.

### Challenge 4: Prerequisite Chain Disruption

- A credited subject may satisfy a prerequisite, but the student never actually learned the material
- E.g., "Data Structures" credited, but student struggles in "Algorithms" because they never took DS

**Solution**: Allow advisers to **recommend** (not require) retaking certain subjects; track "credit source" (transfer vs. taken).

---

## Proposed Enrollify Implementation

### New Tables/Modifications Needed

#### 1. StudentCreditEvaluations (Header Table)

```
StudentCreditEvaluations
├── Id (PK)
├── StudentId (FK → Students)
├── EvaluationType - 'TRANSFEREE', 'SHIFTER', 'RETURNEE'
├── SourceInstitution - Name of previous school (for transferees)
├── SourceCourseId (FK → Courses, nullable) - For shifters
├── TargetCurriculumId (FK → Curricula) - The curriculum being evaluated against
├── EvaluationDate
├── EvaluatedBy (FK → Users) - The evaluator
├── Status - 'DRAFT', 'PENDING_APPROVAL', 'APPROVED', 'REJECTED'
├── TotalUnitsEvaluated
├── TotalUnitsCredited
├── Remarks
├── Audit fields
```

#### 2. CreditEvaluationDetails (Line Items)

```
CreditEvaluationDetails
├── Id (PK)
├── CreditEvaluationId (FK → StudentCreditEvaluations)
├── SourceSubjectCode - Subject code from previous school/program
├── SourceSubjectTitle - Subject title from previous school/program
├── SourceUnits - Units of the source subject
├── SourceGrade - Grade obtained
├── TargetCurriculumSubjectId (FK → CurriculumSubjects, nullable) - Credited equivalent
├── CreditDecision - 'FULL_CREDIT', 'PARTIAL_CREDIT', 'NO_CREDIT', 'CONDITIONAL'
├── DecisionReason - Why this decision was made
├── Audit fields
```

#### 3. SubjectEquivalenceHistory (Track Past Decisions)

```
SubjectEquivalenceHistory
├── Id (PK)
├── SourceInstitution
├── SourceSubjectCode
├── SourceSubjectTitle
├── TargetSubjectId (FK → Subjects)
├── DecisionDate
├── DecisionOutcome - 'EQUIVALENT', 'NOT_EQUIVALENT', 'PARTIAL'
├── DecisionNotes
├── Audit fields
```

This table helps future evaluators see how similar subjects were evaluated in the past.

---

### Credit Evaluation Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                    CREDIT EVALUATION WORKFLOW                    │
└─────────────────────────────────────────────────────────────────┘

    ┌──────────────┐
    │   Student    │
    │  Submits TOR │
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐
    │  Admissions  │
    │   Screens    │
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐
    │  Department  │
    │  Evaluates   │◄──── Checks SubjectEquivalenceHistory
    │   Subjects   │      for past decisions
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐
    │   Generate   │
    │   Report     │
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐
    │   Assign     │
    │  Curriculum  │
    └──────┬───────┘
           │
           ▼
    ┌──────────────┐
    │   Student    │
    │  Enrolled    │
    └──────────────┘
```

---

### Prerequisite Handling for Credited Subjects

When a subject is credited:
1. The system marks the student as having "completed" that `CurriculumSubject`
2. This satisfies prerequisite checks for downstream subjects
3. The credit source is recorded: `TRANSFER_CREDIT`, `SHIFT_CREDIT`, `VALIDATION_EXAM`

```
StudentCompletedSubjects
├── Id (PK)
├── StudentId (FK → Students)
├── CurriculumSubjectId (FK → CurriculumSubjects)
├── CompletionType - 'ENROLLED', 'TRANSFER_CREDIT', 'SHIFT_CREDIT', 'VALIDATION_EXAM'
├── SourceCreditEvaluationDetailId (FK → CreditEvaluationDetails, nullable)
├── Grade (nullable - may not have grade for credited subjects)
├── CompletionDate
├── Audit fields
```

---

## Business Rules

1. **Credit decisions are final once approved** - Changes require a formal appeal process

2. **Credited subjects cannot be retaken for a higher grade** (unless academic policy allows)

3. **Maximum credit cap** - System should enforce maximum transferable units (configurable per course)

4. **Prerequisite satisfaction** - Credited subjects automatically satisfy prerequisites within the assigned curriculum

5. **Curriculum lock** - Once a student has an approved credit evaluation, changing their curriculum requires re-evaluation

6. **Audit trail** - All credit decisions must be logged with evaluator, date, and reasoning

---

## Shifter-Specific Considerations

### Scenario: Same Institution, Different Program

When a student shifts within the same institution:
- Their academic records already exist in the system
- Evaluation is simpler since subjects use the same subject codes
- Can leverage `SubjectEquivalence` table for cross-program mappings

### Handling Common Subjects

Many programs share common subjects (GE courses like English, Math, Filipino):
- These should be automatically credited
- System can detect if the student already passed a subject that exists in the new curriculum

### Year Level Placement

After credit evaluation, the system calculates:
- **Total credited units** ÷ **Units per year** = **Recommended year level**
- Adviser may override based on subject distribution

---

## Transferee-Specific Considerations

### Unknown Subject Codes

- Transferees bring subjects with codes unfamiliar to the system
- The `SourceSubjectCode` and `SourceSubjectTitle` fields capture this
- Manual mapping to `TargetCurriculumSubjectId` is required

### Grade Conversion

Different schools use different grading systems:
- Some use 1.0-5.0 (1.0 is highest)
- Some use letter grades (A, B, C)
- Some use percentage (75%, 80%, etc.)

**Consideration**: Add a `GradeConversionNotes` field or implement a grade conversion utility.

### Institution Verification

- System should maintain a list of **recognized institutions**
- Subjects from unrecognized schools may require additional validation

---

## User Stories for Implementation

1. **As a Department Evaluator**, I want to evaluate a transferee's subjects against our curriculum so that I can determine which subjects to credit.

2. **As a Department Evaluator**, I want to see how similar subjects were evaluated in the past so that I can maintain consistency.

3. **As an Academic Adviser**, I want to view a student's credit evaluation report so that I can advise them on their remaining subjects.

4. **As a Student**, I want to see which of my subjects were credited so that I know what I still need to take.

5. **As a Registrar**, I want to enforce maximum credit caps so that transferees complete a minimum number of subjects at our institution.

---

## Open Questions

1. **Should credit evaluations expire?** If a student is admitted but doesn't enroll for 2 years, should their evaluation still be valid?

2. **Partial credits**: How to handle subjects that are only partially equivalent? Create a new "bridge" subject?

3. **Cross-curriculum credits**: If a shifter was under Curriculum 2020 and shifts to a program with Curriculum 2024, how to map subjects that no longer exist?

4. **Appeals process**: How should a student appeal a credit evaluation decision?

5. **Batch evaluation**: Should there be a feature to evaluate multiple transferees from the same school using saved mappings?

---

## Related Documents

- [Curriculum-Based Prerequisites Design](./curriculum-based-prerequisites-design.md)
- [User Story: Subject Equivalents](../docs/user-stories/08-medium-equivalent-subjects.md)
