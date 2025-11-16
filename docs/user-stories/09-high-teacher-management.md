---
title: US-009 Teacher Management (CRUD)
id: US-009
epic: People Management
priority: high
create_by: Copilot
created_at: 2025-11-16T12:12:00Z
updated_at: 2025-11-16T12:12:00Z
---

## Summary

Manage teacher records, link to departments, and assign subjects.

## Persona(s)

- System Administrator
- Department Head / Program Coordinator

## User Story

As an Admin, I want to create and update teacher profiles, so they can be assigned to subjects and schedules.

## Acceptance Criteria

1. Given valid teacher details When I create Then it saves (201) with unique email.
2. Given duplicate email When creating Then system rejects with 409.
3. Given teacher exists When I change DepartmentId to a non-existent one Then 400/404.

## Definition of Done

- [ ] CRUD UI/API
- [ ] Unique email enforced
- [ ] FK to Departments validated
- [ ] RBAC

## Business Rules / Validation

- Email unique; First/Last name required.

## API / Back-end Notes

- Endpoints: /api/teachers
- DB: Teachers(Id, FirstName, LastName, Email UNIQUE, DepartmentId FK, audit, IsActive)

## UI Notes

- Fields: FirstName, LastName, Email, Department (select), Active.

## Edge Cases & Error Handling

- Duplicate email -> 409.

## Test Cases

1. Create teacher -> 201.
2. Create with existing email -> 409.

## Dependencies

- Departments, TeacherSubjects, ClassSectionSubjectOffering.

## Related Requirements / Source

- SQL: Teachers table
