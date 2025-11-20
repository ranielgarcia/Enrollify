---
title: US-019 Academic Records & Grades (per Enrollment)
id: US-019
epic: Academics
priority: high
create_by: Copilot
created_at: 2025-11-16T12:31:00Z
updated_at: 2025-11-16T12:31:00Z
---

## Summary

Teachers input midterm/final grades per enrollment; registrar manages corrections and publishes records.

## Persona(s)

- Teacher/Faculty (encode)
- Registrar (manage/approve)

## User Story

As a Teacher, I want to submit grades for my enrolled students, so academic records are updated securely and accurately.

## Acceptance Criteria

1. Teacher can list students per offering and enter MidtermGrade/FinalGrade within 0–100; 400 on out-of-range.
2. Registrar can lock/unlock grade submission windows; after lock, updates require registrar override.
3. Grade changes are audited with who/when and require reason.

## API / Back-end Notes

- Endpoints: /api/enrollments/{id}/grades
- DB: EnrollmentAcademicRecords(Id, EnrollmentId FK, MidtermGrade CHECK, FinalGrade CHECK, Remarks, audit, IsActive)

## UI Notes

- Teacher portal page per offering; registrar portal for overrides; export to PDF for Transcript/CoR.

## Related Requirements / Source

- SQL: EnrollmentAcademicRecords; Requirements Phase 4 (grading, transcripts, GPA).
