# Curriculum Lifecycle: PhaseOut & Archive Commands

## Overview
The `CurriculumStatusEnum` defines `PhaseOut (3)` and `Archived (4)` states, but there are **no commands, handlers, or WebAPI endpoints** to transition curriculums into these states. The curriculum lifecycle is stuck at `Active` and can never retire.

## Status
❌ **NOT IMPLEMENTED**

## Current State

### Defined but Unreachable
- **File:** `Enrollify.Core/Constants/CurriculumStatusEnum.cs` (lines 1-13)
- **Enum Values:**
  - `Draft (1)` ✅ Reachable via `CreateDraftCurriculum`
  - `Active (2)` ✅ Reachable via `ApproveCurriculum`
  - `PhaseOut (3)` ❌ Unreachable - no command exists
  - `Archived (4)` ❌ Unreachable - no command exists

### Evidence of Missing Implementation
1. **No Commands Exist:**
   - `PhaseOutCurriculum.cs` - does not exist
   - `ArchiveCurriculum.cs` - does not exist

2. **No WebAPI Endpoints:**
   - `/api/curriculums/{id}/phase-out` - not implemented
   - `/api/curriculums/{id}/archive` - not implemented

3. **No Event Handlers:**
   - `CurriculumPhasedOutEvent` - not defined
   - `CurriculumArchivedEvent` - not defined

4. **Test Files:**
   - `UpdateCurriculumTests.cs` manually sets status via reflection (not via command)
   - Tests for "archived curriculum" don't use a proper command

## Business Requirements for Phase-Out and Archive

### PhaseOut State

**Purpose:** Mark a curriculum as no longer used for new cohorts, but existing students continue using it

**Scenario:** When a new curriculum is approved and replaces an older one

**Example:**
1. BSCS 2020 is Active, used by cohorts 2020-2024
2. BSCS 2024 is approved and becomes Active
3. BSCS 2020 transitions to PhaseOut
4. **Result:**
   - New cohorts starting 2025+ use BSCS 2024
   - Existing cohorts (2020-2024) continue with BSCS 2020
   - No class sections created using BSCS 2020 as curriculum

**Guard Rules for PhaseOut:**
- Can only transition from `Active` to `PhaseOut`
- Existing class sections using this curriculum can continue
- **Cannot create new class sections** using a PhaseOut curriculum
- Subsequent `ApproveCurriculum` for the same course updates `CourseCurriculumAssignment` while PhaseOut prevents new usage

### Archive State

**Purpose:** Permanently retire a curriculum after all students using it have completed their programs

**Scenario:** All cohorts using an old curriculum have graduated

**Example:**
1. BSCS 2020 is in PhaseOut
2. Last cohort using BSCS 2020 completes the program
3. BSCS 2020 transitions to Archived
4. **Result:**
   - Completely historical record
   - No new usage possible
   - Retained for audit/transcript generation

**Guard Rules for Archive:**
- Can only transition from `PhaseOut` to `Archived`
- All class sections using this curriculum must be `Completed` or `Cancelled`
- Read-only for all practical purposes

## Proposed Implementation

### Command 1: PhaseOutCurriculum

**File:** `Enrollify.Application/Features/Curriculums/Commands/PhaseOutCurriculum.cs`

```csharp
public class PhaseOutCurriculum : ICommand<CurriculumDto>
{
    public int Id { get; init; }
    public string Reason { get; init; }  // Why it's being phased out (audit trail)
}

public sealed class Handler : ICommandHandler<PhaseOutCurriculum, CurriculumDto>
{
    private readonly IRepository<Curriculum> _curriculumRepository;
    private readonly IRepository<ClassSection> _classSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<Result<CurriculumDto>> Handle(
        PhaseOutCurriculum command, CancellationToken cancellationToken)
    {
        // Get the curriculum
        var curriculum = await _curriculumRepository.GetByIdAsync(command.Id, cancellationToken);
        if (curriculum == null)
            return Result.NotFound("Curriculum not found.");

        // Guard: Must be Active to phase out
        if (curriculum.StatusId != CurriculumStatusEnum.Active)
            return Result.Invalid("Only Active curriculums can be phased out.");

        // Guard: Check for Locked sections (shouldn't exist, but be safe)
        var lockedSections = await _classSubscriptionRepository
            .ListAsync(
                new GetClassSectionsByCurriculumAndStatusSpec(
                    curriculum.Id,
                    statusThreshold: ClassSectionStatusEnum.Locked,
                    statusCeiling: ClassSectionStatusEnum.Locked),
                cancellationToken);

        if (lockedSections.Any())
            return Result.Invalid(
                $"Cannot phase out curriculum while {lockedSections.Count} section(s) " +
                $"are in Locked status. Complete or cancel those sections first.");

        // Transition to PhaseOut
        curriculum.PhaseOut(command.Reason);  // Domain method
        
        _curriculumRepository.Update(curriculum);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Raise domain event (if needed for side effects)
        // e.g., notify stakeholders that curriculum is retiring

        return Result.Updated(_mapper.Map<CurriculumDto>(curriculum));
    }
}
```

**Domain Method in Curriculum.cs:**
```csharp
public Curriculum PhaseOut(string reason)
{
    Guard.Against.InvalidInput(StatusId, nameof(StatusId),
        s => s == CurriculumStatusEnum.Active,
        "Only Active curriculums can be phased out.");
    
    StatusId = CurriculumStatusEnum.PhaseOut;
    PhaseOutDate = DateTime.UtcNow;
    PhaseOutReason = reason;
    
    AddDomainEvent(new CurriculumPhasedOutEvent(Id, reason));
    return this;
}
```

### Command 2: ArchiveCurriculum

**File:** `Enrollify.Application/Features/Curriculums/Commands/ArchiveCurriculum.cs`

```csharp
public class ArchiveCurriculum : ICommand<CurriculumDto>
{
    public int Id { get; init; }
}

public sealed class Handler : ICommandHandler<ArchiveCurriculum, CurriculumDto>
{
    private readonly IRepository<Curriculum> _curriculumRepository;
    private readonly IRepository<ClassSection> _classSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<Result<CurriculumDto>> Handle(
        ArchiveCurriculum command, CancellationToken cancellationToken)
    {
        // Get the curriculum
        var curriculum = await _curriculumRepository.GetByIdAsync(command.Id, cancellationToken);
        if (curriculum == null)
            return Result.NotFound("Curriculum not found.");

        // Guard: Must be PhaseOut to archive
        if (curriculum.StatusId != CurriculumStatusEnum.PhaseOut)
            return Result.Invalid("Only PhaseOut curriculums can be archived.");

        // Guard: No incomplete sections
        var incompleteSections = await _classSubscriptionRepository
            .ListAsync(
                new GetClassSectionsByCurriculumAndStatusSpec(
                    curriculum.Id,
                    statusThreshold: ClassSectionStatusEnum.Draft,
                    statusCeiling: ClassSectionStatusEnum.Active),  // Exclude Completed/Cancelled
                cancellationToken);

        if (incompleteSections.Any())
            return Result.Invalid(
                $"Cannot archive curriculum while {incompleteSections.Count} section(s) " +
                $"are not yet Completed or Cancelled.");

        // Transition to Archived
        curriculum.Archive();  // Domain method
        
        _curriculumRepository.Update(curriculum);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated(_mapper.Map<CurriculumDto>(curriculum));
    }
}
```

**Domain Method in Curriculum.cs:**
```csharp
public Curriculum Archive()
{
    Guard.Against.InvalidInput(StatusId, nameof(StatusId),
        s => s == CurriculumStatusEnum.PhaseOut,
        "Only PhaseOut curriculums can be archived.");
    
    StatusId = CurriculumStatusEnum.Archived;
    ArchivedDate = DateTime.UtcNow;
    
    AddDomainEvent(new CurriculumArchivedEvent(Id));
    return this;
}
```

## Guards & Constraints

### PhaseOutCurriculum Guards

| Check | Condition | Result |
|-------|-----------|--------|
| Status is Active | StatusId ≠ Active | ❌ Invalid |
| No Locked sections | Locked sections exist | ⚠️ Invalid (warning) |

### ArchiveCurriculum Guards

| Check | Condition | Result |
|-------|-----------|--------|
| Status is PhaseOut | StatusId ≠ PhaseOut | ❌ Invalid |
| All sections Completed/Cancelled | Incomplete sections exist | ❌ Invalid |

## WebAPI Endpoints

### PhaseOut Endpoint
```csharp
public class PhaseOutCurriculumEndpoint : Endpoint<PhaseOutCurriculumRequest, CurriculumDto>
{
    public override void Configure()
    {
        Post("/api/curriculums/{id}/phase-out");
        Roles("Admin");  // Only admins can retire curriculums
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var command = new PhaseOutCurriculum
        {
            Id = id,
            Reason = req.Reason
        };
        
        var result = await Mediator.Send(command, ct);
        await SendResultAsync(result.ToHttpResponse());
    }
}
```

### Archive Endpoint
```csharp
public class ArchiveCurriculumEndpoint : Endpoint<ArchiveCurriculumRequest, CurriculumDto>
{
    public override void Configure()
    {
        Post("/api/curriculums/{id}/archive");
        Roles("Admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var command = new ArchiveCurriculum { Id = id };
        
        var result = await Mediator.Send(command, ct);
        await SendResultAsync(result.ToHttpResponse());
    }
}
```

## Events

### CurriculumPhasedOutEvent
```csharp
public class CurriculumPhasedOutEvent : DomainEvent
{
    public int CurriculumId { get; }
    public string Reason { get; }
    public DateTime PhasedOutDate { get; }

    public CurriculumPhasedOutEvent(int curriculumId, string reason)
    {
        CurriculumId = curriculumId;
        Reason = reason;
        PhasedOutDate = DateTime.UtcNow;
    }
}
```

### CurriculumArchivedEvent
```csharp
public class CurriculumArchivedEvent : DomainEvent
{
    public int CurriculumId { get; }
    public DateTime ArchivedDate { get; }

    public CurriculumArchivedEvent(int curriculumId)
    {
        CurriculumId = curriculumId;
        ArchivedDate = DateTime.UtcNow;
    }
}
```

## Curriculum Entity Changes

**File:** `Enrollify.Core/Aggregates/CurriculumAggregate/Curriculum.cs`

Add properties:
```csharp
public DateTime? PhaseOutDate { get; private set; }
public string? PhaseOutReason { get; private set; }
public DateTime? ArchivedDate { get; private set; }
```

Add methods:
```csharp
public Curriculum PhaseOut(string reason) { ... }
public Curriculum Archive() { ... }
```

## Specification Needed

**Specification:** `GetClassSectionsByCurriculumAndStatusSpec`
- **Input:** `curriculumId: int`, `statusThreshold: ClassSectionStatusEnum`, `statusCeiling: ClassSectionStatusEnum`
- **Output:** Sections using the curriculum with status within the range

```csharp
public class GetClassSectionsByCurriculumAndStatusSpec : Specification<ClassSection>
{
    public GetClassSectionsByCurriculumAndStatusSpec(
        int curriculumId,
        ClassSectionStatusEnum statusThreshold,
        ClassSectionStatusEnum statusCeiling)
    {
        Query
            .Where(s => s.CurriculumId == curriculumId 
                && s.StatusId >= statusThreshold 
                && s.StatusId <= statusCeiling)
            .OrderBy(s => s.CreatedDate);
    }
}
```

## Implementation Steps

1. **Add domain methods** to `Curriculum.cs`: `PhaseOut()`, `Archive()`
2. **Add domain events:** `CurriculumPhasedOutEvent`, `CurriculumArchivedEvent`
3. **Create specifications:** `GetClassSectionsByCurriculumAndStatusSpec`
4. **Create commands:** `PhaseOutCurriculum.cs`, `ArchiveCurriculum.cs`
5. **Create WebAPI endpoints:** `PhaseOutCurriculumEndpoint.cs`, `ArchiveCurriculumEndpoint.cs`
6. **Add unit tests:** Test state transitions and guard conditions
7. **Add integration tests:** Test with real database sections
8. **Update database schema** (if needed) for audit fields like `PhaseOutDate`, `PhaseOutReason`, `ArchivedDate`

## Testing Approach

### Unit Test: PhaseOut Transition
```csharp
[Fact(DisplayName = "PhaseOut transitions Active curriculum to PhaseOut")]
public async Task PhaseOut_ActiveCurriculum_TransitionsSuccessfully()
{
    var curriculum = new Curriculum(...) { StatusId = CurriculumStatusEnum.Active };
    
    curriculum.PhaseOut("Replaced by BSCS 2025");
    
    Assert.Equal(CurriculumStatusEnum.PhaseOut, curriculum.StatusId);
    Assert.Equal("Replaced by BSCS 2025", curriculum.PhaseOutReason);
}
```

### Integration Test: Archive with Completed Sections
```csharp
[Fact(DisplayName = "Archive allowed when all sections are Completed")]
public async Task Archive_AllSectionsCompleted_Succeeds()
{
    // Create curriculum in PhaseOut
    // Create sections in Completed status
    // Call Archive command
    // Verify curriculum is now Archived
}
```

## Real-World Timeline

### Year 1
- BSCS 2020 is Active, used for cohorts 2020-2023

### Year 2
- BSCS 2024 is approved and becomes Active
- BSCS 2020 transitions to PhaseOut
- New cohorts use BSCS 2024
- Existing cohorts (2020-2023) continue with BSCS 2020

### Year 5
- Last cohort using BSCS 2020 (cohort 2020) completes
- All BSCS 2020 sections are now Completed
- BSCS 2020 transitions to Archived
- Historical record preserved for transcripts

## Related Features

- **Curriculum Approval:** Currently reaches Active (see `done/01-curriculum-status-enum.md`)
- **CourseCurriculumAssignment:** Binding prevents PhaseOut curriculum from being used by new cohorts
- **ClassSection Transitions:** Sections continue to completion using their referenced curriculum

## Notes

- ❌ **NOT IMPLEMENTED** - Must be added to complete curriculum lifecycle
- Provides proper curriculum retirement capability
- Prevents accidental reuse of old curriculum versions
- Audit trail preserved via events and date fields
- Implementation follows established DDD/CQRS patterns in codebase
