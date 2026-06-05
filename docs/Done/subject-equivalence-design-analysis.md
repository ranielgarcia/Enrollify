# Subject Equivalence Design Analysis

## Overview

This document analyzes whether `SubjectEquivalence` should be curriculum-scoped (like `CurriculumSubjectPrerequisites`) or remain at the subject level. After careful analysis, the recommendation is to **keep equivalence at the subject level** with minor enhancements.

---

## Current Design

```
SubjectEquivalenceGroups
├── Id (PK)
├── Name - Group identifier (e.g., "Basic English Group", "Intro Programming Group")
└── Audit fields

SubjectEquivalence
├── Id (PK)
├── SubjectId (FK → Subjects)
├── EquivalenceGroupId (FK → SubjectEquivalenceGroups)
└── Audit fields
```

**How it works**: Subjects in the same equivalence group are considered equivalent. If Subject A and Subject B are both in "Intro Programming Group", passing either one satisfies the requirement.

---

## Key Question: Should Equivalence Be Curriculum-Scoped?

### What Curriculum-Scoped Equivalence Would Look Like

```
CurriculumSubjectEquivalence
├── CurriculumSubjectId (FK → CurriculumSubjects)
├── EquivalentCurriculumSubjectId (FK → CurriculumSubjects)
└── Audit fields
```

This would mean: "Programming 1 in BSIT Curriculum 2024 is equivalent to Programming 1 in BSCS Curriculum 2024"

---

## Analysis: Prerequisites vs. Equivalence

| Aspect | Prerequisites | Equivalence |
|--------|---------------|-------------|
| **Scope** | Within a single curriculum | Across programs/institutions |
| **Change Frequency** | Changes with curriculum revisions | Rarely changes (content-based) |
| **Primary Use Case** | Enrollment validation | Credit transfer, cross-program recognition |
| **Affected by Curriculum Version** | Yes - different curricula have different prerequisites | Rarely - subject content determines equivalence |
| **External Institution Support** | N/A | Must support external subjects (no curriculum) |

---

## Why Prerequisites Needed Curriculum Scoping

1. **Regulatory changes** (CHED CMOs) modify prerequisite requirements
2. **Student grandfathering** - old students follow old prerequisites
3. **Curriculum revisions** add/remove prerequisites
4. **Same subject, different rules** per curriculum version

---

## Why Equivalence Should Stay at Subject Level

### Reason 1: Equivalence is Content-Based, Not Time-Based

Subject equivalence is determined by **what the subject covers**, not **when it was taken**.

**Example**:
- "Programming 1 (Java)" from any year is equivalent to "Programming 1 (Python)" from any year
- The content overlap (basic programming concepts) doesn't change based on curriculum version

### Reason 2: Cross-Program Recognition

Equivalence often spans multiple programs that may have different curriculum timelines.

**Example**:
- BSIT Curriculum 2020 has "English 101"
- BSCS Curriculum 2024 has "English 101"
- Both are the same subject (same content) - equivalence is at the subject level

### Reason 3: Transferee/External Institution Support

Transferees bring subjects from external institutions that have **no curriculum in our system**.

**Example**:
- A student transfers with "PROG101 - Introduction to Programming" from University X
- We need to map this to our "CC101 - Programming 1"
- There's no "curriculum" for University X in our system

If equivalence were curriculum-scoped, we couldn't handle external subjects.

### Reason 4: Avoiding Over-Engineering

Curriculum-scoped equivalence would require:
- Maintaining equivalence mappings for every curriculum version
- Complex queries to find equivalents across curricula
- Potential data inconsistencies

**The complexity cost outweighs the benefit.**

### Reason 5: Equivalence Groups Handle Cross-Curriculum Naturally

With the current design:
```
SubjectEquivalenceGroup: "Basic Programming"
├── CC101 (BSIT subject)
├── CS101 (BSCS subject)  
├── IT101 (BSIS subject)
└── PROG101 (from external school, stored in credit evaluation)
```

All subjects in the group are automatically equivalent to each other, regardless of which curriculum they belong to.

---

## When Would Curriculum-Scoped Equivalence Be Needed?

Only in rare edge cases:

1. **Significant content changes**: A subject code is reused but the content changed drastically (e.g., "Web Development" in 2015 vs. 2024 are very different)

2. **Unit changes**: "Math 101" was 3 units in 2020 but is now 5 units - are they still equivalent?

**Solution**: Handle these as **subject versioning** rather than curriculum-scoped equivalence:
- Create a new subject: "Web Development (Legacy)" vs "Web Development"
- Don't put them in the same equivalence group

---

## Recommended Approach: Keep Subject-Level with Enhancements

### Enhancement 1: Add Effective Date Range (Optional)

```sql
ALTER TABLE SubjectEquivalence ADD
    EffectiveFrom DATE NULL,           -- When this equivalence started
    EffectiveTo DATE NULL;             -- When this equivalence ended (NULL = still active)
```

This allows:
- Deprecating old equivalences without deleting them
- Querying historical equivalences for audit purposes

### Enhancement 2: Add Equivalence Type

```sql
ALTER TABLE SubjectEquivalenceGroups ADD
    EquivalenceType VARCHAR(20) NOT NULL DEFAULT 'INTERNAL';
    -- 'INTERNAL' - Within institution (cross-program)
    -- 'EXTERNAL' - With external institution subjects
    -- 'LEGACY' - For old/deprecated subjects
```

### Enhancement 3: Add Notes/Justification

```sql
ALTER TABLE SubjectEquivalence ADD
    Notes VARCHAR(500) NULL;           -- Why this equivalence was established
```

---

## How Equivalence Works with Curriculum-Based Prerequisites

### Scenario: Prerequisite Satisfaction via Equivalent Subject

**Setup**:
- BSIT Curriculum 2024: "Data Structures" requires "Programming 1" (CC101)
- Student passed "CS101" (BSCS version of Programming 1)
- CC101 and CS101 are in the same equivalence group

**Prerequisite Check Logic**:
1. Get the student's curriculum: BSIT Curriculum 2024
2. Find prerequisites for "Data Structures": requires CC101
3. Check if student passed CC101: **No**
4. Check if student passed any subject equivalent to CC101: **Yes (CS101)**
5. **Prerequisite satisfied** ✓

```
┌─────────────────────────────────────────────────────────────────┐
│              PREREQUISITE CHECK WITH EQUIVALENCE                │
└─────────────────────────────────────────────────────────────────┘

Student wants to enroll in: Data Structures (CurriculumSubject #45)
                                    │
                                    ▼
                    ┌───────────────────────────────┐
                    │ Get Prerequisites from        │
                    │ CurriculumSubjectPrerequisites│
                    └───────────────┬───────────────┘
                                    │
                                    ▼
                    Prerequisite: Programming 1 (CC101)
                                    │
                                    ▼
                    ┌───────────────────────────────┐
                    │ Did student pass CC101?       │
                    └───────────────┬───────────────┘
                                    │
                        ┌───────────┴───────────┐
                        │                       │
                       YES                      NO
                        │                       │
                        ▼                       ▼
                    ✓ SATISFIED     ┌───────────────────────────┐
                                    │ Find subjects equivalent  │
                                    │ to CC101 via              │
                                    │ SubjectEquivalenceGroups  │
                                    └───────────────┬───────────┘
                                                    │
                                                    ▼
                                    Equivalents: CS101, IT101
                                                    │
                                                    ▼
                                    ┌───────────────────────────┐
                                    │ Did student pass any of   │
                                    │ these equivalents?        │
                                    └───────────────┬───────────┘
                                                    │
                                        ┌───────────┴───────────┐
                                        │                       │
                                       YES                      NO
                                        │                       │
                                        ▼                       ▼
                                    ✓ SATISFIED           ✗ NOT MET
```

---

## Database Design Decision

### Final Recommendation: NO changes to SubjectEquivalence tables

The current design is appropriate because:

1. ✅ Equivalence is fundamentally subject-based, not curriculum-based
2. ✅ Current group-based design handles cross-program equivalence well
3. ✅ Supports external institution subjects (for transferees)
4. ✅ Works with curriculum-based prerequisites via equivalence lookup
5. ✅ Avoids unnecessary complexity

### Optional Future Enhancements

If needed later, consider adding:
- `EffectiveFrom`/`EffectiveTo` dates for temporal validity
- `EquivalenceType` for categorization
- `Notes` for documentation

---

## Comparison: Prerequisites vs. Equivalence Design

| Feature | Prerequisites | Equivalence |
|---------|---------------|-------------|
| **Table Structure** | CurriculumSubjectPrerequisites | SubjectEquivalence |
| **Scoped To** | Curriculum | Subject (global) |
| **Why This Scope** | Rules change per curriculum | Content doesn't change |
| **Student Impact** | Different students have different rules | All students see same equivalences |
| **External Support** | N/A | Yes (transferees) |

---

## Use Cases Validated

### Use Case 1: Cross-Program Course Sharing ✓
- GE subjects (English, Math, Filipino) shared across programs
- Same subject code, same equivalence group
- Works at subject level

### Use Case 2: Transferee Credit Evaluation ✓
- External subjects mapped to internal equivalents
- No curriculum needed for external subjects
- Works at subject level

### Use Case 3: Shifter Recognition ✓
- Student shifts from BSCS to BSIT
- CS101 (passed) equivalent to CC101 (required in new curriculum)
- Equivalence lookup satisfies prerequisite
- Works at subject level

### Use Case 4: Legacy Subject Handling ✓
- Old subject "COBOL Programming" phased out
- Create equivalence group with new "Programming Fundamentals"
- Old students' records still valid
- Works at subject level

---

## Conclusion

**Subject Equivalence should remain at the subject level** because:

1. It serves a fundamentally different purpose than prerequisites
2. It needs to support external institutions (no curriculum context)
3. The current group-based design is flexible and sufficient
4. Curriculum-scoping would add complexity without significant benefit

The existing `SubjectEquivalenceGroups` and `SubjectEquivalence` tables are well-designed for their purpose.

---

## Related Documents

- [Curriculum-Based Prerequisites Design](./curriculum-based-prerequisites-design.md)
- [Transferee Credit Evaluation Design](./transferee-credit-evaluation-design.md)
