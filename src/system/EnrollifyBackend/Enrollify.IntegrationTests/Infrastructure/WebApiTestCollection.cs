namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition for WebAPI integration tests.
/// All tests marked with [Collection("WebApi")] will share the same WebApiTestFixture instance.
/// This ensures testcontainers are shared across tests for performance.
/// </summary>
[CollectionDefinition("WebApi")]
public class WebApiTestCollection : ICollectionFixture<WebApiTestFixture>
{
    // This class is never instantiated. It's just a marker for xUnit.
}
