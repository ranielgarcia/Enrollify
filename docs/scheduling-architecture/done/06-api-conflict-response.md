# API Conflict Response — POST/GET Return Conflicts[]

**Feature Type:** API Endpoints  
**Research Reference:** `docs\scheduling-architecture\research-document-base\IMPLEMENTATION-SUMMARY.md:83-90`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

The POST endpoint for adding schedules returns HTTP 201 Created with `conflicts[]` embedded in the response body, even when conflicts exist. This enables the frontend to display conflicts immediately without a separate API call.

## Write Path

### `POST /subject-offerings/{id}/schedules`

**Request:**
```json
{
  "schedules": [
    { "dayOfWeek": "MON", "startTime": "09:00", "endTime": "10:30" }
  ]
}
```

**Response (201 Created):**
```json
{
  "scheduleIds": [1, 2, 3],
  "conflicts": [
    {
      "type": "TEACHER_DOUBLE_BOOKED",
      "severity": "error",
      "message": "Dr. Santos is already teaching CS201 on MON 09:00-10:30 in section BSCS-2A",
      "day": "MON",
      "startTime": "09:00",
      "endTime": "10:30",
      "affectedOfferings": [ ... ]
    }
  ]
}
```

## Design Decision

Conflicts do **NOT** block saves. Schedules are saved successfully, and conflicts are returned for the frontend to display. Admins can decide whether to fix conflicts before opening enrollment. This provides flexibility while still surfacing issues.

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.WebAPI/Features/SubjectOfferings/AddScheduleToOfferingEndpoint.cs` | POST endpoint |
| `Enrollify.Application/Features/ClassSectionSubjectOfferings/Commands/AddMultipleSchedulesToOffering.cs` | Command with conflict response |
