# Bulk Initialize Class Sections Implementation Plan

## Problem Statement

The existing `BulkInitializeClassSectionsForAcademicYear` command has an incomplete implementation. The current design assumes that each course should have the same number of sections across all terms in an academic year, which is not realistic for university enrollment scenarios.

**Design Decision (from user clarification):**
- Switch to **per-term selection** instead of full academic year
- Users will initialize class sections for ONE specific academic term at a time
- Users will select ONE year level at a time
- This simplifies the UX and better matches the enrollment workflow

## Approach

### Backend Changes

1. **Refactor Command Structure**
   - Change `BulkInitializeClassSectionsForAcademicYear` to accept a single `AcademicTermId` instead of `AcademicYearId`
   - Add `YearLevel` parameter to the payload (since we initialize one year level at a time)
   - Update Payload model to:
     ```csharp
     public sealed record Payload(
         CourseId courseId,
         CurriculumId curriculumId,
         int numberOfSections);
     ```
   - Update Command model to:
     ```csharp
     public sealed record Command(
         AcademicTermId academicTermId,
         YearLevel yearLevel,
         List<Payload> requestPayload) : ICommand<Result>;
     ```

2. **Complete Handler Implementation**
   - Follow the pattern from `CreateClassSection.cs` (line 100-109 shows the section naming convention)
   - For each payload entry:
     - Get the course
     - Get the academic term
     - Validate curriculum belongs to the course
     - Get existing class sections for that course/term/year level to determine starting section code
     - For each section number (1 to numberOfSections):
       - Generate section code using `GetNextSectionCode()` extension (line 160 in CreateClassSection.cs)
       - Create ClassSection with auto-generated name: `{CourseCode}-{YearLevel}{SectionCode}` (e.g., "BSCS-1A")
       - Create associated ClassSectionSubjectOfferings based on curriculum subjects
   - Use a transaction to ensure atomicity

3. **Update Validator**
   - Change validation to check for `AcademicTermId` instead of `AcademicYearId`
   - Remove the check for "academic year has no terms" (line 105-109) since we're now working with a term directly
   - Add validation for YearLevel
   - Keep curriculum-course relationship validation (line 119-123)

4. **Create Repositories/Specs if needed**
   - Reuse `GetExistingClassSectionsByCourseYearLevelAndTerm` spec from CreateClassSection (line 157)
   - Reuse `GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTermSpec` (line 166)

5. **Add WebAPI Endpoint**
   - Create FastEndpoint in `Enrollify.WebAPI/Features/ClassSections/`
   - Map request model to command
   - Apply authorization policy (check existing ClassSections policies)
   - Return appropriate HTTP responses

### Frontend Changes

1. **Create Bulk Initialize Form Component**
   - Location: `src/page-components/sections-management-page/bulk-initialize-sections-drawer.tsx`
   - Form fields:
     - **Academic Term Selector**: Dropdown/Combobox to select academic term (fetch from academic-years API with terms included)
     - **Year Level Selector**: Dropdown (1-6 based on course duration)
     - **Courses Multi-Select**: Table or multi-select combobox for courses
     - **Section Count per Course**: For each selected course, input number of sections (default: 1)
     - **Curriculum display**: Show auto-selected latest active curriculum per course (read-only, informational)

2. **Wire up API Collection**
   - Add mutation in `src/api/collections/class-sections-collection.ts`
   - Define request/response types in `src/api/models/class-section-models.ts`
   - Use `createMutationOptions` pattern with query invalidation

3. **Integrate with Sections Management Page**
   - Add "Bulk Initialize Sections" button in the page header
   - Open the drawer when clicked
   - Show success/error toast after mutation
   - Invalidate class sections query to refresh table

4. **Add Authorization Check**
   - Use `useAuthorization` hook to check if user has create class section permission
   - Disable/hide bulk initialize button if unauthorized

## Key Files to Change

### Backend
- `Enrollify.Application/Features/ClassSections/Commands/BulkInitializeClassSectionsForAcademicYear.cs` - Complete handler implementation
- `Enrollify.Application/Features/ClassSections/Validators/BulkInitializeClassSectionsForAcademicYearValidator.cs` - Update validation logic
- `Enrollify.WebAPI/Features/ClassSections/` - Create new endpoint (or add to existing endpoint group)

### Frontend
- `src/api/collections/class-sections-collection.ts` - Add bulk initialize mutation
- `src/api/models/class-section-models.ts` - Add request/response types
- `src/page-components/sections-management-page/bulk-initialize-sections-drawer.tsx` - NEW file
- `src/page-components/sections-management-page/index.tsx` - Integrate drawer trigger

## Implementation Notes

1. **Section Code Generation**: Use the existing `GetNextSectionCode()` extension from `SectionCodeExtensions.cs` which increments from 'A' onwards (A, B, C, etc.)

2. **Section Naming Convention**: Follow the pattern from CreateClassSection: `{CourseCode}-{YearLevel}{SectionCode}` (e.g., "BSCS-1A", "BSCS-1B")

3. **Curriculum Selection**: The system should automatically select the latest active curriculum for each course, matching the logic in `CreateClassSection.cs` (line 163-175). If no active curriculum exists for a course, fail validation for that course.

4. **Subject Offerings**: For each created class section, automatically create ClassSectionSubjectOfferings for all subjects in the curriculum for that year level and term (following CreateClassSection pattern, lines 119-134)

5. **Transaction Handling**: Wrap all database operations in a transaction to ensure atomicity (if any section or subject offering creation fails, rollback everything)

6. **Error Handling**:
   - If curriculum not found for any course, return validation error before creating any sections
   - If any section creation fails, rollback entire transaction
   - Return detailed error messages indicating which course failed and why

7. **Frontend UX**:
   - Show a confirmation dialog before bulk creation showing: Term, Year Level, Total sections to be created
   - Show a loading state during creation
   - On success, show summary: "Created {N} sections for {M} courses"
   - On error, show which courses failed and why

## Testing Strategy

### Backend
- Unit tests for the command handler
- Unit tests for the updated validator
- Integration tests for the endpoint

### Frontend
- Verify form validation works
- Verify API integration with mock data
- Verify authorization checks
- Manual testing of the full flow

## Considerations

- **Performance**: For large bulk operations (e.g., 20 courses × 5 sections = 100 class sections), consider adding progress feedback or background job processing in future iterations
- **Duplicate Prevention**: The validator should check if sections already exist for the selected term/year/course combination
- **Idempotency**: Consider what happens if the user runs bulk initialize twice for the same term/year/course
