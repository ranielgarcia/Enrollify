# Manual API Testing — Conflict Detection

**Feature Type:** Testing  
**Research Reference:** `docs\scheduling-architecture\research-document-base\REMAINING-TASKS.md:33-68`  
**Phase:** 1A  
**Priority:** HIGH  
**Status:** ⏳ Not Implemented

---

## Description

Manual end-to-end verification of conflict detection via Swagger/Postman before relying on automated tests.

## Test Scenarios

### HC-01: Teacher Double-Booked
1. Create a class section with 2 offerings (Subject A, Subject B)
2. Assign Teacher X to both offerings
3. Add schedule to Subject A: Monday 08:00-09:00
4. Add schedule to Subject B: Monday 08:30-09:30
5. **Expected:** HTTP 201 + conflicts array with `TEACHER_DOUBLE_BOOKED`

### HC-02: Room Double-Booked
1. Assign same room to two offerings with overlapping schedules
2. **Expected:** HTTP 201 + `ROOM_DOUBLE_BOOKED` in conflicts

### HC-03: Section Overlap
1. Create overlapping schedules within same section
2. **Expected:** HTTP 201 + `SECTION_OVERLAP` in conflicts

### DI-05: Duplicate Subject
1. Assign same subject twice to a section
2. **Expected:** `DUPLICATE_SUBJECT_IN_SECTION` in conflicts

## Verification Checklist

- [ ] POST returns HTTP 201
- [ ] Response includes `conflicts[]` array
- [ ] Conflicts have correct type codes
- [ ] Conflicts have correct severity
- [ ] Conflicts include `affectedOfferings[]` with details
- [ ] GET section detail includes offerings with embedded conflicts
