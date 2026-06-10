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

Create the new `/scheduling/sections` route with all supporting files, backend API endpoints (enhanced paginated list, aggregate stats, lazy offering details, bulk operations), and the frontend API collection layer.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want a new dedicated page route and backend API infrastructure so that the sections management page v2 can be built independently without modifying the existing page.

## Acceptance Criteria

1. Given a user navigates to `/scheduling/sections` When the route loads Then a new page scaffold renders with `useSuspenseQuery` for sections and stats.
2. Given the paginated endpoint `GET /api/class-sections/filter/{page}/{pageSize}` When called Then it returns sections with `validationSummary` per section (aggregate counts only, no full offering data).
3. Given the stats endpoint `GET /api/class-sections/stats?academicYearId={id}` When called Then it returns full-dataset aggregate counts (`totalDraft`, `totalOpen`, `totalCancelled`, `sectionsWithUnresolvedErrors`, `sectionsWithConflicts`, `unscheduledCount`) unaffected by filters.
4. Given the offering details endpoint `GET /api/class-sections/{sectionId}/offerings` When called Then it returns full offering data with `validationMessages[]` and `conflicts[]`.
5. Given the bulk endpoint `POST /api/class-sections/bulk/open` When called with `{ sectionIds: number[] }` Then it validates Draft eligibility and returns `{ succeeded, failed, errors[] }`.
6. Given the bulk endpoint `POST /api/class-sections/bulk/cancel` When called Then it performs soft-delete cascade and returns success/failure counts.
7. Given the bulk endpoint `POST /api/class-sections/bulk/assign-adviser` When called Then it assigns the adviser to all Draft sections.

## Definition of Done

- [ ] New TanStack Router file at `src/routes/scheduling.sections.tsx`
- [ ] `searchParams.ts` with `view` (card/table) param
- [ ] All backend endpoints implemented and returning correct shapes
- [ ] Frontend API collection (`class-section-collection-v2.ts`) with query keys and mutation options
- [ ] API types regenerated via `npm run generate:api:win`
- [ ] Page scaffold renders with loading state

## Preconditions & Assumptions

- Backend is running at `https://localhost:7107` for API type generation.
- Existing `/sections` page remains untouched.

## Business Rules / Validation

- `validationSummary` contains pre-computed aggregates only — no `validationMessages[]` or `conflicts[]` in paginated response.
- Stats endpoint always returns full-dataset numbers, independent of pagination or filters.
- Offering details endpoint returns conflicts only for **Draft** sections (Open sections finalized — no conflict computation).
- Bulk open validates all sections are Draft before processing.

## API / Back-end Notes

**Enhanced Paginated Endpoint:**
`GET /api/class-sections/filter/{page}/{pageSize}`
Response: `PagedResult<ClassSectionDto>` with `validationSummary` per section:
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

**Aggregate Stats Endpoint:**
`GET /api/class-sections/stats?academicYearId={id}`
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

**Bulk Endpoints:**
- `POST /api/class-sections/bulk/assign-adviser` — body: `{ sectionIds: number[], adviserId: number }`
- `POST /api/class-sections/bulk/open` — body: `{ sectionIds: number[] }`
- `POST /api/class-sections/bulk/cancel` — body: `{ sectionIds: number[] }`

All return `{ succeeded: number, failed: number, errors: { sectionId, message }[] }`.

## UI Notes

- New route does not modify existing `/sections` page.
- Page scaffold loads sections + stats via TanStack Query.

## Edge Cases & Error Handling

- Empty academic year → 400 Bad Request.
- Invalid section IDs in bulk → 404 for missing, 400 for non-Draft on open.
- Offering details for non-Draft sections returns empty `conflicts[]`.

## Test Cases

1. GET paginated endpoint returns `validationSummary` per section.
2. GET stats endpoint returns correct aggregate counts.
3. GET offering details returns full messages + conflicts for Draft sections.
4. POST bulk open with mixed Draft/non-Draft returns failure for non-Draft.
5. POST bulk cancel performs soft-delete cascade.

## Dependencies

- Existing `ClassSection`, `Offering`, `Schedule` entities.

## Related Requirements / Source

- PRD-03 Technical Design §4 (API Endpoints)
- PRD-03 Technical Design §5 (Data Flow)
- PRD-04 Implementation Phases (Phase 1)

## Notes / Implementation Considerations

- Use `FilterExpressionBuilder` and `SpecSortBuilder<T>` for paginated query.
- Vogen IDs cast to `object?` in LINQ expressions.
- Stats query uses separate handler (`GetClassSectionStatsQuery`) — no filter/pagination params.
- Offering details lazy-loaded and cached per `sectionId` on frontend.
