---
title: US-SM-10 Integration Tests for New Endpoints
id: US-SM-10
epic: Section Manage Page Redesign
priority: medium
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Write integration tests covering all new backend endpoints: college-scoped stats, college-filtered list, lazy offering details, and bulk operations (open, cancel, assign adviser). Both WebAPI collection (HTTP) and Application collection (Mediator) tests.

## Persona(s)

- Developer / QA Engineer

## User Story

As a Developer, I want integration tests for all new class-sections endpoints, so that regressions are caught during development and the API behavior is documented through tests.

## Acceptance Criteria

1. Given the college-scoped stats endpoint When called via HTTP Then it returns correct selected-college counts independent of filter params.
2. Given the offering details endpoint When called for a Draft section Then it returns full offering data with validation messages and conflicts.
3. Given the offering details endpoint When called for an Open section Then it returns offerings with empty `conflicts[]`.
4. Given the bulk open endpoint When called with valid Draft section IDs and no Error-severity conflicts Then all sections transition to Open and response shows 100% succeeded.
5. Given the bulk open endpoint When called with a mix of Draft and non-Draft sections Then Draft sections succeed, non-Draft fail, and response shows partial success.
6. Given the bulk cancel endpoint When called Then all sections are cancelled and assignments are freed.
7. Given the bulk assign-adviser endpoint When called Then the adviser is assigned to all specified sections.
8. Given Draft → Open When Error-severity conflicts exist Then opening is blocked and returns Invalid.

## Definition of Done

- [ ] WebAPI tests for aggregate stats endpoint
- [ ] WebAPI tests for offering details endpoint
- [ ] WebAPI tests for bulk operation endpoints
- [ ] Application tests for aggregate stats query handler
- [ ] Application tests for offering details query handler
- [ ] Application tests for bulk operation command handlers
- [ ] Tests use `TestDataBuilder` for unique test data
- [ ] Tests follow naming convention: `{Method}_{Scenario}_{ExpectedResult}`

## Preconditions & Assumptions

- Integration test infrastructure already exists (Testcontainers, fixtures).
- `WebApiTestFixture` and `ApplicationTestFixture` available.

## Business Rules / Validation

- Same business rules as production: Draft → Open validates eligibility; conflicts/errors block opening.
- Bulk endpoints validate status before processing.

## API / Back-end Notes

- Test endpoints:
  - `GET /api/colleges/{collegeId}/class-sections/stats?academicYearId={id}`
  - `GET /api/colleges/{collegeId}/class-sections?academicYearId={id}`
  - `GET /api/class-sections/{sectionId}/offerings`
  - `POST /api/class-sections/bulk/assign-adviser`
  - `POST /api/class-sections/bulk/open`
  - `POST /api/class-sections/bulk/cancel`

## Test Cases

1. `GetStats_ValidRequest_ReturnsCorrectCounts` — verify stats endpoint returns selected-college numbers.
2. `GetOfferingDetails_DraftSection_ReturnsFullData` — verify offering details include validation + conflicts.
3. `GetOfferingDetails_OpenSection_ReturnsEmptyConflicts` — verify Open sections have no conflicts computed.
4. `BulkOpen_AllDraft_AllSucceed` — verify all sections transition to Open.
5. `BulkOpen_MixedStatus_PartialSuccess` — verify non-Draft sections fail with appropriate message.
6. `BulkCancel_ValidRequest_FreesAssignments` — verify cancelled sections do not participate in conflict detection and resources are freed.
7. `BulkAssignAdviser_ValidRequest_AdviserAssigned` — verify adviser assigned to all sections.
8. `Open_ConflictsExist_ReturnsInvalid` — verify Error-severity conflicts block Draft → Open.

## Dependencies

- US-SM-01 (backend endpoints must be implemented)
- Existing integration test infrastructure (WebApiTestFixture, ApplicationTestFixture)

## Related Requirements / Source

- PRD-04 Implementation Phases (Phase 7)

## Notes / Implementation Considerations

- Prefer Application tests (Mediator) for business logic; WebAPI tests for HTTP concerns.
- Use `DatabaseHelper.ExecuteInTransactionAsync` or unique test data for isolation.
- Follow patterns from `_SampleWebApiTests.cs` and `_SampleApplicationTests.cs`.
- Use `TestDataBuilder.GenerateUniqueString` for test data to avoid conflicts.
