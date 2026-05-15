---
name: enrollify-integration-tests
description: >
  Step-by-step guide for creating, adding, or updating integration tests in the
  Enrollify backend. Use this skill when asked to write integration tests,
  add test coverage, or create tests for endpoints, commands, or queries.
  Covers both WebAPI integration tests (HTTP endpoints via TestServer) and
  Application layer integration tests (commands/queries via Mediator).
  Located at src/system/EnrollifyBackend/Enrollify.IntegrationTests.
---

# Enrollify Integration Tests Skill

This skill guides you through creating integration tests for the Enrollify backend using xUnit, Testcontainers (MsSQL + Azurite), and test fixtures for WebAPI and Application layers.

**CRITICAL:** Always read the project README first to understand the current infrastructure and patterns.

---

## Reference Files (Read These First)

Before writing any test, always read:

- **README:** `src/system/EnrollifyBackend/Enrollify.IntegrationTests/README.md` — **READ THIS FIRST**
- **WebAPI Sample:** `src/system/EnrollifyBackend/Enrollify.IntegrationTests/_Tests/WebApi/_SampleWebApiTests.cs`
- **Application Sample:** `src/system/EnrollifyBackend/Enrollify.IntegrationTests/_Tests/Application/_SampleApplicationTests.cs`
- **Test Fixtures:** `Infrastructure/WebApiTestFixture.cs` and `Infrastructure/ApplicationTestFixture.cs`
- **Helpers:** `Helpers/TestDataBuilder.cs`, `Helpers/HttpClientExtensions.cs`, `Helpers/DatabaseHelper.cs`

---

## Architecture Overview

### Test Fixtures

1. **WebApiTestFixture** — For testing HTTP endpoints
   - Uses `Microsoft.AspNetCore.Mvc.Testing` with `WebApplicationFactory<Program>`
   - Provides `HttpClient` for HTTP requests
   - Direct `DbContext` access for setup/verification
   - Use `[Collection("WebApi")]` on test classes

2. **ApplicationTestFixture** — For testing Application layer directly
   - Direct access to `IMediator` for commands/queries
   - Direct `DbContext` access
   - Service provider for any registered service
   - Use `[Collection("Application")]` on test classes

### Testcontainers Strategy

- **Shared containers:** MsSQL and Azurite start once per test run (singleton pattern)
- **Isolated databases:** Each test class (fixture instance) gets its own database with migrations (schema + seed + mock data)
- **Performance:** Container reuse speeds up test execution; database creation happens once per test class
- **Isolation:** Each test class has a fresh database; tests within the same class share the database — use unique data, transactions, or cleanup for tests in the same class

---

## Phase 0 — Pre-flight: Determine Test Type

**Decision: WebAPI or Application Layer?**

### Choose WebAPI Test When:
- Testing the full HTTP request/response cycle
- Testing endpoint routing, request binding, validation
- Testing response status codes, headers, serialization
- Testing authentication/authorization at HTTP level
- End-to-end testing through the API surface

### Choose Application Test When:
- Testing command/query handlers directly
- Testing business logic without HTTP overhead
- Testing application services or domain logic
- Faster test execution (no HTTP layer)
- Unit-test-like precision with real database

**Most integration tests should be Application tests unless you specifically need to test HTTP concerns.**

---

## Phase 1 — WebAPI Integration Test Implementation

Use this phase if you chose WebAPI test in Phase 0.

### Step 1.1: Create Test File

**Location:** `_Tests/WebApi/{Feature}{Endpoint}Tests.cs`

**Naming:**
- File: `{Feature}{Endpoint}Tests.cs` (e.g., `RoomTypeEndpointsTests.cs`, `CreateTeacherTests.cs`)
- Class: Same as file name
- Methods: `{MethodName}_{Scenario}_{ExpectedResult}` (e.g., `CreateRoomType_ValidRequest_ReturnsCreated`)

**Template:**
```csharp
using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using Enrollify.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Enrollify.IntegrationTests._Tests.WebApi;

[Collection("WebApi")]
public class RoomTypeEndpointsTests
{
    private readonly WebApiTestFixture _fixture;
    private readonly HttpClient _client;

    public RoomTypeEndpointsTests(WebApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateTestClient();
    }

    [Fact(DisplayName = "Create room type with valid data returns Created")]
    public async Task CreateRoomType_ValidRequest_ReturnsCreated()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### Step 1.2: Implement Test Following AAA Pattern

**Arrange:**
```csharp
// Create unique test data
var request = new CreateRoomTypeRequest
{
    Name = TestDataBuilder.GenerateUniqueString("RoomType"),
    Description = "Test room type description"
};
```

**Act:**
```csharp
// Make HTTP request
var response = await _client.PostAsJsonAsync("/api/room-types", request);
```

**Assert:**
```csharp
// Verify response
Assert.Equal(HttpStatusCode.Created, response.StatusCode);

// Optionally verify in database
await _fixture.ExecuteInScopeAsync(async sp =>
{
    var dbContext = sp.GetRequiredService<EnrollifyDbContext>();
    var roomType = await dbContext.Set<RoomType>()
        .FirstOrDefaultAsync(rt => rt.Name == request.Name);
    Assert.NotNull(roomType);
    Assert.Equal(request.Description, roomType.Description);
});
```

### Step 1.3: Use HttpClientExtensions for Convenience

```csharp
// GET with deserialization
var roomTypes = await _client.GetFromJsonAsync<List<RoomTypeDto>>("/api/room-types");

// POST with request/response
var created = await _client.PostAsJsonAsync<CreateRoomTypeRequest, RoomTypeDto>(
    "/api/room-types", 
    request
);

// PUT with request/response
var updated = await _client.PutAsJsonAsync<UpdateRoomTypeRequest, RoomTypeDto>(
    $"/api/room-types/{id}", 
    updateRequest
);

// DELETE
await _client.DeleteAndEnsureSuccessAsync($"/api/room-types/{id}");

// With authentication (if needed)
var authenticatedClient = _client.WithBearerToken("test-token");
```

### Step 1.4: Test Data Setup and Cleanup

**Setup test data:**
```csharp
// Use DatabaseHelper to seed prerequisite data
await _fixture.ExecuteInScopeAsync(async sp =>
{
    var dbContext = sp.GetRequiredService<EnrollifyDbContext>();
    var roomType = TestDataBuilder.CreateRoomType("Prerequisite RoomType");
    await DatabaseHelper.SeedEntitiesAsync(dbContext, roomType);
});
```

**Cleanup (if needed):**
```csharp
// Option 1: Explicit cleanup in finally block
RoomTypeId? createdId = null;
try
{
    // Test code
    var response = await _client.PostAsJsonAsync<CreateRoomTypeRequest, RoomTypeDto>(...);
    createdId = response.Id;
    
    // Assertions
}
finally
{
    if (createdId != null)
    {
        await _fixture.ExecuteDbContextAsync(async db =>
        {
            var entity = await db.RoomTypes.FindAsync(createdId);
            if (entity != null)
            {
                db.RoomTypes.Remove(entity);
                await db.SaveChangesAsync();
            }
        });
    }
}

// Option 2: Use unique data per test (recommended)
var uniqueName = TestDataBuilder.GenerateUniqueString("RoomType");
```

---

## Phase 2 — Application Layer Integration Test Implementation

Use this phase if you chose Application test in Phase 0.

### Step 2.1: Create Test File

**Location:** `_Tests/Application/{Feature}/{CommandOrQuery}Tests.cs`

**Naming:**
- File: `{CommandOrQuery}Tests.cs` (e.g., `CreateRoomTypeCommandTests.cs`, `GetRoomTypeByIdQueryTests.cs`)
- Class: Same as file name
- Methods: `{CommandOrQueryName}_{Scenario}_{ExpectedResult}`

**Template:**
```csharp
using Enrollify.Application.Features.RoomTypes.Commands;
using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using Enrollify.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Enrollify.IntegrationTests._Tests.Application.RoomTypes;

[Collection("Application")]
public class CreateRoomTypeCommandTests
{
    private readonly ApplicationTestFixture _fixture;

    public CreateRoomTypeCommandTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Create room type command with valid data succeeds")]
    public async Task CreateRoomType_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### Step 2.2: Implement Test Following AAA Pattern

**Arrange:**
```csharp
// Create unique command
var command = new CreateRoomType.Command
{
    Name = TestDataBuilder.GenerateUniqueString("RoomType"),
    Description = "Test room type description"
};
```

**Act:**
```csharp
// Send via Mediator
var result = await _fixture.SendAsync(command);
```

**Assert:**
```csharp
// Verify Ardalis.Result
Assert.True(result.IsSuccess);
Assert.NotNull(result.Value);

// Optionally verify in database
await _fixture.ExecuteDbContextAsync(async db =>
{
    var roomType = await db.RoomTypes.FindAsync(result.Value);
    Assert.NotNull(roomType);
    Assert.Equal(command.Name, roomType.Name);
    Assert.Equal(command.Description, roomType.Description);
});
```

### Step 2.3: Use Fixture Helper Methods

```csharp
// Send command
var result = await _fixture.SendAsync(command);

// Send query
var result = await _fixture.SendAsync(query);

// Execute in scope (for complex setup)
await _fixture.ExecuteInScopeAsync(async sp =>
{
    var mediator = sp.GetRequiredService<IMediator>();
    var dbContext = sp.GetRequiredService<EnrollifyDbContext>();
    
    // Custom logic here
});

// Execute DbContext operation
var roomType = await _fixture.ExecuteDbContextAsync(async db =>
{
    return await db.RoomTypes.FirstOrDefaultAsync(rt => rt.Name == "Test");
});
```

### Step 2.4: Test Validation and Error Cases

```csharp
[Fact(DisplayName = "Create room type with empty name returns validation error")]
public async Task CreateRoomType_EmptyName_ReturnsValidationError()
{
    // Arrange
    var command = new CreateRoomType.Command
    {
        Name = "",  // Invalid
        Description = "Test"
    };

    // Act
    var result = await _fixture.SendAsync(command);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.NotEmpty(result.ValidationErrors);
}

[Fact(DisplayName = "Create room type with duplicate name returns conflict")]
public async Task CreateRoomType_DuplicateName_ReturnsConflict()
{
    // Arrange
    var existingName = TestDataBuilder.GenerateUniqueString("RoomType");
    
    // Seed existing entity
    await _fixture.ExecuteDbContextAsync(async db =>
    {
        var existing = TestDataBuilder.CreateRoomType(existingName);
        await db.RoomTypes.AddAsync(existing);
        await db.SaveChangesAsync();
    });

    var command = new CreateRoomType.Command
    {
        Name = existingName,  // Duplicate
        Description = "Test"
    };

    // Act
    var result = await _fixture.SendAsync(command);

    // Assert
    Assert.False(result.IsSuccess);
    // Verify appropriate error status/message
}
```

---

## Phase 3 — Test Data Management

### Test Isolation Strategy

**Each test class gets its own isolated database** with fresh migrations (schema + seed + mock data).

- **Test classes are fully isolated** from each other - no shared state between different test files
- **Tests within the same class** share the same database instance
- **Recommended**: Use unique data generation to avoid conflicts between tests in the same class

### Strategy 1: Unique Data (Recommended)

Use `TestDataBuilder.GenerateUniqueString()` to avoid conflicts between tests in the same class:

```csharp
var command = new CreateRoomType.Command
{
    Name = TestDataBuilder.GenerateUniqueString("RoomType"),
    Description = "Test description"
};
```

### Strategy 2: Transaction Rollback (For Non-Persisting Tests Within Same Class)

Use for tests that should not persist data in the shared database:

```csharp
await _fixture.ExecuteDbContextAsync(async db =>
{
    await DatabaseHelper.ExecuteInTransactionAsync(db, async () =>
    {
        // All operations here will be rolled back
        var entity = TestDataBuilder.CreateRoomType("Temporary");
        await db.RoomTypes.AddAsync(entity);
        await db.SaveChangesAsync();
        
        // Assertions
        var count = await DatabaseHelper.GetCountAsync<RoomType>(db);
        Assert.True(count > 0);
        
        // Transaction rolls back automatically
    });
});
```

### Strategy 3: Explicit Cleanup (If Needed Within Same Class)

Only use if tests within the same class interfere with each other:

```csharp
RoomTypeId? createdId = null;
try
{
    // Create test data
    var result = await _fixture.SendAsync(createCommand);
    createdId = result.Value;
    
    // Test logic
    // ...
}
finally
{
    // Cleanup
    if (createdId != null)
    {
        await _fixture.ExecuteDbContextAsync(async db =>
        {
            var entity = await db.RoomTypes.FindAsync(createdId);
            if (entity != null)
            {
                db.RoomTypes.Remove(entity);
                await db.SaveChangesAsync();
            }
        });
    }
}
```

### Using TestDataBuilder

```csharp
// Simple entities
var roomType = TestDataBuilder.CreateRoomType("Custom Name");
var college = TestDataBuilder.CreateCollege("CS", "Computer Science");

// Complex entities with dependencies (create dependencies first)
await _fixture.ExecuteDbContextAsync(async db =>
{
    // Create prerequisites
    var college = TestDataBuilder.CreateCollege();
    await db.Colleges.AddAsync(college);
    await db.SaveChangesAsync();
    
    // Create entity with foreign key
    var building = new Building(
        "Main Building",
        "Main campus building",
        "123 University Ave",
        college.Id
    );
    await db.Buildings.AddAsync(building);
    await db.SaveChangesAsync();
});

// Generate random values
var email = TestDataBuilder.GenerateEmail();
var uniqueCode = TestDataBuilder.GenerateUniqueString("TEST");
var randomUnits = TestDataBuilder.GenerateDecimal(1, 5);
```

---

## Phase 4 — Testing Queries (List, Get, Filter)

### Testing List Query

```csharp
[Fact(DisplayName = "List room types returns all active entities")]
public async Task ListRoomTypes_ReturnsActiveEntities()
{
    // Arrange - Seed test data
    await _fixture.ExecuteDbContextAsync(async db =>
    {
        var roomType1 = TestDataBuilder.CreateRoomType("Test Type 1");
        var roomType2 = TestDataBuilder.CreateRoomType("Test Type 2");
        await DatabaseHelper.SeedEntitiesAsync(db, roomType1, roomType2);
    });

    var query = new ListRoomTypes.Query
    {
        Page = 1,
        PerPage = 10
    };

    // Act
    var result = await _fixture.SendAsync(query);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.True(result.Value.Items.Count >= 2);
}
```

### Testing Get By ID Query

```csharp
[Fact(DisplayName = "Get room type by ID returns correct entity")]
public async Task GetRoomTypeById_ExistingId_ReturnsEntity()
{
    // Arrange - Create and get ID
    var roomTypeId = await _fixture.ExecuteDbContextAsync(async db =>
    {
        var roomType = TestDataBuilder.CreateRoomType("Test Type");
        await db.RoomTypes.AddAsync(roomType);
        await db.SaveChangesAsync();
        return roomType.Id;
    });

    var query = new GetRoomTypeById.Query { Id = roomTypeId };

    // Act
    var result = await _fixture.SendAsync(query);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(roomTypeId, result.Value.Id);
}

[Fact(DisplayName = "Get room type by non-existent ID returns not found")]
public async Task GetRoomTypeById_NonExistentId_ReturnsNotFound()
{
    // Arrange
    var nonExistentId = RoomTypeId.From(Guid.NewGuid());
    var query = new GetRoomTypeById.Query { Id = nonExistentId };

    // Act
    var result = await _fixture.SendAsync(query);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.NotFound, result.Status);
}
```

### Testing Filtering

```csharp
[Fact(DisplayName = "List room types with filter returns matching entities")]
public async Task ListRoomTypes_WithFilter_ReturnsMatchingEntities()
{
    // Arrange
    var uniquePrefix = TestDataBuilder.GenerateUniqueString("Filter");
    await _fixture.ExecuteDbContextAsync(async db =>
    {
        var match = TestDataBuilder.CreateRoomType($"{uniquePrefix}_Match");
        var noMatch = TestDataBuilder.CreateRoomType("NoMatch");
        await DatabaseHelper.SeedEntitiesAsync(db, match, noMatch);
    });

    var query = new ListRoomTypes.Query
    {
        Filters = new List<FilterItemDto>
        {
            new() { Field = "name", Operator = "contains", Value = uniquePrefix }
        }
    };

    // Act
    var result = await _fixture.SendAsync(query);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.All(result.Value.Items, item => 
        Assert.Contains(uniquePrefix, item.Name)
    );
}
```

---

## Phase 5 — Testing Commands (Create, Update, Delete)

### Testing Create Command

```csharp
[Fact(DisplayName = "Create room type with valid data succeeds")]
public async Task CreateRoomType_ValidData_Succeeds()
{
    // Arrange
    var command = new CreateRoomType.Command
    {
        Name = TestDataBuilder.GenerateUniqueString("RoomType"),
        Description = "Test description"
    };

    // Act
    var result = await _fixture.SendAsync(command);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    
    // Verify in database
    var exists = await _fixture.ExecuteDbContextAsync(async db =>
    {
        return await DatabaseHelper.ExistsAsync<RoomType>(db, result.Value);
    });
    Assert.True(exists);
}
```

### Testing Update Command

```csharp
[Fact(DisplayName = "Update room type with valid data succeeds")]
public async Task UpdateRoomType_ValidData_Succeeds()
{
    // Arrange - Create entity
    var roomTypeId = await _fixture.ExecuteDbContextAsync(async db =>
    {
        var roomType = TestDataBuilder.CreateRoomType("Original Name");
        await db.RoomTypes.AddAsync(roomType);
        await db.SaveChangesAsync();
        return roomType.Id;
    });

    var command = new UpdateRoomType.Command
    {
        Id = roomTypeId,
        Name = "Updated Name",
        Description = "Updated description"
    };

    // Act
    var result = await _fixture.SendAsync(command);

    // Assert
    Assert.True(result.IsSuccess);
    
    // Verify changes persisted
    var updated = await _fixture.ExecuteDbContextAsync(async db =>
    {
        return await db.RoomTypes.FindAsync(roomTypeId);
    });
    Assert.Equal("Updated Name", updated!.Name);
    Assert.Equal("Updated description", updated.Description);
}
```

### Testing Delete Command

```csharp
[Fact(DisplayName = "Delete room type with valid ID succeeds")]
public async Task DeleteRoomType_ValidId_Succeeds()
{
    // Arrange - Create entity
    var roomTypeId = await _fixture.ExecuteDbContextAsync(async db =>
    {
        var roomType = TestDataBuilder.CreateRoomType("To Delete");
        await db.RoomTypes.AddAsync(roomType);
        await db.SaveChangesAsync();
        return roomType.Id;
    });

    var command = new DeleteRoomType.Command { Id = roomTypeId };

    // Act
    var result = await _fixture.SendAsync(command);

    // Assert
    Assert.True(result.IsSuccess);
    
    // Verify soft delete (or hard delete based on implementation)
    var deleted = await _fixture.ExecuteDbContextAsync(async db =>
    {
        var entity = await db.RoomTypes.FindAsync(roomTypeId);
        return entity?.DeletedAt != null; // Soft delete check
        // OR: return entity == null; // Hard delete check
    });
    Assert.True(deleted);
}
```

---

## Phase 6 — Advanced Scenarios

### Testing with Related Entities

```csharp
[Fact(DisplayName = "Create building with college relationship succeeds")]
public async Task CreateBuilding_WithCollege_Succeeds()
{
    // Arrange - Create prerequisite college
    var collegeId = await _fixture.ExecuteDbContextAsync(async db =>
    {
        var college = TestDataBuilder.CreateCollege();
        await db.Colleges.AddAsync(college);
        await db.SaveChangesAsync();
        return college.Id;
    });

    var command = new CreateBuilding.Command
    {
        Name = TestDataBuilder.GenerateUniqueString("Building"),
        Description = "Test building",
        Address = "123 Test St",
        CollegeId = collegeId
    };

    // Act
    var result = await _fixture.SendAsync(command);

    // Assert
    Assert.True(result.IsSuccess);
    
    // Verify relationship
    var building = await _fixture.ExecuteDbContextAsync(async db =>
    {
        return await db.Buildings
            .Include(b => b.College)
            .FirstOrDefaultAsync(b => b.Id == result.Value);
    });
    Assert.NotNull(building!.College);
    Assert.Equal(collegeId, building.CollegeId);
}
```

### Testing Pagination

```csharp
[Fact(DisplayName = "List room types with pagination returns correct page")]
public async Task ListRoomTypes_WithPagination_ReturnsCorrectPage()
{
    // Arrange - Seed multiple entities
    await _fixture.ExecuteDbContextAsync(async db =>
    {
        var roomTypes = Enumerable.Range(1, 15)
            .Select(i => TestDataBuilder.CreateRoomType($"Type {i}"))
            .ToArray();
        await DatabaseHelper.SeedEntitiesAsync(db, roomTypes);
    });

    var query = new ListRoomTypes.Query
    {
        Page = 2,
        PerPage = 5
    };

    // Act
    var result = await _fixture.SendAsync(query);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(5, result.Value.Items.Count);
    Assert.Equal(2, result.Value.Page);
    Assert.True(result.Value.TotalCount >= 15);
}
```

### Testing Authorization (If Implemented)

```csharp
[Fact(DisplayName = "Delete room type without permission returns forbidden")]
public async Task DeleteRoomType_WithoutPermission_ReturnsForbidden()
{
    // Note: This requires test authentication setup in fixtures
    
    // Arrange
    var roomTypeId = await _fixture.ExecuteDbContextAsync(async db =>
    {
        var roomType = TestDataBuilder.CreateRoomType("Protected");
        await db.RoomTypes.AddAsync(roomType);
        await db.SaveChangesAsync();
        return roomType.Id;
    });

    // Act - Using client without proper authorization
    var response = await _client.DeleteAsync($"/api/room-types/{roomTypeId}");

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

---

## Common Patterns & Best Practices

### 1. Test Naming

```csharp
// ✅ Good
[Fact(DisplayName = "Create room type with valid data returns success")]
public async Task CreateRoomType_ValidData_ReturnsSuccess()

// ❌ Bad
[Fact]
public async Task Test1()
```

### 2. Unique Test Data

```csharp
// ✅ Good - Unique per test
var name = TestDataBuilder.GenerateUniqueString("RoomType");

// ❌ Bad - Hardcoded, may conflict
var name = "Test RoomType";
```

### 3. Cleanup Strategy

```csharp
// ✅ Good - No cleanup needed with unique data
var uniqueName = TestDataBuilder.GenerateUniqueString("Temp");

// ✅ Also Good - Transaction rollback for isolated tests
await DatabaseHelper.ExecuteInTransactionAsync(db, async () => { /*...*/ });

// ⚠️ Use Sparingly - Explicit cleanup only when necessary
try { /* test */ } finally { /* cleanup */ }
```

### 4. Assertions

```csharp
// ✅ Good - Clear, specific assertions
Assert.True(result.IsSuccess);
Assert.NotNull(result.Value);
Assert.Equal(expectedName, actual.Name);

// ❌ Bad - Vague assertion
Assert.True(result != null);
```

### 5. Arrange-Act-Assert Structure

```csharp
// ✅ Good - Clear AAA structure with comments
// Arrange
var command = new CreateCommand { /* ... */ };

// Act
var result = await _fixture.SendAsync(command);

// Assert
Assert.True(result.IsSuccess);

// ❌ Bad - Mixed, unclear
var command = new CreateCommand { /* ... */ };
Assert.True(result.IsSuccess);  // result doesn't exist yet!
var result = await _fixture.SendAsync(command);
```

---

## Troubleshooting

### Docker Not Running
**Error:** Testcontainers fails to start  
**Solution:** Ensure Docker Desktop is running

### Connection String Issues
**Error:** Cannot connect to database  
**Solution:** Verify `TestContainersManager` is initializing correctly. Check logs for container startup.

### Entity Already Tracked
**Error:** Entity with the same ID is already tracked  
**Solution:** Use `DatabaseHelper.DetachAllEntities(dbContext)` or create new scopes via `_fixture.ExecuteInScopeAsync()`

### Test Timeout
**Error:** Test exceeds timeout  
**Solution:** Increase xUnit timeout or check for hanging database operations. Ensure containers started successfully.

### Obsolete Testcontainer Builder Warning
**Status:** Warning only, not an error  
**Solution:** Already using `.WithImage()` — warning can be ignored or upgrade to newer Testcontainers version

---

## Checklist

Before considering a test complete:

- [ ] Test file in correct directory (`_Tests/WebApi/` or `_Tests/Application/`)
- [ ] Correct `[Collection]` attribute (`"WebApi"` or `"Application"`)
- [ ] Descriptive test name following `{Method}_{Scenario}_{Result}` pattern
- [ ] `[Fact(DisplayName = "...")]` with clear description
- [ ] Clear AAA (Arrange-Act-Assert) structure
- [ ] Uses unique test data (no hardcoded conflicting values)
- [ ] Proper assertions (not just `Assert.True(true)`)
- [ ] Cleanup strategy considered (unique data, transaction, or explicit)
- [ ] Runs successfully: `dotnet test --filter "YourTestName"`
- [ ] No leftover test data polluting shared database

---

## Running Tests

```bash
# Run all integration tests
dotnet test Enrollify.IntegrationTests

# Run specific collection
dotnet test --filter "WebApi"
dotnet test --filter "Application"

# Run specific test class
dotnet test --filter "RoomTypeEndpointsTests"

# Run single test method
dotnet test --filter "FullyQualifiedName~CreateRoomType_ValidRequest_ReturnsCreated"
```

---

## Summary

1. **Read README.md first** — Always start here to understand the infrastructure
2. **Choose test type** — WebAPI (HTTP) or Application (direct Mediator)
3. **Follow templates** — Use provided templates and sample tests
4. **Use unique data** — Generate unique strings to avoid conflicts
5. **Test thoroughly** — Success cases, validation errors, edge cases
6. **Clean structure** — AAA pattern, clear naming, descriptive display names
7. **Verify it works** — Run the test before considering it complete

The integration test infrastructure is fully set up and ready. Focus on writing clear, maintainable tests that provide value!
