# Per-Teacher Conflict View

**Feature Type:** API + Frontend  
**Research Reference:** `docs\scheduling-architecture\research-document-base\phase-3-soft-conflicts-implementation.md:202-211`  
**Phase:** 4  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A teacher-specific page showing all conflicts affecting a particular teacher, with a weekly grid visualization and workload summary.

## API Endpoint

### `GET /api/scheduling/teachers/{id}/conflicts?termId={id}`

## Page Features

- Teacher card: Photo, name, total hours, overload status
- Weekly grid: Visual timeline (07:00-21:00 × MON-SAT) with conflict highlights
- Conflict list: All conflicts affecting this teacher
- Workload summary: Scheduled hours vs max load

## Frontend Components Needed

- Teacher info card
- Weekly grid component (7×14 time grid with slot rendering)
- Conflict badge components (color-coded by severity)
- Workload progress bar

## Implementation Steps

1. Create query endpoint
2. Build frontend page
3. Integrate with teacher selection/filtering
