# Class Scheduling Conflict Detection — Remaining Tasks

**Last Updated:** 2026-06-06  
**Current Status:** Phase 1 Complete (Core conflict detection with soft warnings)  
**Next:** Testing, Phase 2 (Data Integrity), Phase 3 (Soft Conflicts & Dashboard)

---

## ✅ What's Already Done (Phase 1)

- ✅ Core conflict detection service (`ScheduleConflictDetector`)
- ✅ Shared conflict detection helper (`ConflictDetectionHelper`)
- ✅ 5 Dapper repository methods for conflict queries
- ✅ HC-01: Teacher Double-Booked (Error)
- ✅ HC-02: Room Double-Booked (Error)
- ✅ HC-03: Section Overlap (Warning)
- ✅ DI-05: Duplicate Subject in Section (Error)
- ✅ API endpoints return conflicts[] in responses (HTTP 201 + conflicts)
- ✅ Section detail endpoint (`GET /api/class-sections/{id}`) embeds conflicts
- ✅ Consolidated endpoints (removed redundant offerings query)
- ✅ Configuration: `AcademicCoreSettings` with scheduling constraints
- ✅ SNAPSHOT isolation already enabled database-wide

---

## 📋 Remaining Tasks

### Phase 1A: Testing & Verification (IMMEDIATE)

**Priority:** HIGH  
**Estimated Time:** 2-4 hours

#### 1. Manual API Testing

**Objective:** Verify conflict detection works end-to-end via Postman/Swagger

**Steps:**

1. **Start backend:** `dotnet run --project Enrollify.WebAPI`
2. **Navigate to Swagger:** `https://localhost:7107/swagger`
3. **Create test scenario:**
   - Create a class section with 2 offerings (Subject A, Subject B)
   - Assign Teacher X to Subject A
   - Assign Teacher X to Subject B
   - Add schedule to Subject A: Monday 08:00-09:00
   - Add schedule to Subject B: Monday 08:30-09:30
4. **Expected result:** HTTP 201 + conflicts array with `TEACHER_DOUBLE_BOOKED` (Error)
5. **Test HC-02 (Room):** Assign same room to both offerings with overlapping schedules
6. **Test HC-03 (Section):** Create overlapping schedules within same section
7. **Test DI-05 (Duplicate):** Assign same subject twice to a section

**Verification Checklist:**

```
☐ POST /api/subject-offerings/{id}/schedules returns HTTP 201
☐ Response includes conflicts[] array
☐ Conflicts have correct type (TEACHER_DOUBLE_BOOKED, ROOM_DOUBLE_BOOKED, etc.)
☐ Conflicts have correct severity (Error, Warning)
☐ Conflicts include affectedOfferings[] with detailed info
☐ GET /api/class-sections/{id} includes offerings with embedded conflicts[]
☐ Frontend OfferingSchema.conflicts field populates correctly
```

**Acceptance Criteria:**
- All 4 conflict types detected correctly
- Response format matches `ConflictResultDto` schema
- No errors in backend logs
- Frontend can deserialize conflicts without type errors

---

#### 2. Unit Tests

**Objective:** Test `ScheduleConflictDetector` domain service in isolation

**File to Create:** `Enrollify.UnitTests/Core/Services/ScheduleConflictDetectorTests.cs`

**Test Cases:**

```csharp
namespace Enrollify.UnitTests.Core.Services;

public class ScheduleConflictDetectorTests
{
    // HC-01: Teacher Double-Booked
    [Fact(DisplayName = "Detects teacher double-booking with overlapping schedules")]
    public void DetectConflicts_TeacherDoubleBooked_ReturnsError() { }

    [Fact(DisplayName = "No conflict when teacher has back-to-back classes (no overlap)")]
    public void DetectConflicts_TeacherBackToBack_NoConflict() { }

    [Fact(DisplayName = "Detects teacher double-booking across different sections")]
    public void DetectConflicts_TeacherAcrossSections_ReturnsError() { }

    // HC-02: Room Double-Booked
    [Fact(DisplayName = "Detects room double-booking with overlapping schedules")]
    public void DetectConflicts_RoomDoubleBooked_ReturnsError() { }

    [Fact(DisplayName = "No conflict when room used in different time slots")]
    public void DetectConflicts_RoomDifferentTimes_NoConflict() { }

    // HC-03: Section Overlap
    [Fact(DisplayName = "Detects section overlap and returns warning severity")]
    public void DetectConflicts_SectionOverlap_ReturnsWarning() { }

    [Fact(DisplayName = "No conflict when section has non-overlapping schedules")]
    public void DetectConflicts_SectionNoOverlap_NoConflict() { }

    // DI-05: Duplicate Subject
    [Fact(DisplayName = "Detects duplicate subject in same section")]
    public void DetectConflicts_DuplicateSubject_ReturnsError() { }

    [Fact(DisplayName = "Allows same subject in different sections")]
    public void DetectConflicts_SameSubjectDifferentSections_NoConflict() { }

    // Edge Cases
    [Fact(DisplayName = "Handles empty schedule list without errors")]
    public void DetectConflicts_EmptySchedules_ReturnsEmptyList() { }

    [Fact(DisplayName = "Handles single schedule without conflicts")]
    public void DetectConflicts_SingleSchedule_NoConflict() { }

    [Fact(DisplayName = "Returns multiple conflicts when multiple violations exist")]
    public void DetectConflicts_MultipleConflicts_ReturnsAll() { }
}
```

**Estimated Time:** 3-4 hours

**Run Tests:**
```bash
cd src/system/EnrollifyBackend
dotnet test --filter "ScheduleConflictDetectorTests"
```

---

#### 3. Integration Tests

**Objective:** Test conflict detection with real database and full application stack

**Reference:** `Enrollify.IntegrationTests/README.md` for architecture patterns

**Files to Create:**

##### A. Application Layer Tests

**File:** `Enrollify.IntegrationTests/_Tests/Application/ClassSchedules/ConflictDetectionTests.cs`

**Test Collection:** `[Collection("Application")]`

**Test Cases:**

```csharp
namespace Enrollify.IntegrationTests._Tests.Application.ClassSchedules;

[Collection("Application")]
public class ConflictDetectionTests : IAsyncLifetime
{
    private readonly ApplicationTestFixture _fixture;
    private readonly IMediator _mediator;

    public ConflictDetectionTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _mediator = fixture.Mediator;
    }

    [Fact(DisplayName = "Add schedules with teacher conflict returns HTTP 201 with conflicts")]
    public async Task AddSchedules_TeacherConflict_ReturnsConflictsInResponse() 
    {
        // Arrange: Create section, 2 offerings, same teacher
        // Act: Add overlapping schedules
        // Assert: Result.IsSuccess = true, Conflicts.Count > 0, Conflicts[0].Type = TEACHER_DOUBLE_BOOKED
    }

    [Fact(DisplayName = "Get section by ID embeds conflicts in offerings")]
    public async Task GetSectionById_WithConflicts_EmbedsConflictsInOfferings()
    {
        // Arrange: Create section with conflicting schedules
        // Act: Send GetClassSectionByIdQuery
        // Assert: Result.Value.Offerings[0].Conflicts.Count > 0
    }

    [Fact(DisplayName = "ConflictDetectionHelper detects all 4 conflict types")]
    public async Task ConflictDetectionHelper_MultipleConflictTypes_DetectsAll()
    {
        // Arrange: Create scenario with teacher, room, section, duplicate conflicts
        // Act: Call ConflictDetectionHelper.DetectConflictsForSectionAsync
        // Assert: Dictionary contains 4 different conflict types
    }

    // Cleanup
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();
}
```

##### B. WebAPI Layer Tests

**File:** `Enrollify.IntegrationTests/_Tests/WebApi/ClassSchedules/AddScheduleEndpointTests.cs`

**Test Collection:** `[Collection("WebApi")]`

**Test Cases:**

```csharp
namespace Enrollify.IntegrationTests._Tests.WebApi.ClassSchedules;

[Collection("WebApi")]
public class AddScheduleEndpointTests : IAsyncLifetime
{
    private readonly WebApiTestFixture _fixture;
    private readonly HttpClient _client;

    public AddScheduleEndpointTests(WebApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact(DisplayName = "POST schedules with conflicts returns 201 with conflicts array")]
    public async Task AddSchedules_WithConflicts_Returns201AndConflicts()
    {
        // Arrange: Create offering via test data builder
        // Act: POST /api/subject-offerings/{id}/schedules
        // Assert: StatusCode = 201, Response.Conflicts.Count > 0
    }

    [Fact(DisplayName = "POST schedules without conflicts returns 201 with empty conflicts")]
    public async Task AddSchedules_NoConflicts_Returns201EmptyConflicts()
    {
        // Arrange: Create offering with no conflicts
        // Act: POST /api/subject-offerings/{id}/schedules
        // Assert: StatusCode = 201, Response.Conflicts.Count = 0
    }

    [Fact(DisplayName = "GET section detail includes conflicts in offerings")]
    public async Task GetSectionDetail_WithConflicts_IncludesConflictsInResponse()
    {
        // Arrange: Create section with conflicting schedules
        // Act: GET /api/class-sections/{id}
        // Assert: StatusCode = 200, Response.Offerings[0].Conflicts.Count > 0
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();
}
```

**Estimated Time:** 4-6 hours

**Run Tests:**
```bash
dotnet test --filter "ConflictDetectionTests"
dotnet test --filter "AddScheduleEndpointTests"
```

---

### Phase 2: Additional Data Integrity Checks (NEXT)

**Priority:** MEDIUM  
**Estimated Time:** 4-6 hours  
**Dependencies:** Phase 1 testing complete

#### DI-01: Offering Without Schedules

**Detection Logic:**

```csharp
// In ScheduleConflictDetector or new validator
public List<ConflictResult> DetectOfferingsWithoutSchedules(
    List<ScheduleConflictDto> schedules, 
    List<int> allOfferingIds)
{
    var offeringsWithSchedules = schedules.Select(s => s.OfferingId).Distinct();
    var offeringsWithoutSchedules = allOfferingIds.Except(offeringsWithSchedules);
    
    return offeringsWithoutSchedules.Select(offeringId => new ConflictResult
    {
        Type = ConflictType.OfferingWithoutSchedule,
        Severity = ConflictSeverity.Error,
        Message = "Subject offering has no class schedules assigned",
        AffectedOfferings = new List<AffectedOffering> { /* ... */ }
    }).ToList();
}
```

**Integration Points:**
- Add to `ConflictDetectionHelper.DetectConflictsForSectionAsync`
- Query all offering IDs for the section
- Check which offerings have zero schedules

**Files to Modify:**
- `Enrollify.Core/Services/ScheduleConflictDetection/ConflictType.cs` — Add `OfferingWithoutSchedule`
- `Enrollify.Application/Features/ClassSchedules/Services/ConflictDetectionHelper.cs`
- `Enrollify.Application/Features/ClassSchedules/Models/ConflictTypeEnum.cs`

---

#### DI-02: Offering Without Teacher

**Detection Logic:**

```csharp
public List<ConflictResult> DetectOfferingsWithoutTeacher(
    List<ScheduleConflictDto> schedules)
{
    return schedules
        .Where(s => s.TeacherId == null)
        .Select(s => s.OfferingId)
        .Distinct()
        .Select(offeringId => new ConflictResult
        {
            Type = ConflictType.OfferingWithoutTeacher,
            Severity = ConflictSeverity.Warning,
            Message = "Subject offering has no teacher assigned",
            AffectedOfferings = new List<AffectedOffering> { /* ... */ }
        }).ToList();
}
```

**Integration:** Same as DI-01

---

#### DI-03: Offering Without Room

**Detection Logic:**

```csharp
public List<ConflictResult> DetectOfferingsWithoutRoom(
    List<ScheduleConflictDto> schedules)
{
    return schedules
        .Where(s => s.RoomId == null)
        .Select(s => s.OfferingId)
        .Distinct()
        .Select(offeringId => new ConflictResult
        {
            Type = ConflictType.OfferingWithoutRoom,
            Severity = ConflictSeverity.Warning,
            Message = "Subject offering has no room assigned",
            AffectedOfferings = new List<AffectedOffering> { /* ... */ }
        }).ToList();
}
```

**Integration:** Same as DI-01

**Tests to Add:**
- Unit tests in `ScheduleConflictDetectorTests.cs`
- Integration tests in `ConflictDetectionTests.cs`

---

#### DI-04: HC-04 Insufficient Break Between Classes

**Status:** Already partially implemented via `MinimumTeacherBreakMinutes`

**Remaining Work:**
- Currently only validates at domain level (entity invariants)
- Add detection logic to `ScheduleConflictDetector` to report breaks < 10 minutes
- Query same-day schedules for same teacher, sort by time, check gaps

**Detection Logic:**

```csharp
public List<ConflictResult> DetectInsufficientTeacherBreaks(
    List<ScheduleConflictDto> schedules, 
    int minBreakMinutes)
{
    var conflicts = new List<ConflictResult>();
    
    var teacherSchedules = schedules
        .Where(s => s.TeacherId.HasValue)
        .GroupBy(s => new { s.TeacherId, s.DayOfWeek })
        .ToList();
    
    foreach (var group in teacherSchedules)
    {
        var sorted = group.OrderBy(s => s.StartTime).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var gapMinutes = (sorted[i + 1].StartTime - sorted[i].EndTime).TotalMinutes;
            if (gapMinutes < minBreakMinutes)
            {
                conflicts.Add(new ConflictResult
                {
                    Type = ConflictType.InsufficientTeacherBreak,
                    Severity = ConflictSeverity.Warning,
                    Message = $"Teacher has only {gapMinutes} minutes between classes (minimum: {minBreakMinutes})",
                    Day = sorted[i].DayOfWeek,
                    StartTime = sorted[i].EndTime,
                    EndTime = sorted[i + 1].StartTime,
                    AffectedOfferings = new List<AffectedOffering> { /* ... */ }
                });
            }
        }
    }
    
    return conflicts;
}
```

**Files to Modify:**
- `Enrollify.Core/Services/ScheduleConflictDetection/ConflictType.cs` — Add `InsufficientTeacherBreak`
- `Enrollify.Core/Services/ScheduleConflictDetection/ScheduleConflictDetector.cs`
- `Enrollify.Application/Features/ClassSchedules/Services/ConflictDetectionHelper.cs`

---

### Phase 3: Soft Conflicts (FUTURE)

**Priority:** LOW  
**Estimated Time:** 8-12 hours  
**Dependencies:** Phase 2 complete

**Reference Document:** `research/scheduling/phase-3-soft-conflicts-implementation.md`

#### SC-01: Teacher Workload Overload

**Detection:** Total teaching hours exceed configured maximum

**Prerequisites:**
- Add `MaxTeacherHoursPerWeek` to `AcademicCoreSettings` (e.g., 18 hours)
- Add Dapper query to sum schedule hours per teacher per term

**SQL Query:**

```sql
SELECT o.TeacherId,
       SUM(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0) AS TotalHours
FROM ClassSchedules cs
JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
JOIN ClassSections sec ON sec.Id = o.ClassSectionId
WHERE sec.AcademicTermId = @termId
  AND o.TeacherId = @teacherId
  AND o.IsActive = 1 AND cs.IsActive = 1
GROUP BY o.TeacherId
HAVING SUM(DATEDIFF(MINUTE, cs.StartTime, cs.EndTime) / 60.0) > @maxLoad
```

**Implementation:**
- Add to `IClassSectionSubjectOfferingRepository`
- Implement in Dapper repository
- Add to `ConflictDetectionHelper`

---

#### SC-02: Room Capacity Exceeded

**Detection:** Expected students exceed room capacity

```csharp
var effectiveStudents = offering.MaxNumberOfStudents ?? section.StudentCapacity;
if (effectiveStudents > room.Capacity)
{
    // Return ROOM_CAPACITY_EXCEEDED warning
}
```

**Integration:**
- Add room capacity check in `ConflictDetectionHelper`
- Query room details in `GetRelatedSchedulesForConflictDetectionAsync`

---

#### SC-03: Teacher No Break

**Status:** Same as DI-04 above (already partially designed)

---

#### SC-04: Adviser as Teacher

**Detection:** Section adviser teaches in their own section

```sql
SELECT s.AdviserId, o.TeacherId
FROM ClassSectionSubjectOffering o
JOIN ClassSections s ON s.Id = o.ClassSectionId
WHERE o.TeacherId = s.AdviserId AND o.IsActive = 1
```

**Severity:** Info (may be institutionally allowed)

---

#### SC-05: Year Level Mismatch

**Detection:** Subject offered to wrong year level per curriculum

**Requires:** Join to `CurriculumSubjects` to get expected year level

---

#### SC-06: Schedule Outside Core Hours

**Detection:** Class starts before `EarliestClassStartTime` or ends after `LatestClassEndTime`

**Implementation:**
- Add check in `ScheduleConflictDetector`
- Compare `schedule.StartTime` vs `AcademicCoreSettings.EarliestClassStartTime`
- Compare `schedule.EndTime` vs `AcademicCoreSettings.LatestClassEndTime`

---

### Phase 4: Admin Conflict Dashboard (FUTURE)

**Priority:** LOW  
**Estimated Time:** 12-16 hours

**Reference:** `research/scheduling/phase-3-soft-conflicts-implementation.md` (lines 200-277)

#### Backend API

**New Endpoint:** `GET /api/scheduling/conflicts?termId={id}`

**Response:**

```json
{
  "term": { "id": 1, "name": "1st Term SY 2025-2026" },
  "summary": {
    "totalConflicts": 42,
    "errorCount": 15,
    "warningCount": 27,
    "byType": {
      "TEACHER_DOUBLE_BOOKED": 8,
      "ROOM_DOUBLE_BOOKED": 3,
      "SECTION_OVERLAP": 12,
      "DUPLICATE_SUBJECT": 4,
      "INSUFFICIENT_BREAK": 10,
      "WORKLOAD_OVERLOAD": 5
    }
  },
  "conflicts": [
    {
      "type": "TEACHER_DOUBLE_BOOKED",
      "severity": "error",
      "message": "Prof. Juan dela Cruz is double-booked",
      "day": "Monday",
      "startTime": "08:00",
      "endTime": "09:00",
      "affectedOfferings": [
        {
          "id": 1,
          "subject": { "code": "CS101", "title": "Intro to CS" },
          "section": { "id": 5, "name": "BSCS 1-A" },
          "room": { "roomNumber": "RM-301", "building": "Main" }
        },
        {
          "id": 2,
          "subject": { "code": "CS102", "title": "Programming I" },
          "section": { "id": 6, "name": "BSCS 1-B" },
          "room": { "roomNumber": "RM-302", "building": "Main" }
        }
      ]
    }
  ]
}
```

**Implementation:**
- Create `Enrollify.Application/Features/Scheduling/Queries/GetConflictsByTermQuery.cs`
- Create `Enrollify.WebAPI/Features/Scheduling/GetConflictsByTermEndpoint.cs`
- Add authorization: `PolicyName.HasViewSchedulingConflictsPermission`

#### Frontend Dashboard

**Route:** `/class-sections/conflicts?termId={id}`

**Components:**
- Conflict summary cards (total, errors, warnings, by type)
- Filterable/sortable conflict table
- Export to CSV/Excel
- Drill-down to specific section detail page

**Features:**
- Filter by conflict type, severity, day, teacher, room
- Sort by section, subject, time
- Quick actions: "Fix in Section", "Ignore", "Mark as Resolved"

---

### Phase 5: Performance Optimization (CONDITIONAL)

**Trigger:** If conflict detection queries exceed 50ms on section detail page load

**Recommended Indexes:**

**Reference:** `research/scheduling/database-index-recommendations.md`

```sql
-- Index 1: Teacher schedule lookups
CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_TeacherId_AcademicTermId_Active
ON ClassSectionSubjectOffering (TeacherId, AcademicTermId)
INCLUDE (Id, ClassSectionId, SubjectId, RoomId)
WHERE IsActive = 1 AND TeacherId IS NOT NULL;

-- Index 2: Room schedule lookups
CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_RoomId_AcademicTermId_Active
ON ClassSectionSubjectOffering (RoomId, AcademicTermId)
INCLUDE (Id, ClassSectionId, SubjectId, TeacherId)
WHERE IsActive = 1 AND RoomId IS NOT NULL;

-- Index 3: Schedule time range queries
CREATE NONCLUSTERED INDEX IX_ClassSchedules_DayOfWeek_Time_Active
ON ClassSchedules (DayOfWeek, StartTime, EndTime)
INCLUDE (Id, ClassSectionSubjectOfferingId)
WHERE IsActive = 1;
```

**Implementation:**
1. Measure baseline query performance (use SQL Profiler or EF Core logging)
2. Add indexes via EF Core migration
3. Re-measure performance
4. Document improvement in `database-index-recommendations.md`

---

## 📊 Estimated Timeline

| Phase | Task | Priority | Time | Status |
|-------|------|----------|------|--------|
| 1A | Manual API Testing | HIGH | 2h | ⏳ Pending |
| 1A | Unit Tests (12 tests) | HIGH | 3-4h | ⏳ Pending |
| 1A | Integration Tests (6 tests) | HIGH | 4-6h | ⏳ Pending |
| 2 | DI-01, DI-02, DI-03 | MEDIUM | 4-6h | ⏳ Pending |
| 2 | DI-04 (Insufficient Break) | MEDIUM | 2-3h | ⏳ Pending |
| 3 | SC-01 through SC-06 | LOW | 8-12h | 📅 Deferred |
| 4 | Admin Dashboard | LOW | 12-16h | 📅 Deferred |
| 5 | Performance Indexes | CONDITIONAL | 2-3h | ⏸️ On Hold |

**Total Immediate Work (Phase 1A + 2):** 15-21 hours  
**Total Future Work (Phase 3 + 4):** 20-28 hours  
**Grand Total:** 35-49 hours

---

## 🎯 Definition of Done

### Phase 1A (Testing)

- ✅ All 4 conflict types verified via Postman/Swagger
- ✅ 12+ unit tests passing with >90% coverage of `ScheduleConflictDetector`
- ✅ 6+ integration tests passing (Application + WebAPI layers)
- ✅ No errors in backend logs during test runs
- ✅ Frontend can deserialize conflicts[] without type errors

### Phase 2 (Data Integrity)

- ✅ DI-01, DI-02, DI-03, DI-04 implemented and tested
- ✅ Unit tests for each DI check
- ✅ Integration tests verify DI checks in full stack
- ✅ All DI checks return appropriate severity (Error/Warning)

### Phase 3 (Soft Conflicts)

- ✅ SC-01 through SC-06 implemented
- ✅ Configuration settings added to `AcademicCoreSettings`
- ✅ Dapper queries optimized for soft conflict detection
- ✅ Unit + integration tests for all SC types

### Phase 4 (Dashboard)

- ✅ Admin dashboard UI shows all conflicts for a term
- ✅ Filters and sorting work correctly
- ✅ Export to CSV/Excel functional
- ✅ Authorization enforced (`HasViewSchedulingConflictsPermission`)

---

## 📚 Reference Documents

1. **Conflict Taxonomy:** `research/scheduling/class-scheduling-conflicts.md`
2. **Phase 1 Summary:** `research/scheduling/IMPLEMENTATION-SUMMARY.md`
3. **Phase 3 Design:** `research/scheduling/phase-3-soft-conflicts-implementation.md`
4. **Dapper Migration:** `research/scheduling/DAPPER-MIGRATION-SUMMARY.md`
5. **Index Recommendations:** `research/scheduling/database-index-recommendations.md`
6. **Integration Test Guide:** `src/system/EnrollifyBackend/Enrollify.IntegrationTests/README.md`

---

## 🚀 Quick Start (Next Steps)

1. **Test Now:**
   ```bash
   # Start backend
   cd src/system/EnrollifyBackend
   dotnet run --project Enrollify.WebAPI
   
   # Open Swagger
   https://localhost:7107/swagger
   
   # Test conflict detection manually (see Phase 1A.1)
   ```

2. **Write Unit Tests:**
   ```bash
   # Create test file
   touch src/system/EnrollifyBackend/Enrollify.UnitTests/Core/Services/ScheduleConflictDetectorTests.cs
   
   # Run tests
   dotnet test --filter "ScheduleConflictDetectorTests"
   ```

3. **Write Integration Tests:**
   ```bash
   # Create test files
   mkdir -p src/system/EnrollifyBackend/Enrollify.IntegrationTests/_Tests/Application/ClassSchedules
   touch src/system/EnrollifyBackend/Enrollify.IntegrationTests/_Tests/Application/ClassSchedules/ConflictDetectionTests.cs
   
   # Run tests
   dotnet test --filter "ConflictDetectionTests"
   ```

4. **Implement Phase 2 (DI Checks):**
   - Add `ConflictType` enum values
   - Implement detection methods in `ScheduleConflictDetector`
   - Add to `ConflictDetectionHelper`
   - Write tests

---

## 🔧 Troubleshooting

### Issue: Integration tests fail with "Database not found"

**Solution:** Ensure `TestContainersManager` is running. Check `Enrollify.IntegrationTests/README.md` for setup.

### Issue: Unit tests can't resolve `ScheduleConflictDetector` dependencies

**Solution:** Tests should instantiate `ScheduleConflictDetector` directly (it has no dependencies). No DI container needed.

### Issue: Frontend conflicts[] field is null

**Solution:** 
1. Verify backend returns conflicts in response (check network tab)
2. Ensure `OfferingSchema` includes `conflicts: z.array(ConflictSchema).default([])`
3. Regenerate API types: `npm run generate:api:win`

---

## ✍️ Notes

- **Cross-term conflicts:** Explicitly ignored per design decision (assumes terms never overlap)
- **Performance threshold:** 50ms for section detail query is acceptable (infrequent admin operation)
- **SNAPSHOT isolation:** Already enabled, no additional locking needed
- **Soft warnings:** Conflicts never block saves; admins decide when to fix before opening enrollment

---

**Last Updated:** 2026-06-06  
**Maintained By:** Development Team  
**Review Cycle:** Update after completing each phase
