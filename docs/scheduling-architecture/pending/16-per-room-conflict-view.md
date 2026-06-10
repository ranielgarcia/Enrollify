# Per-Room Conflict View

**Feature Type:** API + Frontend  
**Research Reference:** `docs\scheduling-architecture\research-document-base\phase-3-soft-conflicts-implementation.md:213-221`  
**Phase:** 4  
**Priority:** LOW  
**Status:** 📅 Not Implemented

---

## Description

A room-specific page showing all conflicts affecting a particular room, with a booking timeline and utilization summary.

## API Endpoint

### `GET /api/scheduling/rooms/{id}/conflicts?termId={id}`

## Page Features

- Room card: Building, capacity, type, utilization %
- Weekly grid: Booking timeline with conflict highlights
- Conflict list: All conflicts for this room
- Room capacity vs expected students info

## Frontend Components Needed

- Room info card with capacity and utilization
- Booking timeline grid
- Conflict list with severity badges
- Capacity comparison visualization

## Implementation Steps

1. Create query endpoint
2. Build frontend page
3. Integrate with room selection/filtering
