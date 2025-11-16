---
title: US-020 Assessment & Fees Management
id: US-020
epic: Finance
priority: high
create_by: Copilot
created_at: 2025-11-16T12:33:00Z
updated_at: 2025-11-16T12:33:00Z
---

## Summary

Compute assessments based on enrolled offerings and fee packages; support misc fees and partial payments.

## Persona(s)

- Cashier/Finance Officer
- Registrar (view-only linkage to enrollment status)

## User Story

As a Cashier, I want to assess fees for a student’s enrolled subjects and accept partial payments, so balances and receipts are tracked correctly per term.

## Acceptance Criteria

1. Assessment generated from enrolled subjects + misc fees; package per college/course supported.
2. Partial payments allowed; payment history tracked per EnrollmentId.
3. Outstanding balance check integrates with enrollment clearance flag.

## API / Back-end Notes

- Endpoints: /api/enrollments/{id}/assessment, /api/enrollments/{id}/payments
- DB: EnrollmentPayments(Id, EnrollmentId FK, Amount, PaymentDate, PaymentMethod, ReferenceNumber, PaymentStatus, audit, IsActive)

## UI Notes

- Cashier portal: search student, view term enrollments, apply payments, print receipt PDF.

## Related Requirements / Source

- Phase 3 Financial Module, requirements item 10 (partial payments), item 9 (misc fees selection rules).
