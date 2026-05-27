using Enrollify.Core.Aggregates.RoomTypeAggregate;
using MediatR;
using Enrollify.Infrastructure.Data;
using MediatR;
using Enrollify.IntegrationTests.Helpers;
using MediatR;
using Enrollify.IntegrationTests.Infrastructure;
using MediatR;
using MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Enrollify.IntegrationTests._Tests.Application;

/// <summary>
/// Sample Application layer integration test demonstrating fixture usage.
/// This is a placeholder to show how to structure Application layer integration tests.
/// </summary>
[Collection("Application")]
public class _SampleApplicationTests
{
    private readonly ApplicationTestFixture _fixture;

    public _SampleApplicationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Sample Application test - verify mediator and database working", Skip = "Placeholder test")]
    public async Task Sample_Application_Test_Verify_Mediator_And_Database_Working()
    {
        // Arrange
        // This test demonstrates how to use the ApplicationTestFixture
        // The fixture provides:
        // - _fixture.SendAsync(): Send commands/queries via Mediator
        // - _fixture.ExecuteDbContextAsync(): Direct database operations
        // - _fixture.ExecuteInScopeAsync(): Execute actions within a service scope

        // Example: Verify database is accessible
        var canConnect = await _fixture.ExecuteDbContextAsync(async db =>
        {
            return await db.Database.CanConnectAsync();
        });

        Assert.True(canConnect);

        // Act & Assert
        // Example: Send a command via Mediator
        // var command = new CreateRoomTypeCommand { Name = "Lecture Hall" };
        // var result = await _fixture.SendAsync(command);
        // Assert.True(result.IsSuccess);

        // Example: Send a query via Mediator
        // var query = new GetRoomTypeByIdQuery { Id = roomTypeId };
        // var result = await _fixture.SendAsync(query);
        // Assert.NotNull(result.Value);

        Assert.True(true); // Placeholder assertion
    }

    [Fact(DisplayName = "Sample test showing direct service access", Skip = "Placeholder test")]
    public async Task Sample_Direct_Service_Access()
    {
        // Access services directly through the fixture's scope
        await _fixture.ExecuteInScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();
            var dbContext = sp.GetRequiredService<EnrollifyDbContext>();

            // Use services as needed
            // Example: Create a test entity directly
            var roomType = TestDataBuilder.CreateRoomType("Computer Lab");
            await dbContext.Set<RoomType>().AddAsync(roomType);
            await dbContext.SaveChangesAsync();

            // Verify it was saved
            var count = await DatabaseHelper.GetCountAsync<RoomType>(dbContext);
            Assert.True(count > 0);
        });

        Assert.True(true); // Placeholder assertion
    }

    [Fact(DisplayName = "Sample test showing test data builder usage", Skip = "Placeholder test")]
    public async Task Sample_Test_Data_Builder_Usage()
    {
        // TestDataBuilder provides convenient methods for creating test entities
        await _fixture.ExecuteDbContextAsync(async db =>
        {
            // Create test entities with Bogus-generated data
            var roomType = TestDataBuilder.CreateRoomType();
            var college = TestDataBuilder.CreateCollege();

            // Or with specific values
            var specificRoomType = TestDataBuilder.CreateRoomType("Specific Name");
            var specificCollege = TestDataBuilder.CreateCollege("CS", "Computer Science");

            // Generate random values
            var email = TestDataBuilder.GenerateEmail();
            var uniqueCode = TestDataBuilder.GenerateUniqueString("TEST");
            var randomUnits = TestDataBuilder.GenerateDecimal(1, 5);

            Assert.NotNull(roomType);
            Assert.NotNull(college);
            Assert.Equal("Specific Name", specificRoomType.Name);

            return true;
        });

        Assert.True(true); // Placeholder assertion
    }

    [Fact(DisplayName = "Sample test showing transaction rollback pattern", Skip = "Placeholder test")]
    public async Task Sample_Transaction_Rollback_Pattern()
    {
        // Use transactions with rollback for tests that shouldn't persist changes
        await _fixture.ExecuteDbContextAsync(async db =>
        {
            await DatabaseHelper.ExecuteInTransactionAsync(db, async () =>
            {
                // Perform operations within transaction
                var roomType = TestDataBuilder.CreateRoomType("Temporary Room Type");
                await db.Set<RoomType>().AddAsync(roomType);
                await db.SaveChangesAsync();

                // Verify within transaction
                var count = await DatabaseHelper.GetCountAsync<RoomType>(db);
                Assert.True(count > 0);

                // Transaction will be rolled back automatically
            });

            // Verify rollback - changes should not persist
            // (This verification would need to be in a new context/scope in real tests)

            return true;
        });

        Assert.True(true); // Placeholder assertion
    }
}
