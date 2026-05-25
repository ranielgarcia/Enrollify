// Disable parallel execution across test collections to prevent global state pollution.
// FastEndpoints sets ValidatorOptions.Global.PropertyNameResolver to camelCase when
// UseFastEndpoints() is called during WebApiTestFixture initialization. Because this
// is a process-wide static mutation, it would break Application-layer tests that
// expect PascalCase property names from FluentValidation. Running collections
// sequentially ensures each fixture can manage its own expected global state.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
