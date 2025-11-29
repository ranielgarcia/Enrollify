# Loading Navigation properties

In DDD and Clean Architecture, navigation properties should generally NOT be exposed publicly to maintain aggregate boundaries and encapsulation. Your current approach is correct. Here's why and how to handle this:

Best Practices

1. Keep Navigation Properties Private (Your Current Approach ✓)

```C#
// ✓ CORRECT - Maintains aggregate boundary
private RoomType? _roomType;
```

Reasons:

* Aggregate Independence: Each aggregate should be independently loadable and modifiable
* Encapsulation: Prevents external code from directly manipulating related entities
* Bounded Context Integrity: Enforces that aggregates communicate through IDs, not direct references

2. Expose Only IDs Publicly (Already Implemented ✓)

```C#
// ✓ CORRECT - Only the ID is exposed
public RoomTypeId RoomTypeId { get; private set; }
```

3. When You Need Related Data

If you need to display RoomType information alongside Room, handle it at the Application/Use Case layer:

```C#
// In your Application/Query layer
public class RoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int StudentCapacity { get; set; }
    
    // Compose related data in DTOs
    public RoomTypeDto RoomType { get; set; }
}

// In your query handler
var room = await _roomRepository.GetByIdAsync(roomId);
var roomType = await _roomTypeRepository.GetByIdAsync(room.RoomTypeId.Value);

return new RoomDto 
{
    // ... map room properties
    RoomType = MapToRoomTypeDto(roomType)
};
```

4. Alternative: Read-Only Access (Use Sparingly)

Only if there's a genuine domain behavior need:

```C#
// Only if required for domain logic
public RoomType? RoomType => _roomType;
```

⚠️ However, this is rarely needed. Most "need to access related data" scenarios are actually presentation concerns that belong in the Application layer.

## ❌ Don't Do This (Breaking Encapsulation)

```C#
// Domain entity exposing navigation property
public class Room : EntityBase<Room, RoomId>
{
    public RoomType RoomType { get; set; } // ❌ Public navigation property
}

// Application code directly accessing it
var room = await _context.Rooms
    .Include(r => r.RoomType) // Loads into public property
    .FirstOrDefaultAsync();

var roomTypeName = room.RoomType.Name; // ❌ Tight coupling
```

## ✓ Do This (Maintaining Aggregate Boundaries)


```C#
// Domain entity - keeps navigation private (your current approach)
public class Room : EntityBase<Room, RoomId>
{
    private RoomType? _roomType; // ✓ Private
    public RoomTypeId RoomTypeId { get; private set; } // ✓ Only ID exposed
}

// Application/Query layer - load separately when needed
public class GetRoomWithTypeQueryHandler
{
    public async Task<RoomDto> Handle(GetRoomWithTypeQuery request)
    {
        // Option 1: Load separately (more explicit aggregate boundaries)
        var room = await _roomRepository.GetByIdAsync(request.RoomId);
        var roomType = await _roomTypeRepository.GetByIdAsync(room.RoomTypeId);
        
        return new RoomDto 
        {
            Id = room.Id.Value,
            Name = room.Name.Value,
            RoomType = new RoomTypeDto 
            {
                Id = roomType.Id.Value,
                Name = roomType.Name.Value
            }
        };
        
        // Option 2: Use EF Include in a read-specific query (for performance)
        // This is acceptable in the infrastructure layer for READ operations
        var query = await _context.Rooms
            .Include(r => r._roomType) // EF can access private fields
            .Where(r => r.Id == request.RoomId)
            .Select(r => new RoomDto 
            {
                Id = r.Id.Value,
                Name = r.Name.Value,
                RoomType = new RoomTypeDto 
                {
                    Id = r._roomType.Id.Value,
                    Name = r._roomType.Name.Value
                }
            })
            .FirstOrDefaultAsync();
    }
}
```