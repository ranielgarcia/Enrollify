namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition for Application layer integration tests.
/// All tests marked with [Collection("Application")] will share the same ApplicationTestFixture instance.
/// This ensures testcontainers are shared across tests for performance.
/// </summary>
[CollectionDefinition("Application")]
public class ApplicationTestCollection : ICollectionFixture<ApplicationTestFixture>
{
    // This class is never instantiated. It's just a marker for xUnit.
}
