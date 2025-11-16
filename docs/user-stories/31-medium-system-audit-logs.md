---
title: US-031 System Audit Logs
id: US-031
epic: Platform & Security
priority: medium
create_by: Copilot
created_at: 2025-11-16T12:53:00Z
updated_at: 2025-11-16T12:53:00Z
---

## Summary

Capture system-level audit logs for key events (login, create/update/archive, approvals, grade submissions), searchable by user, date, and entity.

## Persona(s)

- System Administrator, Registrar

## User Story

As a System Admin, I want audit logs for critical actions, so we can investigate issues and meet compliance.

## Acceptance Criteria

1. Log actor, timestamp, action, entity, before/after snapshots where feasible.
2. Export logs to CSV; retention settings configurable.

## Related Requirements / Source

- Phase 5 scope (system audit logs).
