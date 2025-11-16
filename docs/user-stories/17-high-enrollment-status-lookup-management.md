---
title: US-017 Enrollment Status Lookup Management
id: US-017
epic: Enrollment
priority: high
create_by: Copilot
created_at: 2025-11-16T12:28:00Z
updated_at: 2025-11-16T12:28:00Z
---

## Summary

Admins manage the allowed enrollment statuses (Pending, Approved, Enrolled, Completed, Dropped, Failed, Cancelled) used across enrollment workflows.

## Persona(s)

- Registrar / System Administrator

## User Story

As a Registrar, I want to manage enrollment status codes and order, so enrollment processing and reports stay consistent.

## Acceptance Criteria

1. Create status with unique Code and Name; 201 on success.
2. Validate Code/Name non-empty; 400 on invalid (per checks).
3. Archiving a status is allowed; hard delete blocked if referenced.

## API / Back-end Notes

- Endpoints: /api/lookups/enrollment-statuses
- DB: EnrollmentStatuses(Id PK, Code UNIQUE, Name, Description, DisplayOrder, audit, IsActive, validation checks)

## UI Notes

- Manage list; toggle active; set display order.

## Related Requirements / Source

- SQL: EnrollmentStatuses table and seed values.
