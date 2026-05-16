using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using Enrollify.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Enrollify.IntegrationTests._Tests.WebApi;

/// <summary>
/// Sample WebAPI integration test demonstrating fixture usage.
/// This is a placeholder to show how to structure WebAPI integration tests.
/// </summary>
[Collection("WebApi")]
public class _SampleWebApiTests
{
    private readonly WebApiTestFixture _fixture;
    private readonly HttpClient _client;

    public _SampleWebApiTests(WebApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateTestClient();
    }

    [Fact(DisplayName = "Sample WebAPI test - verify testcontainers are working", Skip = "Placeholder test")]
    public async Task Sample_WebApi_Test_Verify_Testcontainers_Working()
    {
        // Arrange
        // This test demonstrates how to use the WebApiTestFixture
        // The fixture provides:
        // - _client: HttpClient for making HTTP requests
        // - _fixture.DbContext: Direct database access for setup/verification
        // - _fixture.ExecuteInScopeAsync(): Execute actions within a service scope

        // Example: Verify database is accessible
        await _fixture.ExecuteInScopeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<EnrollifyDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync();
            Assert.True(canConnect);
        });

        // Act
        // Example: Make an HTTP request to an endpoint
        // var response = await _client.GetAsync("/api/rooms");

        // Assert
        // Example: Verify response
        // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.True(true); // Placeholder assertion
    }

    [Fact(DisplayName = "Sample test showing HttpClient extensions usage", Skip = "Placeholder test")]
    public async Task Sample_HttpClient_Extensions_Usage()
    {
        // The HttpClientExtensions provide convenient methods:
        
        // Example GET with deserialization
        // var rooms = await _client.GetFromJsonAsync<List<RoomDto>>("/api/rooms");

        // Example POST with request/response
        // var request = new CreateRoomRequest { Name = "Test Room", ... };
        // var response = await _client.PostAsJsonAsync<CreateRoomRequest, RoomDto>("/api/rooms", request);

        // Example with Bearer token
        // var authenticatedClient = _client.WithBearerToken("your-token-here");
        // var response = await authenticatedClient.GetAsync("/api/protected-resource");

        Assert.True(true); // Placeholder assertion
    }

    [Fact(DisplayName = "Sample test showing database setup and verification", Skip = "Placeholder test")]
    public async Task Sample_Database_Setup_And_Verification()
    {
        // Use DatabaseHelper for common database operations
        await _fixture.ExecuteInScopeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<EnrollifyDbContext>();

            // Seed test data
            var testRoom = TestDataBuilder.CreateRoomType("TestRoomType");
            await DatabaseHelper.SeedEntitiesAsync(dbContext, testRoom);

            // Verify entity exists
            var count = await DatabaseHelper.GetCountAsync<Enrollify.Core.Aggregates.RoomTypeAggregate.RoomType>(dbContext);
            Assert.True(count > 0);

            // Clean up (if needed)
            // await DatabaseHelper.ClearTableAsync<RoomType>(dbContext);
        });

        Assert.True(true); // Placeholder assertion
    }
}
