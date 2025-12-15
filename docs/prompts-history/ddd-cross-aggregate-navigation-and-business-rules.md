# DDD: Cross-Aggregate Navigation and Business Rules

**Date:** Conversation Export  
**Topic:** DDD and Clean Architecture - Cross-Aggregate References and Business Rules  
**Context:** Enrollify Backend - RoomType and Room Aggregates

---

## Question 1: Cross-Aggregate Navigation Properties

### User Question

> I'm trying to implement DDD and Clean Architecture, I have the RoomType and Room aggregates. The room already has a RoomType navigation property, now I want to be able to retrieve rooms via RoomType aggregate.
>
> Will that be okay for DDD and Clean Architecture?
>
> If so, how can I configure RoomType entity/aggregate to be able to retrieve rooms per room type? Is it via RoomTypeConfiguration?

### Response

This is a great question about DDD and Clean Architecture!

#### DDD Perspective on Cross-Aggregate Navigation

In strict DDD, **aggregates should reference each other only by ID, not by navigation properties**. This is because:

1. **Aggregate boundaries** define consistency boundaries - each aggregate should be independently loadable and savable
2. **Cross-aggregate navigation properties** can lead to tight coupling and make it unclear which aggregate "owns" the relationship
3. **Performance concerns** - lazy loading across aggregates can cause unexpected database hits

#### Your Current Design Analysis

Your current design is actually **correct from a DDD perspective**:
- `Room` has `RoomTypeId` (ID reference) ?
- `Room` has `RoomType?` navigation property (acceptable for read scenarios) ?
- `RoomType` does NOT have a `Rooms` collection ?

#### Recommended Approach

Instead of adding a `Rooms` collection to `RoomType`, you should **query rooms by RoomTypeId through a repository or query service**. This keeps your aggregates properly bounded.

Your existing implementation is correct:

```csharp
// IRoomRepository.cs
public interface IRoomRepository
{
    Task<List<Room>> GetAllByRoomType(RoomTypeId roomTypeId, CancellationToken cancellationToken);
}

// RoomRepository.cs
public async Task<List<Room>> GetAllByRoomType(RoomTypeId roomTypeId, CancellationToken cancellationToken)
{
    var rooms = await _dbContext.Rooms
        .Where(r => r.RoomTypeId == roomTypeId)
        .ToListAsync(cancellationToken);
    return rooms;
}
```

#### Why This Is The Right Approach

| Approach | DDD Compliant? | Reason |
|----------|---------------|--------|
| Query via Repository (`IRoomRepository.GetAllByRoomType`) | ? Yes | Keeps aggregates independent, explicit queries |
| Navigation collection on `RoomType` (`List<Room> Rooms`) | ? No | Violates aggregate boundaries, creates tight coupling |
| EF Core configuration for relationship | ?? Partial | Only needed for FK constraints, not for navigation |

#### Your RoomTypeConfiguration Is Correct

You do **NOT** need to add any relationship configuration to `RoomTypeConfiguration.cs` for retrieving rooms. The FK relationship is already defined from the `Room` side.

#### How To Use It

```csharp
// In your Application layer (e.g., a query handler)
public class GetRoomsByRoomTypeHandler
{
    private readonly IRoomRepository _roomRepository;
    
    public async Task<List<Room>> Handle(RoomTypeId roomTypeId, CancellationToken ct)
    {
        return await _roomRepository.GetAllByRoomType(roomTypeId, ct);
    }
}
```

#### Bottom Line

**Don't add a `Rooms` collection to `RoomType`**. Your current approach using `IRoomRepository.GetAllByRoomType(RoomTypeId)` is the correct DDD pattern. It:

- Maintains aggregate boundaries
- Keeps queries explicit and intentional
- Avoids accidental lazy loading issues
- Follows Clean Architecture principles (repository abstraction in Application layer)

---

## Question 2: Cross-Aggregate Business Rules Location

### User Question

> I have a requirement that the user should not be able to delete a room type that has associated rooms. Is it okay to handle that outside the RoomType aggregate? e.g., in the DeleteRoomType command?

### Current Implementation

```csharp
public class DeleteRoomType
{
    public sealed record Command(RoomTypeId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IRoomRepository _roomRepository;

        public Handler(IRoomTypeRepository roomTypeRepository, IRoomRepository roomRepository)
        {
            _roomTypeRepository = roomTypeRepository;
            _roomRepository = roomRepository;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.GetAllByRoomType(command.id, cancellationToken);

            if (rooms.Count > 0)
            {
                return Result.Invalid(new ValidationError(
                    $"This room type cannot be deleted because it has {rooms.Count} room(s) associated with it. \n Please reassign or remove these rooms from this room type before deleting."));
            }

            return await _roomTypeRepository.Delete(command.id, cancellationToken);
        }
    }
}
```

### Response

Yes, your current approach is **perfectly acceptable and actually preferred** for DDD and Clean Architecture!

#### Why Handling This in the Application Layer is Correct

##### 1. This is a Cross-Aggregate Business Rule

The rule "cannot delete a RoomType if Rooms exist" involves **two separate aggregates** (`RoomType` and `Room`). In DDD:

- **Aggregates should not know about other aggregates' internals**
- Cross-aggregate invariants are enforced at the **Application Layer** (use cases/commands)

##### 2. Your Current Implementation is Clean

```csharp
public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
{
    // Cross-aggregate check - Application Layer responsibility ?
    var rooms = await _roomRepository.GetAllByRoomType(command.id, cancellationToken);

    if (rooms.Count > 0)
    {
        return Result.Invalid(new ValidationError(...));
    }

    return await _roomTypeRepository.Delete(command.id, cancellationToken);
}
```

This is exactly where this logic belongs!

#### Where Different Types of Rules Should Live

| Rule Type | Example | Where to Put It |
|-----------|---------|-----------------|
| **Single Aggregate Invariant** | "Room name cannot be empty" | Inside the Aggregate (`Room` entity) |
| **Cross-Aggregate Rule** | "Cannot delete RoomType with associated Rooms" | Application Layer (Command Handler) ? |
| **Domain Service** | Complex calculations involving multiple aggregates | Domain Service in Core |
| **Database Constraint** | FK constraint preventing orphan records | Infrastructure (EF Config + DB) |

#### Alternative Approaches (For Reference)

##### Option A: Domain Service (Also Valid)

If this rule is reused across multiple use cases, you could create a domain service:

```csharp
// In Core layer
public class RoomTypeDeletionService
{
    public bool CanDelete(RoomType roomType, int associatedRoomCount)
    {
        return associatedRoomCount == 0;
    }
}
```

But for a single use case like yours, **the command handler is simpler and sufficient**.

##### Option B: Database Constraint (Defense in Depth)

You already have this as a backup in your repository:

```csharp
catch (DbUpdateException ex) when (IsForeignKeyConstraintException(ex))
{
    return Result.Conflict("Cannot delete this room type...");
}
```

This is good **defense in depth** - the DB protects data integrity even if the application check fails.

#### Summary

| Aspect | Your Approach | Verdict |
|--------|---------------|---------|
| Cross-aggregate rule in Application Layer | ? | Correct |
| Aggregates remain independent | ? | Correct |
| Clear, explicit business rule | ? | Correct |
| DB constraint as backup | ? | Good practice |

**Your implementation follows DDD and Clean Architecture principles correctly.** The Application Layer is the right place for orchestrating cross-aggregate business rules.

---

## Key Takeaways

1. **Aggregates should reference each other by ID**, not navigation properties
2. **Cross-aggregate queries** should go through repositories, not navigation collections
3. **Cross-aggregate business rules** belong in the Application Layer (command/query handlers)
4. **Single aggregate invariants** belong inside the aggregate itself
5. **Database constraints** provide defense in depth for data integrity
