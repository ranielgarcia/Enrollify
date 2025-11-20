---
title: US-030 Assessment Clearance & Enrollment Blocks
id: US-030
epic: Enrollment & Finance
priority: high
create_by: Copilot
created_at: 2025-11-16T12:52:00Z
updated_at: 2025-11-16T12:52:00Z
---

## Summary

Block enrollment when a prior-term balance exists unless Assessment marks the student as cleared to enroll.

## Persona(s)

- Admissions Officer (blocked), Assessment/Cashier (clearance), Registrar (override)

## User Story

As an Admissions Officer, I want to see whether a student is cleared by Assessment, so I know if I can proceed with enrollment despite prior balances.

## Acceptance Criteria

1. Enrollment create validates balance; if pending, require clearance flag.
2. Assessment staff can toggle "Cleared to enroll" with timestamp and reason.

## Related Requirements / Source

- Requirements item 7; Phase 3/2 cross-module rule.
