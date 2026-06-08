# Admin Conflict Dashboard

**Feature Type:** API + Frontend  
**Research Reference:** `docs\scheduling-architecture\research-document-base\phase-3-soft-conflicts-implementation.md:169-253`  
**Phase:** 4  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A dedicated admin dashboard page showing all conflicts across an academic term. Provides a bird's-eye view of scheduling health.

## API Endpoints

### `GET /api/scheduling/conflicts?termId={id}`

**Response:**
```json
{
  "term": { "id": 1, "name": "1st Term SY 2025-2026" },
  "summary": {
    "totalConflicts": 42,
    "errorCount": 15,
    "warningCount": 27,
    "byType": {
      "TEACHER_DOUBLE_BOOKED": 8,
      "ROOM_DOUBLE_BOOKED": 3,
      "SECTION_OVERLAP": 12,
      "DUPLICATE_SUBJECT": 4
    }
  },
  "conflicts": [ /* ConflictResultDto[] */ ]
}
```

## Dashboard Features

- Summary cards: Total conflicts, by type, by severity
- Filters: Academic term, conflict type, severity, teacher, room, section
- Sortable table with export to CSV
- Drill-down to specific section detail page

## Frontend Components Needed

- Conflict summary cards (total, errors, warnings, by type)
- Filterable/sortable conflict table
- Export to CSV/Excel
- Drill-down navigation to section detail

## Implementation Steps

1. Create `GetConflictsByTermQuery` in Application layer
2. Create `GetConflictsByTermEndpoint` in WebAPI
3. Add authorization: `HasViewSchedulingConflictsPermission`
4. Build frontend page under `/class-sections/conflicts`

## Authorization

New permission required: `HasViewSchedulingConflictsPermission`
