# AcademicCoreSettings — Scheduling Configuration

**Feature Type:** Configuration  
**Research Reference:** `docs\scheduling-architecture\research-document-base\IMPLEMENTATION-SUMMARY.md:22-25`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

Centralized configuration for scheduling rules, stored in `AcademicCoreSettings` with sensible defaults.

## Settings

| Setting | Default | Purpose | Used By |
|---------|---------|---------|---------|
| `MinimumTeacherBreakMinutes` | 10 | Minimum gap between back-to-back classes | SC-03 (future) |
| `EarliestClassStartTime` | 07:00 | Earliest allowed class start time | SC-06 (future) |
| `LatestClassEndTime` | 21:00 | Latest allowed class end time | SC-06 (future) |

## Missing Settings (Phase 3)

The following setting is referenced in research docs but **not yet implemented**:
- `MaxTeacherHoursPerWeek` (default: 18) — Required for SC-01 (Teacher Workload Overload)

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Core/AcademicCoreSettings.cs` | Configuration class with private setters |
