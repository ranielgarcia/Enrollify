# Consolidated Endpoints — Removed Redundant Offerings Query

**Feature Type:** API Refactor  
**Research Reference:** `docs\scheduling-architecture\research-document-base\REMAINING-TASKS.md:19`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

Removed redundant offerings query endpoint to consolidate the API surface. Conflict detection and offering retrieval are now consistently handled through:

| Endpoint | Purpose |
|----------|---------|
| `GET /class-sections/{id}` | Section detail with embedded offerings + conflicts |
| `GET /subject-offerings?classSectionId=X` | Offerings with schedules + conflicts |
| `POST /subject-offerings/{id}/schedules` | Add schedules + return conflicts |

This eliminates API duplication and ensures conflict detection is consistently applied across all read paths.
