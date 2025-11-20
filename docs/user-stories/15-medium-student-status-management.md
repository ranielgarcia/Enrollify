---
title: US-015 Student Status Lookup Management
id: US-015
epic: Master Data Management
priority: medium
create_by: Copilot
created_at: 2025-11-16T12:19:00Z
updated_at: 2025-11-16T12:19:00Z
---

## Summary

Admins manage the set of allowed student statuses (Active, LOA, etc.).

## Persona(s)

- Registrar / System Administrator

## User Story

As a Registrar, I want to manage student status codes, so we can categorize students consistently across the system.

## Acceptance Criteria

1. Given unique Code and Name When I create Then it saves (201).
2. Given blank Code/Name When I create Then 400 per checks.
3. Given status in use When I archive Then allowed (IsActive=0); hard delete blocked.

## API / Back-end Notes

- Endpoints: /api/lookups/student-statuses
- DB: StudentStatuses(Id, Code UNIQUE, Name, Description, DisplayOrder, audit, IsActive, checks)

## UI Notes

- Manage list of statuses; toggle active; change display order.

## Related Requirements / Source

- SQL: StudentStatuses table and seed data.
