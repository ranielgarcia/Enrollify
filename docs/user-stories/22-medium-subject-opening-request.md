---
title: US-022 Subject Opening Request Workflow
id: US-022
epic: Enrollment
priority: medium
create_by: Copilot
created_at: 2025-11-16T12:40:00Z
updated_at: 2025-11-16T12:40:00Z
---

## Summary

When a needed subject isn’t offered this term, an irregular student (via admissions) can request opening a one-off offering; request is reviewed by Registrar/Dean and may include extra fee.

## Persona(s)

- Admissions Officer (submit request)
- Registrar / Dean (approve and schedule)
- Scheduler (creates offering & schedule)

## User Story

As an Admissions Officer, I want to submit a subject opening request for a student when a subject isn’t offered, so the Registrar can approve and scheduling can create a special offering.

## Acceptance Criteria

1. Submit request with Student, Subject, reason, preferred days/times; status = Pending.
2. Registrar/Dean can Approve/Reject; approval captures approver and remarks; optional extra fee flag.
3. Approved requests can generate a prefilled Subject Offering draft (subject/teacher TBD) for Scheduler to complete.

## UI Notes

- Request form; approvals queue; link to create offering; show extra fee policy note.

## Related Requirements / Source

- Requirements item 6 (Subject Request to Open) and Phase 2 scope.
