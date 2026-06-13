# Room Schedule Management Page — Design Proposal

> **Audience:** School administrator / Scheduler responsible for viewing and managing room assignments and section offering schedules.
> **Goal:** Provide a visual room-based scheduler view showing all class section offerings scheduled across rooms with time slots, color-coded by course, with conflict detection and detail drilling.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Page Layout & Navigation](#2-page-layout--navigation)
3. [Main Schedule Table — ASCII Wireframes](#3-main-schedule-table--ascii-wireframes)
4. [Filters & Controls](#4-filters--controls)
5. [Conflict Visualization](#5-conflict-visualization)
6. [Interaction Patterns](#6-interaction-patterns)
7. [Data Model & Backend](#7-data-model--backend)
8. [Proposed File Structure](#8-proposed-file-structure)
9. [Implementation Notes](#9-implementation-notes)

---

## 1. Overview

The **Room Schedule** page displays a **room × time slot** grid showing all class section offerings scheduled across physical rooms. Unlike the **Sections Management** page (which is section-centric), this page is **room-centric** and allows schedulers to:

- See which rooms are in use at any given time
- Detect **scheduling conflicts** (two offerings in the same room at overlapping times)
- Visually inspect the distribution of offerings across campus
- Click any offering to drill into its detail view
- Filter by building, room type, college, and course to focus on specific areas

---

## 2. Page Layout & Navigation

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  Curriculum & Scheduling  >  Room Schedule  [Academic Year: 2025-2026 ▾]     │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  [Filter Controls Bar - See Section 4]                                      │
│                                                                              │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  [Schedule Table - See Section 3]  (Horizontal & Vertical Scrollable)       │
│                                                                              │
│                                                                              │
│  Legend:                                                                     │
│  ■ Computer Science (BSCS)  ■ Information Tech (BSIT)  ■ Accountancy (BSA) │
│  ■ Engineering (BSEE)       ■ Business Admin (BSBA)    [More ▾]            │
│                                                                              │
├──────────────────────────────────────────────────────────────────────────────┤
│  Stats: 187 rooms scheduled | 42 conflicts detected | 5,243 student capacity│
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Main Schedule Table — ASCII Wireframes

### 3.1 Full Page View (Horizontal Scroll)

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ ROOM      TYPE        BLDG    │ 7:00 AM │ 8:00 AM │ 9:00 AM │ 10:00 AM│ 11:00 AM│ 12:00 PM│ 1:00 PM │ 2:00 PM │ 3:00 PM │ 4:00 PM │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 101      │ Lecture  │ Main    │         │         │ ┌─────┐ │         │         │ LUNCH   │ ┌─────┐ │         │         │         │
│          │          │         │         │         │ │BSCS3A│ │         │         │         │ │BSIT2B│ │         │         │         │
│          │          │         │         │         │ │Smith │ │         │         │         │ │Jones │ │         │         │         │
│          │          │         │         │         │ │08:30 │ │         │         │         │ │13:00 │ │         │         │         │
│          │          │         │         │         │ └─────┘ │         │         │         │ └─────┘ │         │         │         │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 102      │ Lab      │ Main    │         │ ┌─────┐ │ ┌─────┐ │ ┌─────┐ │         │ LUNCH   │         │ ┌─────┐ │         │         │
│          │          │         │         │ │BSEE │ │ │BSEE │ │ │BSEE │ │         │         │         │ │BSCS │ │         │         │
│          │          │         │         │ │Lee  │ │ │Lee  │ │ │Lee  │ │         │         │         │ │3C    │ │         │         │
│          │          │         │         │ │08:00│ │ │09:00│ │ │10:00│ │         │         │         │ │14:00│ │         │         │
│          │          │         │         │ └─────┘ │ └─────┘ │ └─────┘ │         │         │         │ └─────┘ │         │         │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 201      │ Lecture  │ Main    │         │         │         │ ┌─────┐ │ ┌─────┐ │ LUNCH   │ ┌─────┐ │         │ ┌─────┐ │         │
│          │          │         │         │         │         │ │BSBA │ │ │BSBA │ │         │ │BSBA │ │         │ │BSIT │ │         │
│          │          │         │         │         │         │ │3B   │ │ │3B   │ │         │ │3B   │ │         │ │4A   │ │         │
│          │          │         │         │         │         │ │Reyes│ │ │Reyes│ │         │ │Reyes│ │         │ │Dela │ │         │
│          │          │         │         │         │         │ │10:30│ │ │11:00│ │         │ │13:00│ │         │ │15:30│ │         │
│          │          │         │         │         │         │ └─────┘ │ └─────┘ │         │ └─────┘ │         │ └─────┘ │         │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 301      │ Conference│ Annex  │ ┌─────┐ │         │ ┌─────┐ │         │         │ LUNCH   │ ┌─────┐ │         │         │         │
│ ⚠️CONFLICT│          │         │ │BSCS │ │ ┌─────┐│ │BSA  │ │ ┌─────┐ │         │         │ │BSEE │ │ ┌─────┐ │         │         │
│          │          │         │ │1A   │ │ │BSIT │ │ │4B   │ │ │BSIT │ │         │         │ │1B   │ │ │BSCS │ │         │         │
│          │          │         │ │Smith│ │ │2A   │ │ │Cruz │ │ │1A   │ │         │         │ │Lee  │ │ │3C   │ │         │         │
│          │          │         │ │07:30│ │ │08:30│ │ │10:00│ │ │10:30│ │         │         │ │13:00│ │ │13:30│ │         │         │
│          │          │         │ └─────┘ │ │Jonas│ │ └─────┘ │ └─────┘ │         │         │ └─────┘ │ └─────┘ │         │         │
│          │          │         │  ┌────────┤ │08:00│─────────┤         │         │         │         │         │         │         │
│          │          │         │  │OVERLAP!│ └─────┘         │         │         │         │         │         │         │         │
│          │          │         │  └────────┘                 │         │         │         │         │         │         │         │
│          │          │         │ (BSCS 1A overlaps BSIT 2A)  │         │         │         │         │         │         │         │
│          │          │         │                             │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 302      │ Lab      │ Annex   │         │         │         │ ┌─────┐ │ ┌─────┐ │ LUNCH   │ ┌─────┐ │ ┌─────┐ │         │ ┌─────┐ │
│          │          │         │         │         │         │ │BSEE │ │ │BSEE │ │         │ │BSEE │ │ │BSEE │ │         │ │BSEE │ │
│          │          │         │         │         │         │ │2A   │ │ │2A   │ │         │ │2A   │ │ │2A   │ │         │ │2A   │ │
│          │          │         │         │         │         │ │Lee  │ │ │Lee  │ │         │ │Lee  │ │ │Lee  │ │         │ │Lee  │ │
│          │          │         │         │         │         │ │10:00│ │ │11:00│ │         │ │13:00│ │ │14:00│ │         │ │16:00│ │
│          │          │         │         │         │         │ └─────┘ │ └─────┘ │         │ └─────┘ │ └─────┘ │         │ └─────┘ │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 401      │ Lecture  │ North   │         │         │ ┌─────┐ │         │         │ LUNCH   │         │ ┌─────┐ │         │         │
│          │          │         │         │         │ │BSA  │ │         │         │         │         │ │BSCS │ │         │         │
│          │          │         │         │         │ │3A   │ │         │         │         │         │ │2B   │ │         │         │
│          │          │         │         │         │ │Cruz │ │         │         │         │         │ │Smith│ │         │         │
│          │          │         │         │         │ │09:00│ │         │         │         │         │ │14:30│ │         │         │
│          │          │         │         │         │ └─────┘ │         │         │         │         │ └─────┘ │         │         │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 402      │ Seminar  │ North   │ ┌─────┐ │         │         │ ┌─────┐ │ ┌─────┐ │ LUNCH   │ ┌─────┐ │         │ ┌─────┐ │         │
│          │          │         │ │BSIT │ │         │         │ │BSIT │ │ │BSBA │ │         │ │BSIT │ │         │ │BSBA │ │         │
│          │          │         │ │1B   │ │         │         │ │4A   │ │ │2B   │ │         │ │3A   │ │         │ │4B   │ │         │
│          │          │         │ │Jonas│ │         │         │ │Dela │ │ │Cruz │ │         │ │Jonas│ │         │ │Reyes│ │         │
│          │          │         │ │07:00│ │         │         │ │10:30│ │ │11:00│ │         │ │13:30│ │         │ │15:30│ │         │
│          │          │         │ └─────┘ │         │         │ └─────┘ │ └─────┘ │         │ └─────┘ │         │ └─────┘ │         │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ 403      │ Lab      │ North   │         │ ┌─────┐ │         │         │         │ LUNCH   │         │         │ ┌─────┐ │         │
│          │          │         │         │ │BSEE │ │         │         │         │         │         │         │ │BSEE │ │         │
│          │          │         │         │ │1A   │ │         │         │         │         │         │         │ │3B   │ │         │
│          │          │         │         │ │Lee  │ │         │         │         │         │         │         │ │Lee  │ │         │
│          │          │         │         │ │08:30│ │         │         │         │         │         │         │ │15:00│ │         │
│          │          │         │         │ └─────┘ │         │         │         │         │         │         │ └─────┘ │         │
│          │          │         │         │         │         │         │         │         │         │         │         │         │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Offering Card Cell Detail (Zoomed In)

Each time slot cell contains one or more offerings. When there are conflicts, they stack vertically with a **red border/background**.

```
Normal Offering (No Conflict):
┌──────────────────┐
│ ┌──────────────┐ │
│ │  BSCS 3A     │ │  ← Section Code
│ │  Smith       │ │  ← Teacher Name
│ │  08:30-09:30 │ │  ← Time Range
│ └──────────────┘ │
└──────────────────┘

Green background = BSCS (Computer Science)


Conflicting Offerings (Red Border & Stacked):
┌──────────────────┐
│ ┌──────────────┐ │
│ │  BSCS 1A     │ │
│ │  Smith       │ │
│ │  07:30-08:30 │ │
│ └──────────────┘ │
│ ┌──────────────┐ │  ← Overlapping with above
│ │  BSIT 2A     │ │     Same room, overlapping time
│ │  Jonas       │ │
│ │  08:00-09:00 │ │  ← Red border indicates CONFLICT
│ └──────────────┘ │
│ ⚠️ CONFLICT      │
└──────────────────┘

Red background border + warning symbol


Empty/Lunch Slot:
┌──────────────────┐
│   LUNCH BREAK    │
│                  │
│   12:00 PM       │
└──────────────────┘

Gray background
```

---

## 4. Filters & Controls

### 4.1 Filter Bar Layout

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  [Building ▾]           [Room Type ▾]          [College ▾]        [Course ▾] │
│  ─────────────          ──────────────         ───────────        ────────── │
│  ○ All Buildings         ○ All Types           ○ All Colleges     ○ All     │
│  ☑ Main Campus           ☑ Lecture Halls      ☑ Engineering     ☑ BSCS    │
│  ☑ Annex                 ☑ Labs               ☑ Business        ☑ BSIT    │
│  ☑ North Building        ☑ Conference Rooms   ☑ Arts & Sciences  ☑ BSEE    │
│  ☑ South Building        ☑ Seminars           ☐ Fine Arts        ☑ BSA     │
│                                                                  ☑ BSBA    │
│  [Apply]  [Clear All]    [Export]                               [More ▾]   │
│                                                                              │
│  Showing: 187 rooms | 312 offerings | 42 conflicts | 5,243 student capacity│
└──────────────────────────────────────────────────────────────────────────────┘
```

**Filter Options:**

| Filter | Options | Purpose |
|---|---|---|
| **Building** | List all buildings on campus; multi-select checkboxes | Focus on physical location; helps with resource planning by building |
| **Room Type** | Lecture Hall, Lab, Conference Room, Seminar, Amphitheater, etc. | Filter by facility type (some courses require specific room types) |
| **College/Department** | Engineering, Business, Arts & Sciences, Fine Arts, etc. | View schedules by organizational unit |
| **Course/Program** | BSCS, BSIT, BSEE, BSA, BSBA, etc. | Focus on specific program's offerings |

---

## 5. Conflict Visualization

### 5.1 Conflict Indicators

**On the page:**
- **Red border/background** on offering cards that have conflicts
- **⚠️ CONFLICT** label with count (e.g., "⚠️ 3 conflicts in this cell")
- **Stats bar at bottom** shows total conflict count: `42 conflicts detected`

**On the Room row:**
- **⚠️ CONFLICT badge** next to room name if room has any conflicts today
- Red text/icon to highlight problematic rooms

### 5.2 Conflict Types Detected

1. **Room Double-Booked:** Two offerings assigned to same room at overlapping times
2. **Time Overlap:** Offerings in same room with overlapping time ranges
3. **Teacher Double-Booked:** (Optional) Show if a teacher is assigned to two offerings at the same time (cross-room visualization)

### 5.3 Conflict Resolution Workflow

When user **clicks a conflicted offering card:**

```
┌─────────────────────────────────────────────────────────────┐
│  Offering Detail Drawer                      [Close]         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  BSCS 3A — Section Code: 3A                                 │
│  Subject: Data Structures                                   │
│  Teacher: Dr. Smith                                         │
│  Room: 301 (Annex - Conference Room)                        │
│  Schedule: Monday 07:30 - 08:30                            │
│                                                              │
│  ⚠️ CONFLICT DETECTED:                                      │
│  This offering overlaps with:                              │
│  - BSIT 2A (Jonas) in same room at 08:00 - 09:00          │
│                                                              │
│  Suggested Actions:                                        │
│  [Change Time]  [Change Room]  [View BSIT 2A]  [Help Me]  │
│                                                              │
│  ─────────────────────────────────────────────────────────  │
│  [Go to Section Detail Page]  [Go to Room Management]      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. Interaction Patterns

### 6.1 Click an Offering Card

**Action:** Click on any offering card (e.g., "BSCS 3A")

**Result:** Opens a **side drawer** (right-slide) showing:
- Section code, subject name, teacher
- Current room & time assignment
- **Conflict warning** (if applicable with details)
- **Quick actions:** Change Time, Change Room, View Full Section Detail
- Link to full section detail page at `/portal/curriculum-and-scheduling/sections-details/$sectionId`

### 6.2 Hover Over Offering Card

**Action:** Hover over an offering card

**Result:** 
- Card background slightly brightens
- Tooltip shows: `BSCS 3A (Dr. Smith) | Room 301 | 07:30-08:30 | 45 students`
- If conflicted: adds "⚠️ OVERLAPS WITH BSIT 2A"

### 6.3 Scroll Behavior

**Horizontal (Time):** 
- Sticky left columns (Room, Type, Building) remain visible
- Time columns scroll left/right
- Time header remains sticky at top

**Vertical (Rooms):**
- Scrolls through room rows
- Header (Room | Type | Building | Time columns) remains sticky

### 6.4 Export & Analysis

**Export button** → Downloads CSV with columns:
- Room Number, Building, Type, Capacity
- Offering Code, Section, Subject, Teacher
- Time Start, Time End, Conflicts

---

## 7. Data Model & Backend

### 7.1 Required Data for Display

**Rooms:**
```typescript
Room {
  id: number
  roomNumber: string
  building: { id, name }
  roomType: { id, name }
  capacity: number
}
```

**Offerings (already exists):**
```typescript
Offering {
  id: number
  classSectionId: number
  subjectId: number
  snapshotSubjectCode: string
  snapshotSubjectTitle: string
  teacher: { id, firstName, lastName }
  room: { id, roomNumber, building: { name } }
  schedules: ClassSchedule[]
  conflicts: ConflictResult[]
  daysPerWeek: number
  hoursPerDay: number
}

ClassSchedule {
  id: number
  dayOfWeek: string
  startTime: string (HH:mm)
  endTime: string (HH:mm)
}

ClassSection {
  id: number
  name: string
  sectionCode: string
  course: { id, code, name }
  intendedYearLevel: number
}
```

### 7.2 Required Backend Endpoints

| Endpoint | Method | Purpose |
|---|---|---|
| `GET /api/rooms/schedule` | GET | Fetch all rooms + offerings + schedules + conflicts filtered by criteria |
| `GET /api/rooms/{id}/schedule` | GET | Fetch single room with all offerings for a day |
| `POST /api/rooms/schedule/export` | POST | Export filtered schedule as CSV |
| (Existing) `GET /api/subject-offerings/{id}` | GET | Detail view when clicking an offering |

**Example payload for `GET /api/rooms/schedule`:**

```json
{
  "academicYearId": 1,
  "building": ["Main Campus", "Annex"],
  "roomType": ["Lecture Hall", "Lab"],
  "college": ["Engineering", "Business"],
  "course": ["BSCS", "BSEE"],
  "dayOfWeek": "Monday"
}

Response:
{
  "rooms": [
    {
      "id": 1,
      "roomNumber": "101",
      "building": { "id": 1, "name": "Main Campus" },
      "roomType": { "id": 1, "name": "Lecture Hall" },
      "capacity": 50,
      "hasConflicts": false,
      "offerings": [
        {
          "id": 101,
          "sectionCode": "3A",
          "subjectCode": "BSCS",
          "subjectTitle": "Data Structures",
          "teacherName": "Dr. Smith",
          "startTime": "08:00",
          "endTime": "09:30",
          "conflicts": []
        },
        {
          "id": 102,
          "sectionCode": "2B",
          "subjectCode": "BSIT",
          "subjectTitle": "Database Design",
          "teacherName": "Dr. Jones",
          "startTime": "14:00",
          "endTime": "15:30",
          "conflicts": []
        }
      ]
    },
    {
      "id": 2,
      "roomNumber": "301",
      "building": { "id": 1, "name": "Annex" },
      "roomType": { "id": 3, "name": "Conference Room" },
      "capacity": 25,
      "hasConflicts": true,
      "offerings": [
        {
          "id": 201,
          "sectionCode": "1A",
          "subjectCode": "BSCS",
          "subjectTitle": "Intro to CS",
          "teacherName": "Dr. Smith",
          "startTime": "07:30",
          "endTime": "08:30",
          "conflicts": [
            {
              "type": "ROOM_DOUBLE_BOOKED",
              "severity": "Error",
              "overlappingOfferingId": 202,
              "overlappingOfferingCode": "BSIT 2A",
              "overlappingTeacher": "Dr. Jonas",
              "timeRange": "08:00-09:00"
            }
          ]
        },
        {
          "id": 202,
          "sectionCode": "2A",
          "subjectCode": "BSIT",
          "subjectTitle": "Network Basics",
          "teacherName": "Dr. Jonas",
          "startTime": "08:00",
          "endTime": "09:00",
          "conflicts": [
            {
              "type": "ROOM_DOUBLE_BOOKED",
              "severity": "Error",
              "overlappingOfferingId": 201,
              "overlappingOfferingCode": "BSCS 1A",
              "overlappingTeacher": "Dr. Smith",
              "timeRange": "07:30-08:30"
            }
          ]
        }
      ]
    }
  ],
  "stats": {
    "totalRooms": 187,
    "totalOfferings": 312,
    "totalConflicts": 42,
    "totalStudentCapacity": 5243
  }
}
```

---

## 8. Proposed File Structure

New page under `src/page-components/`:

```
room-schedule-page/
├── index.tsx                      # Page root — fetches rooms + offerings, renders layout
├── room-schedule-table.tsx        # Main table component (room × time grid)
├── room-schedule-cell.tsx         # Single cell (offering card or multiple for conflicts)
├── offering-card.tsx              # Individual offering display (color-coded box)
├── conflict-indicator.tsx         # ⚠️ badge and "OVERLAP" label
├── schedule-filters.tsx           # Building, Room Type, College, Course filters
├── filter-controls.tsx            # Filter dropdowns/checkboxes
├── offering-detail-drawer.tsx     # Side drawer when clicking an offering
├── room-row.tsx                   # Single room row (header + time cells)
├── time-column-header.tsx         # Hour headers (7:00 AM, 8:00 AM, etc.)
├── lunch-break-cell.tsx           # Gray "LUNCH BREAK" cells
├── export-schedule.tsx            # Export as CSV button
└── schedule-legend.tsx            # Color legend by course/program

src/api/collections/
├── room-schedule-collection.ts    # NEW — query keys, endpoints for room schedule
src/api/models/
├── room-schedule.ts               # NEW — types for Room Schedule endpoint response
```

---

## 9. Implementation Notes

### 9.1 Color Coding Scheme

Assign a distinct color to each course/program:

```
BSCS (Computer Science)      → Green (#22c55e)
BSIT (Information Tech)      → Blue (#3b82f6)
BSEE (Electrical Engineering)→ Orange (#f97316)
BSA (Accountancy)             → Purple (#a855f7)
BSBA (Business Admin)         → Pink (#ec4899)
BSMA (Business Management)    → Teal (#06b6d4)
Other programs                → Gray (#6b7280)

Conflicting offerings         → Red border (#ef4444) + warning icon
```

The legend at bottom should display all courses with their colors.

### 9.2 Time Slot Granularity

- **Column interval:** 1 hour (7:00 AM, 8:00 AM, 9:00 AM, etc.)
- **Offerings can span multiple columns** (e.g., 07:30-09:00 spans 1.5 hour cells)
- Consider showing **30-minute intervals** if space allows, or offer a zoom control

### 9.3 Sticky Headers & Scrolling

- **Left columns (Room, Type, Building)** remain fixed when scrolling horizontally
- **Top header (time columns)** remains fixed when scrolling vertically
- **Footer (stats bar)** remains visible
- Use CSS `position: sticky` or a virtualization library for large datasets

### 9.4 Performance Considerations

- **Lazy-load rooms** if 200+ rooms exist; paginate or use virtual scrolling
- **Pre-calculate conflicts** in the backend; don't compute on frontend
- **Memoize** offering cards and room rows to prevent unnecessary re-renders
- **Debounce filter changes** to avoid excessive re-fetches

### 9.5 Mobile / Responsive Design

- On **small screens**, switch to a **single-room detail view** (picker for which room to view)
- Or show a **course/program view** instead (all offerings of BSCS across all rooms)
- Consider a **list view** alternative for mobile (show offerings as scrollable list with room/time inline)

### 9.6 Legend & Visual Indicators

At the bottom of the page:

```
Legend:  ■ BSCS  ■ BSIT  ■ BSEE  ■ BSA  ■ BSBA  [More ▾]

⚠️ Conflict indicator — Red border around offerings that overlap in time/room
🕐 Lunch Break — Gray cell marking lunch period (typically 12:00-13:00)
```

---

## 10. Sample Data Breakdown

From the ASCII wireframes above, here's the sample data structure:

### Room 101 (Main - Lecture Hall, Capacity 50)
- **8:00-9:30 AM:** BSCS 3A (Dr. Smith) — Green card
- **1:00-2:30 PM:** BSIT 2B (Dr. Jones) — Blue card
- Rest: Empty

### Room 301 (Annex - Conference Room, Capacity 25) ⚠️ **CONFLICTS**
- **7:30-8:30 AM:** BSCS 1A (Dr. Smith) — Green card with RED BORDER
  - **Overlaps with:** BSIT 2A (Dr. Jonas) at 8:00-9:00 AM — Blue card with RED BORDER
- **10:00-11:00 AM:** BSA 4B (Cruz)
- **10:30-11:30 AM:** BSIT 1A (Jonas) — Overlaps with above
- **1:00-2:00 PM:** BSEE 1B (Dr. Lee)
- **1:30-2:30 PM:** BSCS 3C
- Rest: Empty

The wireframe shows these stacked in the time cells with conflict warning.

---

## 11. Implementation Phases

### Phase 1: Core Table & Layout (2-3 days)
- Build room schedule table structure
- Sticky headers & scrolling
- Time column generation
- Room row rendering

### Phase 2: Offerings & Conflict Display (2-3 days)
- Fetch offerings via new endpoint
- Render offering cards in cells
- Implement conflict visualization (red borders, stacking)
- Color-code by course

### Phase 3: Filters & Controls (1-2 days)
- Build filter bar (Building, Room Type, College, Course)
- Implement multi-select checkboxes
- Apply/clear filters
- Update query params in URL

### Phase 4: Detail Drawer & Interaction (1-2 days)
- Click handler for offering cards
- Side drawer with offering details
- Conflict details in drawer
- Quick action buttons

### Phase 5: Export & Polish (1 day)
- CSV export endpoint
- Legend component
- Responsive design
- Performance optimization (memoization, virtualization)

---

## 12. User Journey Example

**Scenario:** Scheduler notices conflict in Room 301

1. **Opens Room Schedule page** → Sees full grid of rooms × times
2. **Notices** ⚠️ red border on Room 301 at 7:30-9:00 AM slot
3. **Clicks the red offering card** → Side drawer opens showing:
   - BSCS 1A (Smith) 07:30-08:30
   - ⚠️ Conflicts with BSIT 2A (Jonas) 08:00-09:00 in same room
4. **Clicks "Change Time" or "Change Room"** → Navigates to section detail page to adjust schedule
5. **Returns to Room Schedule page** → Refreshes and sees conflict resolved

---
