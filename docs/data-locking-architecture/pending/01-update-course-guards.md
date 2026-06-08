# UpdateCourse Guards - PARTIALLY IMPLEMENTED

## Overview
The `UpdateCourse` command lacks guards to prevent dangerous mutations when class sections reference the course. Currently, a course code can be changed while active class sections are using that course, which would silently change section names in UI displays.

## Status
⚠️ **PARTIALLY IMPLEMENTED** — Needs completion

## Current Implementation

### File Location
- **File:** `Enrollify.Application/Features/Courses/Commands/UpdateCourse.cs` (lines 30-52)

### Current Code
```csharp
public class UpdateCourse : ICommand<CourseDto>
{
    public int Id { get; init; }
    public string Code { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
}

public sealed class Handler : ICommandHandler<UpdateCourse, CourseDto>
{
    public async Task<Result<CourseDto>> Handle(
        UpdateCourse command, CancellationToken cancellationToken)
    {
        var existing = await _courseRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existing == null)
            return Result.NotFound();

        // ⚠️ NO GUARD HERE — should check for open/active/locked sections!
        
        existing.UpdateCourseCode(command.Code);
        existing.UpdateCourseName(command.Name);
        existing.UpdateCourseDescription(command.Description);

        _courseRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated(_mapper.Map<CourseDto>(existing));
    }
}
```

**Issue:** No validation to prevent course code or name changes when class sections are referencing the course.

## Missing Guards

### Guard 1: Prevent Code Changes During Active Usage

**Proposed Guard:**
```csharp
// Check if course is used in any open/locked/active sections
var activeSections = await _classSubscriptionRepository
    .ListAsync(
        new GetClassSectionsByCourseAndStatusSpec(
            command.Id,
            statusThreshold: ClassSectionStatusEnum.Open),
        cancellationToken);

if (activeSections.Any())
{
    return Result.Conflict(
        $"Course '{existing.Code}' is used in {activeSections.Count} open, locked, or active class section(s). " +
        $"Cannot change course code while those sections are active. " +
        $"Complete or cancel those sections before updating.");
}
```

**Condition:** Allow code change only if:
- No class sections reference the course, OR
- All sections referencing the course are in Draft or Completed status

**Why?** 
- `ClassSection.Name` is often derived from `Course.Code` (e.g., "BSCS-1A" = course code + year/section)
- If code changes to "CS" while section is called "BSCS-1A", the section name becomes stale
- Students see inconsistent course identity in their enrollment records

### Guard 2: Warn on Name Changes (Optional)

**Proposed Guard (Less Strict):**
```csharp
// Warn if course name is being changed while active sections exist
var allSections = await _classSubscriptionRepository
    .ListAsync(
        new GetClassSectionsByCourseSpec(command.Id),
        cancellationToken);

if (allSections.Any() && existing.Name != command.Name)
{
    // Log warning but allow the change
    _logger.LogWarning(
        "Course {CourseId} name is being changed from '{OldName}' to '{NewName}' " +
        "while {SectionCount} class sections reference it.",
        command.Id, existing.Name, command.Name, allSections.Count);
}
```

**Approach:** Allow the change but log it for audit purposes.

## Real-World Scenarios

### Scenario 1: Code Change During Active Term

**Current State:**
- BSCS (Bachelor of Science in Computer Science)
  - BSCS-1A section in Open status (students enrolling)
  - BSCS-1B section in Locked status (ready for term)
  - BSCS-2A section in Draft (next year)

**Action:** Admin typo-corrects course code from "BSCS" to "CS" (not allowed in reality, but checking logic)

**Current Behavior:** ✅ Allowed to change
**Problem:** Sections now show inconsistent names:
  - Section records still reference course ID but code is stale
  - UI displays might show "CS-1A" even though ClassSection.Name says "BSCS-1A"
  - Confusion for students and registrar

**Proposed Behavior:** ❌ Blocked
**Result:** `Result.Conflict()` with message: "Cannot change course code while 2 sections are active"

### Scenario 2: Code Change Before Any Sections

**Current State:**
- MATH (Mathematics) - no class sections created yet
- Plan to change code to "MTMTH" (comprehensive abbreviation)

**Current Behavior:** ✅ Allowed

**Proposed Behavior:** ✅ Still Allowed
**Reason:** No dependencies, safe to change

### Scenario 3: Code Change After Sections Complete

**Current State:**
- PHYS (Physics)
  - PHYS-1A completed 3 years ago
  - PHYS-2A completed 2 years ago
  - No active sections

**Action:** Admin corrects typo: "PHYS" → "PHYSICS"

**Current Behavior:** ✅ Allowed

**Proposed Behavior:** ✅ Still Allowed
**Reason:** All sections are Completed (grades final, historical record); change won't affect them

## Specification Needed

To implement this guard, the codebase needs:

**Specification Class:** `GetClassSectionsByCourseAndStatusSpec`
- **Input:** `courseId: int`, `statusThreshold: ClassSectionStatusEnum`
- **Output:** `IQueryable<ClassSection>` filtered by:
  - `ClassSection.CourseId == courseId`
  - `ClassSection.StatusId >= statusThreshold`

**Example:**
```csharp
public class GetClassSectionsByCourseAndStatusSpec : Specification<ClassSection>
{
    public GetClassSectionsByCourseAndStatusSpec(int courseId, ClassSectionStatusEnum statusThreshold)
    {
        Query
            .Where(s => s.CourseId == courseId && s.StatusId >= statusThreshold)
            .OrderBy(s => s.CreatedDate);
    }
}
```

## Implementation Steps

### Step 1: Create Specification
Create `Enrollify.Application/Specifications/ClassSections/GetClassSectionsByCourseAndStatusSpec.cs`

### Step 2: Add Guard to UpdateCourse Handler
Add the validation check before updating the course code:
```csharp
var activeSections = await _classSubscriptionRepository
    .ListAsync(new GetClassSectionsByCourseAndStatusSpec(command.Id, ClassSectionStatusEnum.Open), cancellationToken);

if (activeSections.Any())
{
    return Result.Conflict(...);  // See guard proposal above
}
```

### Step 3: Add Unit Tests
- Test that code change is blocked when Open sections exist
- Test that code change is blocked when Locked sections exist
- Test that code change is blocked when Active sections exist
- Test that code change is allowed when only Draft sections exist
- Test that code change is allowed when no sections exist
- Test that code change is allowed when only Completed sections exist

### Step 4: Verify WebAPI Endpoint
- Confirm 409 Conflict response when guard fails
- Add to API documentation

## Guard Matrix

| Condition | Current | Proposed | Result |
|-----------|---------|----------|--------|
| No sections reference course | ✅ Allow | ✅ Allow | Code can change |
| Course used in Draft sections only | ✅ Allow | ✅ Allow | Code can change |
| Course used in Open/Locked/Active sections | ✅ Allow | ❌ Block | Conflict error |
| Course used in Completed sections only | ✅ Allow | ✅ Allow | Code can change (historical) |

## Related Guards

- **Subject Updates:** Blocked if used in Open/Active sections (see `done/05-subject-guards.md`)
- **AcademicYear Updates:** Blocked if has Active/Completed sections (see `done/06-academic-year-guards.md`)
- **ClassSection Transitions:** Blocked if trying to change course after Open (see `done/03-class-section-status-machine.md`)

## Testing Approach

### Unit Test Template
```csharp
[Fact(DisplayName = "UpdateCourse blocks code change when sections in Open status")]
public async Task UpdateCourse_SectionOpen_ReturnsConflict()
{
    // Arrange: Create course with Open section
    var course = new Course(code: "BSCS", name: "BS Computer Science");
    var section = new ClassSection(...) { StatusId = ClassSectionStatusEnum.Open };
    // ... setup ...

    var command = new UpdateCourse
    {
        Id = course.Id,
        Code = "CS",  // Attempting to change code
        Name = course.Name
    };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsConflict);
    Assert.Contains("Cannot change course code", result.Errors[0].Message);
}
```

## Notes

- ⚠️ **MUST IMPLEMENT BEFORE:** Students are actively enrolling in courses
- The principle mirrors Subject/AcademicYear guards
- Specification pattern allows reuse in other queries
- Error message should be specific and actionable
- Consider adding section count to error for visibility into impact
