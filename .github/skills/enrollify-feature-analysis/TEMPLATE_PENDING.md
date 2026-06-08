# [Feature Name] — TEMPLATE (PENDING/PARTIAL)

Use this template for creating feature files in the `pending/` folder for features that are not implemented or only partially implemented.

---

# Feature Name — STATUS

## Overview
[1-2 sentence description of what this feature should do and why it's needed]

## Status
⚠️ **PARTIALLY IMPLEMENTED** (or ❌ **NOT IMPLEMENTED**)

[Brief explanation of what's missing or incomplete]

## Current State

### What Exists ✅

[Describe what parts are already implemented]

- File: `Path/To/Existing/File.cs` (lines X-Y)
- What works: [description]
- Tests: [if any exist]

### What's Missing ❌

[List the gaps that need to be filled]

- [ ] Missing handler command
- [ ] No WebAPI endpoint
- [ ] Guards not implemented
- [ ] Tests not written
- [ ] Domain methods not added

### Impact of Missing Implementation

[Why does this gap matter? What breaks without it?]

- **Problem:** [What goes wrong if this isn't fixed]
- **Scope:** [How many users/features are affected]
- **Workaround:** [Is there a temporary workaround? If any]

---

## Missing Specification / Design

### What Needs to Be Built

[Detailed breakdown of what code needs to be written]

#### 1. Domain Method (if needed)

```csharp
// File: Enrollify.Core/Aggregates/[Entity]/[Entity].cs
// Add this method to the entity

public [Entity] [MethodName]([Type] param1, [Type] param2)
{
    Guard.Against.InvalidInput([Property], nameof([Property]),
        condition => [condition],
        "[error message]");
    
    // Implementation
    [Property] = param1;
    AddDomainEvent(new [EntityChangedEvent](...));
    return this;
}
```

#### 2. Command (if needed)

```csharp
// File: Enrollify.Application/Features/[Feature]/Commands/[CommandName].cs
// Create this new command file

public class [CommandName] : ICommand<[DTOName]>
{
    public [Type] Property1 { get; init; }
    public [Type] Property2 { get; init; }
}

public sealed class Handler : ICommandHandler<[CommandName], [DTOName]>
{
    private readonly IRepository<[Entity]> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<Result<[DTOName]>> Handle(
        [CommandName] command, CancellationToken cancellationToken)
    {
        // Load entity
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            return Result.NotFound();

        // Call domain method
        entity.[MethodName](command.Property1);

        // Persist
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated(_mapper.Map<[DTOName]>(entity));
    }
}
```

#### 3. Command Validator (if needed)

```csharp
// File: Enrollify.Application/Features/[Feature]/Commands/[CommandName]Validator.cs

public class [CommandName]Validator : AbstractValidator<[CommandName]>
{
    public [CommandName]Validator()
    {
        RuleFor(x => x.Property1)
            .NotEmpty()
            .WithMessage("Property1 is required");
    }
}
```

#### 4. WebAPI Endpoint (if needed)

```csharp
// File: Enrollify.WebAPI/Features/[Feature]/[CommandName]Endpoint.cs

public class [CommandName]Endpoint : Endpoint<[CommandName]Request, [DTOName]>
{
    public override void Configure()
    {
        Post("/api/[route]");
        Roles("Admin"); // Adjust as needed
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new [CommandName]
        {
            Property1 = Request.Property1,
            Property2 = Request.Property2
        };

        var result = await Mediator.Send(command, ct);
        await SendResultAsync(result.ToHttpResponse());
    }
}

public class [CommandName]Request
{
    public [Type] Property1 { get; init; }
    public [Type] Property2 { get; init; }
}
```

#### 5. Specification (if needed)

```csharp
// File: Enrollify.Application/Specifications/[Entity]/Get[Entity]By[Criteria]Spec.cs

public class Get[Entity]By[Criteria]Spec : Specification<[Entity]>
{
    public Get[Entity]By[Criteria]Spec([Type] criteria)
    {
        Query
            .Where(x => x.Property == criteria)
            .OrderBy(x => x.CreatedDate);
    }
}
```

#### 6. Domain Event (if needed)

```csharp
// File: Enrollify.Core/Events/[Entity][Action]Event.cs

public class [Entity][Action]Event : DomainEvent
{
    public int [Entity]Id { get; }
    public string Property { get; }

    public [Entity][Action]Event(int entityId, string property)
    {
        [Entity]Id = entityId;
        Property = property;
    }
}
```

---

## Step-by-Step Implementation Plan

### Phase 1: Domain Layer (30 min)

1. [ ] Open `Enrollify.Core/Aggregates/[Entity]/[Entity].cs`
2. [ ] Add domain method `[MethodName]()`
3. [ ] Add guard using `Guard.Against.InvalidInput()`
4. [ ] Create domain event class in `Enrollify.Core/Events/`
5. [ ] Raise event in domain method: `AddDomainEvent(new [Event](...))`
6. [ ] Test domain method in isolation

### Phase 2: Application Layer (1 hour)

1. [ ] Create command class: `Enrollify.Application/Features/[Feature]/Commands/[Command].cs`
2. [ ] Create command handler with domain logic
3. [ ] Create validator: `[Command]Validator.cs`
4. [ ] Add specifications if needed: `Get[Entity]By[Criteria]Spec.cs`
5. [ ] Add unit tests: `Enrollify.UnitTests/Features/[Feature]/Commands/[Command]Tests.cs`
6. [ ] Test command handlers with mocked repository

### Phase 3: WebAPI Layer (30 min)

1. [ ] Create endpoint: `Enrollify.WebAPI/Features/[Feature]/[Command]Endpoint.cs`
2. [ ] Add request/response DTOs
3. [ ] Map to command in handler
4. [ ] Add authorization attribute if needed
5. [ ] Test endpoint response codes

### Phase 4: Integration Tests (45 min)

1. [ ] Create integration test: `Enrollify.IntegrationTests/_Tests/WebApi/[Feature]Tests.cs`
2. [ ] Test happy path (success scenario)
3. [ ] Test guard violation (failure scenario)
4. [ ] Test edge cases
5. [ ] Verify database state changes

### Phase 5: Documentation & Polish (15 min)

1. [ ] Update project README if needed
2. [ ] Add XML doc comments to public methods
3. [ ] Verify all tests pass
4. [ ] Code review checklist

**Total Effort:** [2h / 4h / 6h / etc.]

---

## Guard Rules (When Implemented)

[What guards/validations should this feature have?]

| Guard | Condition | Result |
|-------|-----------|--------|
| Guard 1 | When this condition | ❌ Result Type |
| Guard 2 | When that condition | ❌ Result Type |

**Rationale:** [Why these guards are necessary]

---

## Real-World Scenarios (Future)

### Scenario 1: Normal Usage

**Current (without feature):** [How it works now or workaround]

**Future (with feature):** [How it will work after implementation]

---

### Scenario 2: Prevented by Guard

**What could go wrong:** [Dangerous operation]

**How guard prevents it:** [What check stops it]

---

## Testing Strategy

### Unit Tests

**Location:** `Enrollify.UnitTests/Features/[Feature]/Commands/[Command]Tests.cs`

**Test cases:**
```csharp
[Fact(DisplayName = "Description of what's tested")]
public async Task Test_Scenario_ExpectedResult()
{
    // Arrange: Setup
    
    // Act: Execute
    
    // Assert: Verify
}
```

**What to test:**
- [ ] Guard blocks when condition met
- [ ] Guard allows when condition not met
- [ ] Domain event is raised
- [ ] Database is updated correctly
- [ ] Error message is clear

### Integration Tests

**Location:** `Enrollify.IntegrationTests/_Tests/WebApi/[Feature]Tests.cs`

**Approach:**
- Create test data using `TestDataBuilder`
- Execute command via `WebApplicationFactory` or Mediator
- Verify HTTP status code
- Verify database state
- Verify error response format

**Key test methods:**
```csharp
[Fact(DisplayName = "Happy path succeeds")]
public async Task Operation_Valid_Returns200Ok() { ... }

[Fact(DisplayName = "Guard blocks operation")]
public async Task Operation_GuardCondition_Returns403Forbidden() { ... }

[Fact(DisplayName = "Invalid input fails")]
public async Task Operation_InvalidInput_Returns400BadRequest() { ... }
```

---

## Priority & Effort

| Aspect | Assessment |
|--------|------------|
| **Priority** | Medium (High/Medium/Low) |
| **Effort** | 4 hours |
| **Dependencies** | [Feature X, Feature Y] |
| **Blocking** | [What features are blocked by this] |
| **Complexity** | Medium (Low/Medium/High) |

### Effort Breakdown

- Domain method: 30 min
- Command + validator: 1 hour
- WebAPI endpoint: 30 min
- Specifications: 30 min (if needed)
- Tests (unit + integration): 1 hour
- Documentation + polish: 15 min

---

## Dependencies

### Must Have Before Implementation

- [ ] Feature X (see `done/XX-feature-x.md`)
- [ ] Database migration for new table/column
- [ ] Permission/authorization policy defined

### Optional (Nice to Have)

- [ ] Event handler for side effects
- [ ] Cache invalidation strategy
- [ ] Performance optimization

---

## Related Features

- **[Feature A](../done/01-feature-a.md)** — This feature depends on A
- **[Feature B](../pending/02-feature-b.md)** — B depends on this feature
- **[Concept C](../README.md#concept-c)** — Uses this pattern

---

## Success Criteria

After implementation, verify:

- [ ] Domain method exists and enforces guards
- [ ] Command handler successfully executes domain method
- [ ] WebAPI endpoint responds with correct status codes
- [ ] Guards are tested (unit + integration)
- [ ] Error messages are user-friendly and actionable
- [ ] Code follows project patterns (DDD, CQRS, etc.)
- [ ] All tests pass
- [ ] Performance is acceptable

---

## Potential Issues & Mitigations

### Issue 1: [Potential Problem]
- **Why it might happen:** [Scenario]
- **How to prevent:** [Mitigation]

### Issue 2: [Potential Problem]
- **Why it might happen:** [Scenario]
- **How to prevent:** [Mitigation]

---

## Future Enhancements

[What could be added after basic implementation?]

- [ ] [Enhancement 1]
- [ ] [Enhancement 2]

---

## Notes

- [Important implementation detail]
- [Gotcha to watch out for]
- [Design decision rationale]

---

## References

- **Research document section:** [Link to research doc section if applicable]
- **Related tests:** `[Path to similar test file]`
- **Similar feature:** `[Link to similar implemented feature]`

---

## Summary

[1 paragraph recap of what this feature will do when implemented, why it's important, and any key considerations]

**Key Takeaway:** [One sentence that captures the essence of implementing this feature]

---

**Feature Status:** ⚠️ Partially Implemented / ❌ Not Implemented  
**Estimated Completion:** [Date/sprint if known]  
**Owner/Assignee:** [If known]  
**Last Updated:** [Date]  
**Questions?** See README.md or SKILL.md for more context
