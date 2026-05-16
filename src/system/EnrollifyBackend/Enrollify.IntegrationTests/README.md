# Enrollify Integration Tests

This project contains integration tests for the Enrollify backend using **xUnit**, **Testcontainers**, and **Microsoft.AspNetCore.Mvc.Testing**.

## Overview

The integration test infrastructure provides two distinct test fixtures:

1. **WebApiTestFixture** - For testing HTTP endpoints via TestServer
2. **ApplicationTestFixture** - For testing Application layer (commands, queries, services) directly

Both fixtures share the same testcontainers (MsSQL + Azurite) for optimal performance while maintaining isolation through test collections.

## Architecture

### Test Containers Strategy

- **Shared Containers**: MsSQL and Azurite containers are started once per test run and shared across all tests
- **Isolated Databases**: Each test class (fixture instance) gets its own unique database with full migrations (schema + seed + mock data)
- **Performance**: Container reuse significantly speeds up test execution; database creation happens once per test class
- **Isolation**: Each test class works with a fresh database, preventing test interference and avoiding shared state conflicts

### Test Fixtures

#### WebApiTestFixture

Used for testing the full HTTP request/response cycle through the WebAPI.

**Features:**
- Hosts the WebAPI using `WebApplicationFactory<Program>`
- Provides `HttpClient` for making HTTP requests
- Testcontainer connection strings injected automatically
- Direct `DbContext` access for test setup and verification
- Service scope helpers for accessing scoped services

**Usage:**
```csharp
[Collection("WebApi")]
public class RoomEndpointTests
{
    private readonly WebApiTestFixture _fixture;
    private readonly HttpClient _client;

    public RoomEndpointTests(WebApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateTestClient();
    }

    [Fact]
    public async Task CreateRoom_ReturnsCreated()
    {
        // Arrange
        var request = new CreateRoomRequest { /* ... */ };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rooms", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

#### ApplicationTestFixture

Used for testing Application layer logic (commands, queries, services) without the HTTP layer.

**Features:**
- Direct access to `IMediator` for sending commands/queries
- Direct `DbContext` access for database operations
- Service provider for accessing any registered service
- Helper methods for common operations

**Usage:**
```csharp
[Collection("Application")]
public class CreateRoomCommandTests
{
    private readonly ApplicationTestFixture _fixture;

    public CreateRoomCommandTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateRoom_Command_Success()
    {
        // Arrange
        var command = new CreateRoomCommand { /* ... */ };

        // Act
        var result = await _fixture.SendAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
```

## Test Collections

xUnit collections ensure that tests sharing the same fixture run sequentially to avoid conflicts:

- `[Collection("WebApi")]` - For WebAPI endpoint tests
- `[Collection("Application")]` - For Application layer tests

Tests in different collections can run in parallel.

## Helper Utilities

### TestDataBuilder

Creates test entities with realistic fake data using Bogus:

```csharp
var roomType = TestDataBuilder.CreateRoomType("Lecture Hall");
var college = TestDataBuilder.CreateCollege("CS", "Computer Science");
var email = TestDataBuilder.GenerateEmail();
```

### HttpClientExtensions

Convenient methods for HTTP operations:

```csharp
var rooms = await _client.GetFromJsonAsync<List<RoomDto>>("/api/rooms");
var room = await _client.PostAsJsonAsync<CreateRoomRequest, RoomDto>("/api/rooms", request);
var authenticatedClient = _client.WithBearerToken("token");
```

### DatabaseHelper

Common database operations:

```csharp
// Seed test data
await DatabaseHelper.SeedEntitiesAsync(dbContext, entity1, entity2);

// Clear a table
await DatabaseHelper.ClearTableAsync<RoomType>(dbContext);

// Get count
var count = await DatabaseHelper.GetCountAsync<Room>(dbContext);

// Transaction with rollback
await DatabaseHelper.ExecuteInTransactionAsync(dbContext, async () => {
    // Operations here will be rolled back
});
```

## Running Tests

### Run All Integration Tests
```bash
dotnet test Enrollify.IntegrationTests
```

### Run Specific Collection
```bash
dotnet test --filter "WebApi"
dotnet test --filter "Application"
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~YourTestMethodName"
```

## Writing New Tests

### WebAPI Test Template

```csharp
using Enrollify.IntegrationTests.Infrastructure;

namespace Enrollify.IntegrationTests._Tests.WebApi;

[Collection("WebApi")]
public class YourEndpointTests
{
    private readonly WebApiTestFixture _fixture;
    private readonly HttpClient _client;

    public YourEndpointTests(WebApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateTestClient();
    }

    [Fact(DisplayName = "Description of what this test does")]
    public async Task YourTest_Scenario_ExpectedResult()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### Application Test Template

```csharp
using Enrollify.IntegrationTests.Infrastructure;

namespace Enrollify.IntegrationTests._Tests.Application;

[Collection("Application")]
public class YourCommandOrQueryTests
{
    private readonly ApplicationTestFixture _fixture;

    public YourCommandOrQueryTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Description of what this test does")]
    public async Task YourTest_Scenario_ExpectedResult()
    {
        // Arrange
        
        // Act
        var result = await _fixture.SendAsync(yourCommand);
        
        // Assert
        Assert.True(result.IsSuccess);
    }
}
```

## Test Data Management

### Test Isolation Strategy

**Each test class gets its own isolated database** with fresh migrations (schema + seed + mock data). This means:

- **Test classes are fully isolated** from each other - no shared state between test classes
- **Tests within the same class** share the same database instance
- **Recommended**: Use `TestDataBuilder.GenerateUniqueString()` for creating unique data within a test class to avoid conflicts between tests in the same class

### Strategies for Test Isolation

1. **Unique Data (Recommended)**: Use `TestDataBuilder.GenerateUniqueString()` for codes/names to avoid conflicts between tests in the same class
2. **Transactions with Rollback**: Use `DatabaseHelper.ExecuteInTransactionAsync()` for non-persisting tests within the same test class
3. **Cleanup (If Needed)**: Explicitly clean up test data if tests within the same class interfere with each other
4. **Seed Data**: Use the pre-seeded data from migrations, or add specific test data at the start of each test

### Example: Test with Cleanup

```csharp
[Fact]
public async Task CreateRoom_Test()
{
    RoomId? createdRoomId = null;
    try
    {
        // Arrange & Act
        var command = new CreateRoomCommand { /* ... */ };
        var result = await _fixture.SendAsync(command);
        createdRoomId = result.Value;
        
        // Assert
        Assert.True(result.IsSuccess);
    }
    finally
    {
        // Cleanup
        if (createdRoomId != null)
        {
            await _fixture.ExecuteDbContextAsync(async db =>
            {
                var room = await db.Rooms.FindAsync(createdRoomId);
                if (room != null)
                {
                    db.Rooms.Remove(room);
                    await db.SaveChangesAsync();
                }
            });
        }
    }
}
```

## Authentication/Authorization

Currently, authentication is disabled in the test configuration. When testing authenticated endpoints:

1. **Option 1**: Use test authentication handlers that accept test tokens
2. **Option 2**: Directly set user claims in the test service configuration
3. **Option 3**: Use the `HttpClientExtensions.WithBearerToken()` method with a valid test token

## Performance Considerations

- **Container Startup**: Containers start once (~10-20 seconds overhead per test run)
- **Database Migrations**: Migrations run once per test run
- **Shared State**: Tests share the database - design tests to be independent or use transactions
- **Parallel Execution**: Tests in different collections run in parallel by default

## Troubleshooting

### Tests Fail with Connection Issues
- Ensure Docker is running (required for Testcontainers)
- Check that ports 1433 (SQL) and 10000 (Azurite) are available

### Tests Fail with "Database does not exist"
- The `TestContainersManager` handles database creation automatically
- If issues persist, check the `TestContainersManager` logs in test output

### Slow Test Execution
- Verify containers are being shared (check console output)
- Consider reducing the number of integration tests or splitting into focused test classes
- Use unit tests for logic that doesn't require database/HTTP

### Entity Already Tracked Error
- Use `DatabaseHelper.DetachAllEntities()` to clear the change tracker
- Create new scopes for database operations: `_fixture.ExecuteInScopeAsync()`

## CI/CD Integration

Integration tests can be run in CI/CD pipelines that support Docker:

- GitHub Actions: ✅ (Docker available)
- Azure DevOps: ✅ (Requires hosted agents with Docker)
- GitLab CI: ✅ (Docker-in-Docker or Docker executor)

Ensure the pipeline has sufficient resources (RAM/CPU) for running containers.

## Additional Resources

- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [xUnit Documentation](https://xunit.net/)
- [Microsoft.AspNetCore.Mvc.Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Bogus - Fake Data Generator](https://github.com/bchavez/Bogus)
