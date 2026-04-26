# Enrollify DTO Usage Review Prompt

Act as a senior .NET architect with deep expertise in Domain-Driven
Design (DDD), Clean Architecture, and MediatR.

## Solution Structure

-   Enrollify.Application\
-   Enrollify.Core\
-   Enrollify.Infrastructure\
-   Enrollify.DatabaseMigration\
-   Enrollify.SharedKernel\
-   Enrollify.WebAPI

## Context

-   `Enrollify.Core` contains domain models, aggregates, value objects,
    and domain logic following DDD principles.
-   `Enrollify.Application` contains use cases implemented via MediatR
    (commands and queries).
-   `Enrollify.WebAPI` exposes endpoints that call MediatR
    queries/commands and return responses to the client.

## Focus of the Review

### 1. DTO Usage in Application Layer

-   Analyze how DTOs are used in `Enrollify.Application`, especially in
    MediatR query handlers.
-   Determine whether DTO usage is excessive, redundant, or justified.
-   Identify cases where DTOs duplicate domain models or create
    unnecessary mapping layers.

### 2. Separation of Concerns

-   Validate whether returning DTOs from query handlers (instead of
    domain aggregates) aligns with Clean Architecture principles.
-   Check if domain models are properly protected from external layers
    (e.g., WebAPI).

### 3. Mapping Strategy

-   Evaluate how mapping between domain models and DTOs is implemented
    (manual mapping, AutoMapper, etc.).
-   Identify inefficiencies, over-abstraction, or unnecessary
    transformations.

### 4. CQRS and Query Design

-   Assess whether using DTOs per query (read models) is appropriate or
    over-engineered.
-   Suggest when it is better to:
    -   Return DTOs
    -   Return domain models
    -   Use projection (e.g., LINQ select) directly to DTOs

### 5. Performance and Maintainability

-   Identify potential performance issues caused by excessive mapping or
    over-fetching data.
-   Evaluate maintainability trade-offs of the current DTO pattern.

### 6. Best Practices and Recommendations

-   Provide concrete recommendations on:
    -   When DTOs are necessary vs unnecessary
    -   How to simplify the current design
    -   Improvements aligned with DDD + Clean Architecture + CQRS

## Output Format

-   Clear findings (issues / anti-patterns)
-   What is acceptable and should be kept
-   Specific refactoring suggestions with examples (before/after if
    possible)
-   Final verdict: Is the DTO usage appropriate, overused, or underused?

## Output File

- Create a markdown file and put your analysis in that file

If possible, reference specific files or patterns in the codebase to
support the analysis.
