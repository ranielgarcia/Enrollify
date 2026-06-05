# Database Index Recommendations for Conflict Detection

**Status:** To be implemented after performance measurement  
**Created:** 2026-06-06  
**Decision:** Implement queries first, measure performance on realistic dataset (10,000+ schedules), then add indexes if queries exceed 50ms

---

## Background

The conflict detection queries in Phase 1 join `ClassSchedules` → `ClassSectionSubjectOffering` → `ClassSections` to check for teacher double-booking (HC-01), room double-booking (HC-02), and section overlap (HC-03). These queries filter by:
- `DayOfWeek` (e.g., 'MON', 'TUE')
- `StartTime` and `EndTime` (half-open interval overlap check)
- `TeacherId` or `RoomId`  
- `AcademicTermId` (via join to `ClassSections`)
- `IsActive = 1` (soft-delete filter)

Currently, **no indexes exist** on `ClassSchedules.(DayOfWeek, StartTime, EndTime)` or on `ClassSectionSubjectOffering.(TeacherId, RoomId)` other than the default foreign key indexes.

---

## Recommended Indexes

### Index 1: ClassSchedules — DayOfWeek + Time Range

**Purpose:** Speed up teacher/room conflict queries by day and time overlap

```sql
-- Script: Script0020__ClassScheduleConflictIndexes.sql

CREATE INDEX IX_ClassSchedules_DayOfWeek_Time
    ON ClassSchedules (DayOfWeek, StartTime, EndTime)
    WHERE IsActive = 1;
```

**Benefit:**  
- Reduces full table scan when filtering by `DayOfWeek = 'MON'`
- Enables index seek for range queries on `StartTime < @endTime AND EndTime > @startTime`
- Filtered index (WHERE `IsActive = 1`) reduces index size and maintenance cost

**Cost:**  
- Additional storage (~5-10% of table size)
- Slower inserts/updates (negligible for admin operations — schedules are created infrequently)

**When to add:**  
- If conflict queries on 10,000+ schedule rows exceed 50ms without this index
- Likely unnecessary for smaller datasets (<5,000 rows)

---

### Index 2: ClassSectionSubjectOffering — TeacherId

**Purpose:** Speed up teacher conflict lookups

```sql
CREATE INDEX IX_ClassSectionSubjectOffering_TeacherId
    ON ClassSectionSubjectOffering (TeacherId, ClassSectionId)
    WHERE IsActive = 1 AND TeacherId IS NOT NULL;
```

**Benefit:**  
- Faster join on `TeacherId` for conflict detection queries
- Composite with `ClassSectionId` allows covering index for section-level queries

**Cost:**  
- Filtered index (low overhead) since `TeacherId` is nullable
- Only indexes active offerings with assigned teachers

**When to add:**  
- If teacher conflict queries consistently exceed 50ms
- More likely needed if you have 200+ active teachers

---

### Index 3: ClassSectionSubjectOffering — RoomId

**Purpose:** Speed up room conflict lookups

```sql
CREATE INDEX IX_ClassSectionSubjectOffering_RoomId
    ON ClassSectionSubjectOffering (RoomId, ClassSectionId)
    WHERE IsActive = 1 AND RoomId IS NOT NULL;
```

**Benefit:** Similar to Index 2, but for room-based conflict queries  
**Cost:** Similar to Index 2  
**When to add:** If room conflict queries consistently exceed 50ms

---

## Memory Footprint for Your Dataset

A flat `ScheduleDTO` record needs approximately 200–300 bytes including .NET object overhead:

| Dataset | Schedule rows | Memory estimate |
|---|---|---|
| Small (100 sections × 8 offerings × 3 days) | 2,400 | ~0.7 MB |
| Medium (300 × 10 × 4) | 12,000 | ~3.5 MB |
| **Worst case** (500 × 15 × 6) | **45,000** | **~13 MB** |

**13 MB is negligible on a modern server.** Even without indexes, SQL Server can scan 45,000 rows in <5ms if the data is cached in memory.

---

## Performance Testing Plan

Before adding these indexes:

1. **Generate test data:**
   - Create 500 sections × 10 offerings × 3 schedules = 15,000 schedule rows
   - Use a realistic distribution of teachers (50-100 distinct) and rooms (30-80 distinct)

2. **Measure baseline query time:**
   ```sql
   SET STATISTICS TIME ON;
   
   -- Test teacher conflict query (HC-01)
   SELECT TOP 1 cs.Id
   FROM ClassSchedules cs
   JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
   JOIN ClassSections sec ON sec.Id = o.ClassSectionId
   WHERE o.TeacherId = 5
     AND sec.AcademicTermId = 10
     AND cs.DayOfWeek = 'MON'
     AND cs.StartTime < '10:30:00'
     AND cs.EndTime > '09:00:00'
     AND o.IsActive = 1
     AND cs.IsActive = 1;
   
   SET STATISTICS TIME OFF;
   ```

3. **Threshold:** If query time < 50ms, indexes are **not needed yet**

4. **Add indexes incrementally:**  
   - Add Index 1 first (most impactful)
   - Measure again
   - Add Index 2/3 only if specific teacher/room queries are still slow

---

## Monitoring Query Plans

Use SQL Server Management Studio execution plan viewer to check:
- Are queries doing **table scans** or **index scans**?
- Are joins using **index seeks**?
- What's the estimated cost?

**Command:**
```sql
SET SHOWPLAN_ALL ON;
-- Run your query
SET SHOWPLAN_ALL OFF;
```

Or use:
```sql
SELECT * FROM sys.dm_exec_query_stats
ORDER BY total_worker_time DESC;
```

---

## Alternative: Query Store

Enable Query Store on the database to track query performance over time:

```sql
ALTER DATABASE [YourDatabase]
SET QUERY_STORE = ON (
    OPERATION_MODE = READ_WRITE,
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 100,
    QUERY_CAPTURE_MODE = AUTO
);
```

Then monitor conflict queries in SQL Server Management Studio → Database → Query Store → Top Resource Consuming Queries.

---

## Implementation Checklist

- [ ] Generate 15,000+ test schedule rows with realistic teacher/room distribution
- [ ] Measure baseline query time for HC-01, HC-02, HC-03 queries
- [ ] Document query times in this file
- [ ] If any query exceeds 50ms:
  - [ ] Add Index 1 (`ClassSchedules`)
  - [ ] Re-measure
  - [ ] Add Index 2/3 if still needed
- [ ] Monitor production query times via Query Store or Application Insights

---

## References

- Research document: `research/how-to-properly-implement-a-validation-logic-that-.md` (line 445-471)
- Dataset size analysis: Same document (line 76-84)
- Implementation: `Enrollify.Infrastructure/Repositories/ClassSectionSubjectOfferingRepository.cs`
