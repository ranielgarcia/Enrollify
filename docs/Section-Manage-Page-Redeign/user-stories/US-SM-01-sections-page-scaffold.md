---
title: US-SM-01 Sections Page Scaffold & Data Layer
id: US-SM-01
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Create the new `/scheduling/sections` route with all supporting files, backend API endpoints (college-filtered full list, college-scoped stats, lazy offering details, bulk operations), and the frontend API collection layer.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want a new dedicated page route with frontend infrastructure and backend API endpoints so that the sections management page v2 can be built independently without modifying the existing page.

---

# Phase 1: Frontend Page Scaffold

## Acceptance Criteria (Phase 1)

1. Given a user navigates to `/scheduling/sections` When the route loads Then a new TanStack Router page scaffold renders.
2. Given the page loads When it queries for sections and stats Then `useSuspenseQuery` initiates requests using mock/stub API collection.
3. Given the page loads When search params are present (`collegeId`, `view`, `filters`, `sort`) Then the page reflects those params in the UI state.
4. Given the page renders Then it displays a loading skeleton until sections + stats queries resolve.
5. Given a college ID is selected in the UI When the local state updates Then the `collegeId` param syncs to the URL via `nuqs`.

## Definition of Done (Phase 1)

- [ ] New TanStack Router file at `src/routes/curriculum-and-scheduling/scheduling.sections.tsx` with `useSuspenseQuery` setup
- [ ] `src/routes/curriculum-and-scheduling/scheduling.sections.search-params.ts` with `collegeId`, `view` (card/table), `filters`, `sort` parsers
- [ ] Frontend API collection stub (`src/api/collections/class-section-collection-v2.ts`) with:
  - Query keys for sections list, stats, and offering details
  - Mock query options returning dummy data matching expected DTO shapes
  - Mutation options for bulk operations (open, cancel, assign-adviser) with stub handlers
- [ ] Page scaffold renders sections table/card with mock data
- [ ] Page scaffold renders stats panel with mock aggregate counts
- [ ] Loading state (skeleton) visible while queries resolve
- [ ] College selector syncs `collegeId` to URL params
- [ ] TypeScript strict mode passes (no `any` types for sections/stats)

## UI Notes (Phase 1)

- New route does not modify existing `/sections` page.
- Page scaffold uses `useSuspenseQuery` for sections and stats queries.
- Mock data is realistic enough to visualize the UI layout.
- Search params are synced via `nuqs` (no manual state management).

## Test Cases (Phase 1)

1. Route `/scheduling/sections` renders the page scaffold without errors.
2. Page displays loading skeleton while queries are pending.
3. College selector updates URL param when selection changes.
4. Search params (collegeId, view, filters, sort) persist across navigation back to the page.
5. Mock sections list displays validationSummary fields (totalOfferings, offeringsWithErrors, etc.).
6. Mock stats panel displays all aggregate fields (totalDraft, totalOpen, etc.).

## Edge Cases & Error Handling (Phase 1)

- No college selected initially → page displays "Select a college" placeholder.
- Missing search params → defaults applied (e.g., first college, table view).

---

# Phase 2: Backend Implementation

## Acceptance Criteria (Phase 2)

1. Given a college is selected When the list endpoint `GET /api/scheduling/colleges/{collegeId}/class-sections?academicYearId={id}` is called Then it returns the full list of sections for that college with `validationSummary` per section (aggregate counts only, no nested validation/conflict arrays).
2. Given a college is selected When the stats endpoint `GET /api/scheduling/colleges/{collegeId}/class-sections/stats?academicYearId={id}` is called Then it returns selected-college aggregate counts (`totalDraft`, `totalOpen`, `totalCancelled`, `sectionsWithUnresolvedErrors`, `sectionsWithConflicts`, `unscheduledCount`) unaffected by filters.
3. Given a section ID When the offering details endpoint `GET /api/class-sections/{sectionId}/offerings` is called Then it returns offering schedules and conflict details; conflicts are computed for Draft sections only.
4. Given the single-section Draft → Open mutation is called Then it validates enrollment eligibility **and** blocks on **Error-severity** conflicts.
5. Given the bulk endpoint `POST /api/scheduling/colleges/class-sections/bulk/open` is called with `{ sectionIds: number[] }` Then it validates Draft eligibility **and** blocks on Error-severity conflicts and returns `{ succeeded, failed, errors[] }`.
6. Given the bulk endpoint `POST /api/scheduling/colleges/class-sections/bulk/cancel` is called Then it frees assignments (teacher/room/schedules) and returns success/failure counts.
7. Given the bulk endpoint `POST /api/scheduling/colleges/class-sections/bulk/assign-adviser` is called Then it assigns the adviser to all Draft sections.

## Definition of Done (Phase 2)

- [x] FastEndpoints endpoint for college-filtered list: `GET /api/scheduling/colleges/{collegeId}/class-sections?academicYearId={id}`
- [x] FastEndpoints endpoint for college-scoped stats: `GET /api/scheduling/colleges/{collegeId}/class-sections/stats?academicYearId={id}`
- [x] FastEndpoints endpoint for offering details: `GET /api/class-sections/{sectionId}/offerings`
- [x] FastEndpoints endpoint for bulk open: `POST /api/scheduling/colleges/class-sections/bulk/open`
- [x] FastEndpoints endpoint for bulk cancel: `POST /api/scheduling/colleges/class-sections/bulk/cancel`
- [ ] FastEndpoints endpoint for bulk assign-adviser: `POST /api/scheduling/colleges/class-sections/bulk/assign-adviser`
- [ ] Mediator command/query handlers for each operation
- [ ] Business logic validates Draft eligibility and Error-severity conflicts
- [ ] Database queries correctly compute `validationSummary` and stats
- [ ] All responses return correct DTO shapes (with `validationSummary`, `succeeded`/`failed`/`errors`, etc.)
- [ ] Authorization policies applied to all endpoints
- [ ] Integration tests verify each endpoint behavior
- [ ] API types regenerated via `npm run generate:api:win` and frontend collection updated to use real endpoints

## API / Backend Notes (Phase 2)

**College-Filtered List Endpoint:**
`GET /api/scheduling/colleges/{collegeId}/class-sections?academicYearId={id}`

Response: list of `ClassSectionDto` with `validationSummary` per section:
```json
{
  "validationSummary": {
    "totalOfferings": 2,
    "offeringsWithErrors": 1,
    "offeringsWithConflicts": 1,
    "missingTeacherCount": 1,
    "missingRoomCount": 0,
    "missingScheduleCount": 1
  }
}
```

**College-Scoped Stats Endpoint:**
`GET /api/scheduling/colleges/{collegeId}/class-sections/stats?academicYearId={id}`

```json
{
  "totalDraft": 24,
  "totalOpen": 156,
  "totalCancelled": 8,
  "sectionsWithUnresolvedErrors": 12,
  "sectionsWithConflicts": 7,
  "unscheduledCount": 31
}
```

**Offering Details Endpoint:**
`GET /api/class-sections/{sectionId}/offerings`

Returns offering schedules, teacher assignments, room assignments, and conflict details (conflicts computed for Draft sections only).

**Bulk Endpoints:**
- `POST /api/scheduling/colleges/class-sections/bulk/assign-adviser` — body: `{ sectionIds: number[], adviserId: number }`
- `POST /api/scheduling/colleges/class-sections/bulk/open` — body: `{ sectionIds: number[] }`
- `POST /api/scheduling/colleges/class-sections/bulk/cancel` — body: `{ sectionIds: number[] }`

All bulk endpoints return:
```json
{
  "succeeded": 18,
  "failed": 2,
  "errors": [
    { "sectionId": 42, "message": "Section has unresolved conflicts" },
    { "sectionId": 99, "message": "Invalid college for bulk operation" }
  ]
}
```

## Business Rules / Validation (Phase 2)

- The college-filtered list response contains pre-computed aggregates only — no nested validation/conflict arrays in the list payload.
- Stats endpoint returns selected-college numbers, independent of active filters.
- Draft → Open must validate enrollment eligibility and block when any **Error-severity** conflict exists.
- Offering details endpoint returns conflicts only for **Draft** sections (Open sections finalized — no conflict computation).
- Bulk open validates all sections are Draft before processing.

## Test Cases (Phase 2)

1. GET college-filtered list endpoint returns `validationSummary` per section with correct aggregate counts.
2. GET college-scoped stats endpoint returns correct aggregate counts for selected college.
3. GET offering details returns full messages + conflict details for Draft sections.
4. GET offering details for non-Draft sections returns empty `conflicts[]`.
5. POST bulk open with mixed Draft/non-Draft returns failure for non-Draft sections.
6. POST bulk open with Error-severity conflicts returns failure for affected sections.
7. POST bulk cancel frees all assignments and returns success count.
8. POST bulk assign-adviser assigns adviser to all Draft sections in request.
9. Empty academic year parameter returns 400 Bad Request.
10. Invalid section IDs in bulk endpoints return 404 or appropriate error.

## Edge Cases & Error Handling (Phase 2)

- Empty academic year → 400 Bad Request.
- Invalid section IDs in bulk → 404 for missing, 400 for non-Draft on open.
- Offering details for non-Draft sections returns empty `conflicts[]`.
- College mismatch in bulk operation → 400 Bad Request.
- Adviser not found in assign-adviser → 404 Not Found.

---

## Preconditions & Assumptions

- Backend is running at `https://localhost:7107` for API type generation (Phase 2 only).
- Existing `/sections` page remains untouched.
- Phase 1 uses mock/stub endpoints; Phase 2 replaces them with real implementations.

## Dependencies

- **Phase 1 → Phase 2:** Phase 1 frontend scaffold must be complete and testable with mocks before Phase 2 backend implementation begins.
- Existing `ClassSection`, `Offering`, `Schedule` entities (used in Phase 2).
- Existing authentication/authorization policies (used in Phase 2 endpoints).

## Related Requirements / Source

- PRD-03 Technical Design §4 (API Endpoints)
- PRD-03 Technical Design §5 (Data Flow)
- PRD-04 Implementation Phases (Phase 1)

## Notes / Implementation Considerations

- Vogen IDs cast to `object?` in LINQ expressions (Phase 2 only).
- Stats query uses separate handler (`GetCollegeClassSectionStatsQuery`) — no filter params (Phase 2).
- Offering details lazy-loaded and cached per `sectionId` on frontend (Phase 1 → Phase 2).
- Phase 1 can be tested and merged independently using mock data.
- Phase 2 replaces mock endpoints by updating frontend API collection query options to hit real endpoints.
