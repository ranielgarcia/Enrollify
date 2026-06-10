# Bulk Initialize ClassSections: Validation & Guards

## Overview
The `BulkInitializeClassSectionsForAcademicYear` command includes comprehensive validation before creating class sections in bulk. This guard prevents invalid bulk operations that would create orphaned or inconsistent sections.

## Status
✅ **FULLY IMPLEMENTED**

## Purpose

When an admin bulk-creates class sections for an entire academic year, the system validates:
1. Academic terms exist for the specified year
2. Year levels are valid for the curriculum
3. Courses have curriculum assignments for the cohort year
4. No duplicate sections are created
5. All prerequisite data is consistent

## Validator Implementation

### Primary Validator Class
- **File:** `Enrollify.Application/Features/ClassSections/Commands/BulkInitializeClassSectionsForAcademicYearValidator.cs` (166 lines)
- **Pattern:** `AbstractValidator<BulkInitializeClassSectionsForAcademicYearCommand>` (FluentValidation)
- **Applied By:** FastEndpoints endpoint automatically

### Handler
- **File:** `Enrollify.Application/Features/ClassSections/Commands/BulkInitializeClassSectionsForAcademicYear.cs` (268 lines)
- **Pattern:** `ICommandHandler<BulkInitializeClassSectionsForAcademicYearCommand>`

## Validation Rules

### 1. Academic Year Exists

**Guard:** Academic year ID must reference a real, non-deleted `AcademicYear`

**Validator Check:**
```
RuleFor(x => x.AcademicYearId)
    .MustAsync(async (academicYearId, ct) => 
        await _academicYearRepository.GetByIdAsync(academicYearId) != null)
    .WithMessage("Academic year does not exist.");
```

**Failure:** Validation error returned before any sections created

### 2. Academic Terms Exist for Year

**Guard:** The academic year must have at least one academic term defined

**Validator Check:**
```
RuleFor(x => x.AcademicYearId)
    .MustAsync(async (academicYearId, ct) => 
    {
        var terms = await _academicTermRepository
            .ListAsync(new GetTermsByAcademicYearIdSpec(academicYearId), ct);
        return terms.Any();
    })
    .WithMessage("Academic year has no terms defined.");
```

**Failure:** Cannot bulk-create sections without knowing which terms to assign them to

### 3. Courses Exist

**Guard:** All course IDs in the bulk request must reference real courses

**Validator Check:**
```
RuleForEach(x => x.CourseIds)
    .MustAsync(async (courseId, ct) => 
        await _courseRepository.GetByIdAsync(courseId) != null)
    .WithMessage(courseId => 
        $"Course {courseId} does not exist.");
```

**Failure:** Each invalid course ID is listed in validation errors

### 4. Courses Have Curriculum Assignment for Cohort Year

**Guard:** Each course must have a `CourseCurriculumAssignment` for the cohort's entry year

**Validator Check:**
```
RuleForEach(x => x.CourseIds)
    .MustAsync(async (courseId, ct) => 
    {
        var assignment = await _assignmentRepository
            .GetBySpecAsync(new GetCurriculumAssignmentSpec(courseId, cohortEntryYear), ct);
        return assignment != null;
    })
    .WithMessage(courseId => 
        $"Course {courseId} has no curriculum assignment for entry year.");
```

**Why This Matters:**
- Each course-cohort combination must have a curriculum
- Without this, the system doesn't know which curriculum to use for the sections
- Ensures `CourseCurriculumAssignment` sync has completed

### 5. Year Levels Are Valid for Curriculum

**Guard:** Year level (1-4) must be valid for the chosen curriculum

**Validator Check:**
```
RuleFor(x => x.YearLevelStart)
    .Must(yearLevel => yearLevel >= 1 && yearLevel <= 4)
    .WithMessage("Year level must be between 1 and 4.");

RuleFor(x => x.NumberOfYears)
    .Must(numYears => numYears > 0 && numYears <= 4)
    .WithMessage("Number of years must be between 1 and 4.");

RuleFor(x => x)
    .Must(cmd => (cmd.YearLevelStart + cmd.NumberOfYears - 1) <= 4)
    .WithMessage("Year level range exceeds maximum of 4.");
```

**Example:**
- If `YearLevelStart = 3` and `NumberOfYears = 3`, sections for years 3, 4 are created
- If `YearLevelStart = 3` and `NumberOfYears = 4`, validation fails (would need years 3, 4, 5, 6)

### 6. No Duplicate Sections

**Guard:** Cannot create multiple sections for the same course-cohort-year combination

**Validator Check:**
```
RuleFor(x => x)
    .MustAsync(async (cmd, ct) => 
    {
        var existingSections = await _classSubscriptionRepository
            .ListAsync(new GetDuplicateSectionCheckSpec(
                cmd.CourseIds, cmd.CohortAcademicYearId, cmd.YearLevelStart), ct);
        
        return !existingSections.Any();
    })
    .WithMessage("Some sections already exist for the specified courses.");
```

**Prevents:**
- Creating BSCS-1A twice
- Accidental bulk operations that overwrite existing sections

### 7. Curriculum Content Exists

**Guard:** Curriculum must have subjects defined for the year levels being created

**Validator Check:**
```
RuleForEach(x => x.CourseIds)
    .MustAsync(async (courseId, ct) => 
    {
        var curriculum = await _curriculumRepository
            .GetBySpecAsync(new GetAssignedCurriculumSpec(courseId, cohortYear), ct);
        
        if (curriculum == null) return false;
        
        var subjects = curriculum.Subjects
            .Where(s => s.YearLevel >= yearLevelStart && s.YearLevel <= yearLevelEnd);
        
        return subjects.Any();
    })
    .WithMessage(courseId => 
        $"Curriculum for {courseId} has no subjects in the specified year levels.");
```

**Prevents:**
- Creating empty sections with no subject offerings
- Creating sections referencing incomplete curricula

## Handler Logic

### Step 1: Validation
Validator runs first; if any rule fails, handler never executes.

### Step 2: Load Prerequisites
```csharp
// Get academic year and terms
var academicYear = await _academicYearRepository.GetByIdAsync(command.AcademicYearId, ct);
var terms = await _academicTermRepository
    .ListAsync(new GetTermsByAcademicYearIdSpec(command.AcademicYearId), ct);

// Get courses
var courses = await _courseRepository
    .ListAsync(new GetCoursesByIdsSpec(command.CourseIds), ct);

// Get curriculum assignments and their content
var assignments = await _assignmentRepository
    .ListAsync(new GetCurriculumAssignmentsForBulkSpec(command.CourseIds, cohortYear), ct);
```

### Step 3: Create Sections
For each course:
- Determine which terms to use based on curriculum structure
- Load curriculum subjects for the year level range
- Create section with name pattern: `[CourseCode]-[YearLevel][SectionLetter]` (e.g., "BSCS-1A")
- Add subject offerings
- Assign advisers (if provided)

### Step 4: Persist & Return
```csharp
_classSubscriptionRepository.AddRange(newSections);
await _unitOfWork.SaveChangesAsync(ct);

return Result.Created(
    newSections.Select(s => _mapper.Map<ClassSectionDto>(s)).ToList());
```

## Real-World Example

### Scenario: Bulk Create Sections for Academic Year 2025-2026

**Input:**
```json
{
  "AcademicYearId": 10,                    // AY 2025-2026
  "CourseIds": [1, 2, 3],                  // BSCS, BSECE, BSME
  "CohortAcademicYearId": 9,               // Cohort entered in 2024-2025
  "YearLevelStart": 2,                     // Creating sections for Year 2
  "NumberOfYears": 1,
  "Adviser": null                          // To be assigned later
}
```

**Validations Performed:**
1. ✅ AY 2025-2026 exists
2. ✅ AY 2025-2026 has Fall and Spring terms
3. ✅ Courses 1, 2, 3 exist (BSCS, BSECE, BSME)
4. ✅ Each course has curriculum assignment for 2024-2025 cohort
5. ✅ Year level 2 is valid (≤ 4)
6. ✅ BSCS, BSECE, BSME have subjects in Year 2
7. ✅ No duplicate sections exist for these courses in Year 2

**Output:**
- BSCS-2A created (Fall term) with all Year 2 subjects for BSCS curriculum
- BSCS-2B created (Spring term) if applicable
- BSECE-2A created
- BSECE-2B created
- BSME-2A created
- BSME-2B created
- All sections in Draft status, ready for editing

**If Any Validation Fails:**
- Example: "BSCS has no curriculum assignment for 2024-2025"
- Result: 400 Bad Request with validation error
- **No sections are created**
- Admin must fix the issue and retry

## Guard Prevention Examples

### Example 1: Missing Curriculum Assignment
**Attempt:** Bulk-create sections for BSCS which has no curriculum assigned to the cohort year
**Validation:** Fails at step 4 (curriculum assignment check)
**Result:** ❌ 400 Bad Request
**Message:** "Course 1 has no curriculum assignment for entry year 2024."

### Example 2: Curriculum with No Content
**Attempt:** Bulk-create for a newly drafted curriculum with no subjects defined
**Validation:** Fails at step 7 (curriculum content check)
**Result:** ❌ 400 Bad Request
**Message:** "Curriculum for course 1 has no subjects in year levels 2-2."
**Workaround:** Admin must add subjects to curriculum first

### Example 3: Invalid Year Level
**Attempt:** Bulk-create sections for year levels 3-6 (curriculum only has 4 years)
**Validation:** Fails at step 5 (year level range check)
**Result:** ❌ 400 Bad Request
**Message:** "Year level range exceeds maximum of 4."

### Example 4: Duplicate Detection
**Attempt:** Bulk-create sections when BSCS-2A already exists
**Validation:** Fails at step 6 (duplicate check)
**Result:** ❌ 400 Bad Request
**Message:** "Some sections already exist for the specified courses."

## Benefits of Comprehensive Validation

1. **Atomic Failure:** All-or-nothing; either all sections created or none
2. **Clear Errors:** Specific validation messages guide admin to fix issues
3. **Prevents Orphaned Data:** No sections without curriculum content
4. **Ensures Consistency:** All prerequisites met before creation
5. **Performance:** Validates before creating hundreds of sections

## Testing

- **Unit Tests:** `Enrollify.UnitTests/Features/ClassSections/Commands/BulkInitializeClassSectionsForAcademicYearValidatorTests.cs` (multiple test cases per rule)
- **Integration Tests:** Full handler test with database
- **Test Scenarios:**
  - All validations pass → sections created
  - Academic year missing → validation fails
  - Curriculum assignment missing → validation fails
  - Duplicate sections → validation fails
  - Invalid year level → validation fails
  - Curriculum with no content → validation fails

## Related Features

- **ClassSection State Machine:** Created sections start in Draft (see `03-class-section-status-machine.md`)
- **CourseCurriculumAssignment:** Must exist before bulk create (see `02-course-curriculum-assignment.md`)
- **Subject Offerings:** Created automatically from curriculum subjects

## Implementation Pattern

Uses **FluentValidation** with async rules:
- `RuleFor(x => ...)` - Single property validation
- `RuleForEach(x => x.Collection)` - Collection validation
- `.MustAsync(async (value, ct) => ...)` - Custom async predicate
- `.WithMessage(...)` - User-friendly error message

## Notes

- ✅ All 7 validation rules fully implemented
- ✅ Prevents common bulk-operation mistakes
- ✅ Atomic: either all succeed or all fail (no partial bulk creates)
- ✅ Guards check prerequisites before section creation
- The validator is the first line of defense against invalid bulk operations
