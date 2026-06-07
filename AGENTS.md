# Enrollify Copilot Instructions

Enrollify is a university enrollment management system with two main sub-systems: a **React/TypeScript frontend** and a **.NET backend**. There is also a **Genetic Algorithm** POC for course scheduling under `src/POCs/`.

---

## Repository Layout

```
src/
  system/
    EnrollifyBackend/                # .NET 10 Web API (Clean Architecture + DDD)
      Enrollify.Core/                # Domain layer
      Enrollify.Application/         # Use cases (Mediator commands/queries)
      Enrollify.Infrastructure/      # Data access (EF Core, Dapper)
      Enrollify.WebAPI/              # FastEndpoints API
      Enrollify.SharedKernel/        # Shared abstractions
      Enrollify.DatabaseMigration/   # DbUp migration runner
      Enrollify.UnitTests/           # Unit tests
      Enrollify.IntegrationTests/    # Integration tests (Testcontainers)
    enrollify-frontend/              # React 19 + Vite + TanStack Router SPA
  POCs/
    Enrollify.CleanArchPOC/          # Aspire-hosted Clean Architecture reference
    Genetic-algorithm/               # GA scheduling solver
```

---

## Frontend

**Root:** `src/system/enrollify-frontend/`

### Commands

```bash
# Dev server
npm run dev

# Type-check (use this instead of build to validate TypeScript)
npm run test:ts

# Lint
npm run lint

# Build
npm run build

# Regenerate API types from running backend (Windows)
npm run generate:api:win

# Regenerate API types from running backend (Linux/Mac)
npm run generate:api:lin
```

> The backend must be running at `https://localhost:7107` when regenerating API types.

### Architecture

- **Routing:** TanStack Router with file-based routes under `src/routes/`. `routeTree.gen.ts` is auto-generated — never edit it manually.
- **Data fetching:** TanStack Query (`useSuspenseQuery` preferred). All queries/mutations go through `createAppQueryOptions` / `createMutationOptions` in `src/hooks/`.
- **API types:** `src/api/generated/api.ts` is generated from the backend's OpenAPI schema. Types are fully typed end-to-end; do not write manual API types.
- **State management:** URL-synced table state via `nuqs` (`useQueryStates`). Component-local CRUD state via `useCrudState<T>` (`src/hooks/use-crud-state.ts`).
- **Forms:** TanStack Form with Zod schemas. Form models live in `src/api/models/`. All models extend `AuditInfoSchema`.
- **Styling:** Tailwind CSS v4 + shadcn/ui components. Icons are centralized in `src/config/module-icons.ts` (Lucide icons) — import from there, not directly from `lucide-react` for module-level icons.
- **Auth:** Azure AD via MSAL (`@azure/msal-react`). Authorization is policy-based — use `useAuthorization` hook and `PolicyNames` from `src/infrastructure/authorization/models/PolicyNames.ts`.

### Management Page Pattern

The canonical pattern for CRUD pages is `src/page-components/subjects-management-page/`. Every management page has:

| File                        | Purpose                                                                            |
| --------------------------- | ---------------------------------------------------------------------------------- |
| `index.tsx`                 | Page root; `useSuspenseQuery` + `useCrudState<T>` + `useQueryStates(searchParams)` |
| `searchParams.ts`           | `nuqs` parsers for `page`, `perPage`, `filters`, `sort`, `joinOperator`            |
| `*-table.tsx`               | TanStack Table + `useDataTable` hook                                               |
| `*-form-drawer.tsx`         | TanStack Form inside a `vaul` Drawer                                               |
| `delete-*-alert-dialog.tsx` | Confirm-delete dialog                                                              |

The `src/api/collections/` file for each entity defines `queryKeys`, `createQueryOptions`/`createMutationOptions` wrappers, and Zod `.parse()` in the `select` callback.

**Use the `enrollify-management-page` skill** (`.github/skills/enrollify-management-page/SKILL.md`) when implementing any new management page — it encodes the full phase-by-phase process.

**Use the `enrollify-ui-redesign` skill** (`.github/skills/enrollify-ui-redesign/SKILL.md`) when redesigning or improving any frontend component's visual design.

---

## Backend

**Root:** `src/system/EnrollifyBackend/`  
**Solution file:** `EnrollifyBackend.slnx`

### Commands

```bash
# Run the API
dotnet run --project Enrollify.WebAPI

# Build
dotnet build

# Run all tests
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedName~YourTestMethodName"

# Add EF migration (from Infrastructure dir)
dotnet ef migrations add <MigrationName> --startup-project ..\Enrollify.DatabaseMigration\Enrollify.DatabaseMigration.csproj

# Apply migrations
dotnet ef database update --startup-project ..\Enrollify.DatabaseMigration\Enrollify.DatabaseMigration.csproj
```

### Architecture

Clean Architecture with DDD, using [Ardalis patterns](https://github.com/ardalis/CleanArchitecture):

| Project                    | Role                                                                          |
| -------------------------- | ----------------------------------------------------------------------------- |
| `Enrollify.Core`           | Domain — aggregates, value objects, domain events, interfaces                 |
| `Enrollify.Application`    | Use cases — Mediator commands/queries, DTOs, specs, `FilterExpressionBuilder` |
| `Enrollify.Infrastructure` | EF Core (`EnrollifyDbContext`), Dapper queries, repositories, Azure Blob      |
| `Enrollify.WebAPI`         | FastEndpoints endpoints, authorization policies, request/response models      |
| `Enrollify.SharedKernel`   | `EntityBase`, `ValueObject`, `IAggregateRoot`, repository interfaces          |

**CQRS via [Mediator](https://github.com/martinothamar/Mediator):** All use cases are `ICommand<T>` / `IQuery<T>` records handled by inner `Handler` classes within the same static class (e.g., `CreateSubject.Command` + `CreateSubject.Handler`).

**Endpoints via [FastEndpoints](https://fast-endpoints.com/):** Each endpoint is a class inheriting `Endpoint<TRequest, TResponse>`. Endpoints are grouped with `[Group<TGroup>]`. Validators are co-located in the same file as `Validator<TRequest>`.

**Results via [Ardalis.Result](https://github.com/ardalis/Result):** Application handlers return `Result<T>` / `Result`. The WebAPI layer maps results to HTTP responses with `.ToCreatedResult()`, `.ToOkResult()`, etc.

**Value Objects via [Vogen](https://github.com/SteveDunn/Vogen):** Entity IDs (e.g., `SubjectId`, `SubjectCode`) are strongly-typed Vogen structs. When used in EF Core specs, cast explicitly: `s => (string)s.Code` or `s => (object?)s.Id`.

**Soft delete:** All entities implementing `IAuditable` support soft-delete via `DeletedAt` / `DeletedBy` properties.

### Filtering & Sorting in Specs

Use `FilterExpressionBuilder` (static) and `SpecSortBuilder<T>` from `Enrollify.Application/Filtering/` for paginated specs:

```csharp
// String filter
FilterExpressionBuilder.ForString<Entity>(e => e.Name, filterItem)

// Numeric filter (parse value first)
FilterExpressionBuilder.ForNumeric<Entity, decimal>(e => e.Units, parsedDecimal, filterItem)

// Combine with AND/OR
var combined = FilterExpressionBuilder.Combine(expressions, joinOperator);
if (combined is not null) Query.Where(combined);

// Sorting
_sortBuilder.Apply(Query, sortList[0], isFirst: true, SortMap);
```

The `SortMap` dictionary maps lowercased field names to `Expression<Func<T, object?>>`. For Vogen types, cast to `object?`: `e => (object?)e.Code`.

### Authorization

Policies are declared in `Enrollify.WebAPI/Authorization/PolicyName.cs` and registered in `AuthorizationPolicyRegistrations.cs`. Endpoints are decorated with `[Authorize(Policy = PolicyName.HasCreate*Permission)]`.

The permission system uses `Ardalis.SmartFlag` enum (`PermissionEnum`) stored as bit-flags per role.

### Testing

#### Unit Tests

**Project:** `Enrollify.UnitTests`

- Test naming: `MethodName_Condition_ExpectedResult()`
- Test structure: Setup → Execution → Verification (no comment labels needed in simple tests)
- Tests use `[Fact(DisplayName = "...")]` for descriptive scenarios
- `FeaturesTestsFixture` provides a shared service provider for integration-style application tests

#### Integration Tests

**Project:** `Enrollify.IntegrationTests`  
**Root:** `src/system/EnrollifyBackend/Enrollify.IntegrationTests/`  
**README:** `Enrollify.IntegrationTests/README.md` — **Read this first when writing integration tests**

##### Commands

```bash
# Run all integration tests
dotnet test Enrollify.IntegrationTests

# Run WebAPI collection tests
dotnet test --filter "WebApi"

# Run Application collection tests
dotnet test --filter "Application"

# Run specific test class
dotnet test --filter "RoomTypeEndpointsTests"

# Run single test method
dotnet test --filter "FullyQualifiedName~CreateRoomType_ValidRequest_ReturnsCreated"
```

##### Architecture

**Testcontainers:** MsSQL + Azurite containers (shared across test run, singleton pattern)  
**Database strategy:** Each test class gets its own database with full migrations (schema + seed + mock data)  
**Test fixtures:**

| Fixture                  | Purpose                                                                 | Collection Attribute          |
| ------------------------ | ----------------------------------------------------------------------- | ----------------------------- |
| `WebApiTestFixture`      | HTTP endpoint testing via `WebApplicationFactory<Program>` + TestServer | `[Collection("WebApi")]`      |
| `ApplicationTestFixture` | Direct Mediator testing (commands/queries) without HTTP overhead        | `[Collection("Application")]` |

**Key files:**

- `Infrastructure/TestContainersManager.cs` — Singleton container manager with database migration
- `Infrastructure/WebApiTestFixture.cs` — WebAPI test fixture with HttpClient and DbContext access
- `Infrastructure/ApplicationTestFixture.cs` — Application layer fixture with Mediator and DbContext access
- `Helpers/TestDataBuilder.cs` — Bogus-based test data generation (simple entities only)
- `Helpers/HttpClientExtensions.cs` — HTTP convenience methods (`PostAsJsonAsync<TReq, TRes>`, etc.)
- `Helpers/DatabaseHelper.cs` — Database operations (seed, transaction rollback, detach)
- `_Tests/WebApi/_SampleWebApiTests.cs` — WebAPI test examples
- `_Tests/Application/_SampleApplicationTests.cs` — Application test examples

##### Test Structure

```
_Tests/
  WebApi/                          # HTTP endpoint tests
    {Feature}EndpointsTests.cs
  Application/                     # Command/Query tests
    {Feature}/
      {CommandOrQuery}Tests.cs
```

##### Conventions

- **Naming:** `{Method}_{Scenario}_{ExpectedResult}` (e.g., `CreateRoomType_ValidData_ReturnsSuccess`)
- **Display names:** `[Fact(DisplayName = "Create room type with valid data succeeds")]`
- **AAA pattern:** Arrange → Act → Assert with comment labels
- **Unique data:** Use `TestDataBuilder.GenerateUniqueString("Prefix")` to avoid conflicts between tests in the same class
- **Test isolation:** Each test class gets its own database; tests within a class share the database—use unique data, transaction rollback, or explicit cleanup
- **Assertions:** Verify `Result<T>` status with `Assert.True(result.IsSuccess)`, check database state when needed

##### Choose WebAPI vs Application Test

**WebAPI test when:**

- Testing HTTP request/response cycle, routing, status codes, serialization
- Testing authentication/authorization at HTTP level
- End-to-end API surface testing

**Application test when:**

- Testing command/query handlers directly
- Testing business logic without HTTP overhead
- Faster execution (no HTTP layer)

**Default:** Prefer Application tests unless HTTP concerns must be tested.

##### Test Data Strategies

1. **Unique data (recommended):** `TestDataBuilder.GenerateUniqueString("RoomType")` — no cleanup needed
2. **Transaction rollback:** `DatabaseHelper.ExecuteInTransactionAsync(db, async () => { /* test */ })` — automatic rollback
3. **Explicit cleanup:** `try { /* test */ } finally { /* cleanup */ }` — use sparingly

**Use the `enrollify-integration-tests` skill** (`.github/skills/enrollify-integration-tests/SKILL.md`) when writing any integration test — it provides phase-by-phase templates, examples, and best practices for both WebAPI and Application layer tests.

---

## Key Cross-Cutting Concerns

- **EF Core + Vogen:** Vogen value objects need explicit operators in LINQ to satisfy EF Core's expression translator. Always cast ID/value-object properties when used in `.Where()` / `.OrderBy()`.
- **API type regeneration:** After any backend schema change, run `npm run generate:api:win` from the frontend directory before updating frontend code.
- **URL-synced filters:** Table filter/sort/pagination state is stored in the URL via `nuqs`. The `searchParams.ts` file per page defines the parsers. Use `useQueryStates(searchParams)` to read them.
- **Query invalidation:** Mutations specify `meta: { invalidateQueries: [queryKeys.base()] }` — the query client middleware handles automatic invalidation after successful mutations.
- **DB connection:** The backend resolves the connection string in priority order: `cleanarchitecture` (Aspire) → `DefaultConnection` (SQL Server) → `SqliteConnection` (fallback).

---

## Instruction Files

Additional scoped instruction files exist in `.github/`:

- `.github/dotnet-architecture-good-practices.instructions.md` — applies to all `*.cs` files; enforces DDD + SOLID analysis process before implementation
- `.github/genetic-algo-instructions.md` — applies to `src/POCs/Genetic-algorithm/**`; GA tuning and data-model conventions

## Agent Skills

Project-level Copilot skills for complex, multi-step workflows:

- `.github/skills/enrollify-management-page/SKILL.md` — Step-by-step guide for implementing CRUD management pages in the React frontend (model schema, API collection, table, form drawer, delete dialog)
- `.github/skills/enrollify-ui-redesign/SKILL.md` — Design language guide for redesigning frontend components (hero-card pattern, icon chips, status badges, form sections, empty states)
- `.github/skills/enrollify-integration-tests/SKILL.md` — Comprehensive guide for creating integration tests (WebAPI tests via TestServer, Application tests via Mediator, test data strategies, fixtures, helpers)
- `.github/skills/enrollify-resize-dialog/SKILL.md` — Quick reference for resizing Dialog components (width classes, height control, scrollable body, when to switch from Dialog to Drawer)
- `.github/skills/enrollify-resize-drawer/SKILL.md` — Step-by-step guide for resizing right-side Drawer components (vaul data-attribute override pattern, half-screen, near-full, full-screen widths)
- `.github/skills/grill-me/SKILL.md` — Interview the user relentlessly to stress-test a plan, research, or design until shared understanding is reached
