---
title: US-016 Student Management (CRUD)
id: US-016
epic: People & Enrollment
priority: high
create_by: Copilot
created_at: 2025-11-16T12:20:00Z
updated_at: 2025-11-16T12:20:00Z
---

## Summary

Manage student records and link to course, year level, and status needed for enrollment and billing.

## Persona(s)

- Admissions Officer
- Registrar

## User Story

As an Admissions Officer, I want to create and update student profiles, so they can be processed for enrollment, assessment, and record-keeping.

## Acceptance Criteria

1. Given required fields When I create a student Then StudentNumber and Email must be unique and record saved (201).
2. Given invalid YearLevel When I set YearLevel outside 1–6 Then 400.
3. Given a student exists When I change Status Then StudentStatuses FK must validate.

## Definition of Done

- [ ] CRUD UI/API
- [ ] Unique StudentNumber and Email enforced
- [ ] YearLevel validation
- [ ] Audit fields & RBAC

## Business Rules / Validation

- StudentNumber format <=13 unique; Email unique; YearLevel 1–6; CourseId FK required.

## API / Back-end Notes

- Endpoints: /api/students
- DB: Students(Id, StudentNumber UNIQUE, FirstName, LastName, Email UNIQUE, CourseId FK, YearLevel, Status FK, audit, IsActive)

## UI Notes

- Fields: StudentNumber, FirstName, LastName, Email, Course, YearLevel, Status.
- Search by StudentNumber, Name, Email.

## Edge Cases & Error Handling

- Duplicate StudentNumber or Email -> 409.
- Course not found -> 404.

## Test Cases

1. Create valid student -> 201.
2. Duplicate student number -> 409.

## Dependencies

- Courses, StudentStatuses, Enrollments, Payments.

## Related Requirements / Source

- SQL: Students table; Requirements: Admission and enrollment workflows.
