# SQL Schema Extensions for Genetic Algorithm Integration

**Date:** November 11, 2025  
**Purpose:** Bridge database schema with genetic algorithm scheduling requirements

---

## 🎯 Overview

Your genetic algorithm (`Enhance-Genetic-Algorithm-v2`) requires additional fields not present in the current database schema. This document outlines necessary schema extensions.

---

## 📊 Current GA Requirements vs Database Schema

### Missing Fields in Subjects Table

| GA Field | Current Schema | Status | Impact |
|----------|----------------|--------|--------|
| `DaysPerWeek` | ❌ Not present | **Critical** | Cannot schedule correctly |
| `HoursPerDay` | ❌ Not present | **Critical** | Cannot determine timeslot duration |
| `PreferredDayPattern` | ❌ Not present | **High** | Sub-optimal scheduling |
| `RequiresLab` | ⚠️ Indirect via RoomType | **Medium** | Can infer from room requirements |
| `SubjectType` | ❌ Not present | **Medium** | Cannot apply type-specific rules |

### Missing Fields in ClassSectionSubjectOffering

| GA Field | Current Schema | Status | Impact |
|----------|----------------|--------|--------|
| Schedule Pattern | Individual rows per day | **High** | Difficult to enforce DaysPerWeek |
| TimeSlot validation | No constraint | **High** | Can create invalid schedules |
| Professor qualification | No link to TeacherSubjects | **Critical** | Can assign unqualified teachers |

---

## 🔧 Required Schema Modifications

### 1. Extend Subjects Table (Critical)

```sql
ALTER TABLE Subjects
ADD 
    -- For genetic algorithm scheduling
    DaysPerWeek INT NOT NULL DEFAULT 2,
    HoursPerDay DECIMAL(3,1) NOT NULL DEFAULT 1.5,
    PreferredDayPattern VARCHAR(10) NULL, -- 'MW', 'TTh', 'MWF', 'MTWTHF'
    RequiresLab BIT NOT NULL DEFAULT 0,
    SubjectType VARCHAR(50) NULL, -- 'ComputerScience', 'Mathematics', 'Physics', etc.
    
    -- Constraints
    CONSTRAINT CHK_Subjects_DaysPerWeek 
        CHECK (DaysPerWeek BETWEEN 1 AND 6),
    CONSTRAINT CHK_Subjects_HoursPerDay 
        CHECK (HoursPerDay BETWEEN 0.5 AND 4.0),
    CONSTRAINT CHK_Subjects_PreferredDayPattern 
        CHECK (PreferredDayPattern IS NULL OR 
               PreferredDayPattern IN ('M','T','W','Th','F','S',
                                       'MW','TTh','MWF','TThF','MTWTHF',
                                       'MTWTh','WThF','FS'));
GO

-- Add comment
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Number of days per week this subject should be scheduled (used by genetic algorithm)',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE', @level1name = 'Subjects',
    @level2type = N'COLUMN', @level2name = 'DaysPerWeek';
```

### 2. Add SubjectTypes Lookup Table (Recommended)

```sql
CREATE TABLE SubjectTypes
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255) NULL,
    DefaultRequiresLab BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- Seed data matching GA enums
INSERT INTO SubjectTypes (Name, Description, DefaultRequiresLab)
VALUES 
    ('ComputerScience', 'Computer Science and IT courses', 1),
    ('Mathematics', 'Mathematics courses', 0),
    ('Physics', 'Physics courses', 1),
    ('Chemistry', 'Chemistry courses', 1),
    ('Engineering', 'Engineering courses', 1),
    ('English', 'English and Literature courses', 0),
    ('GeneralEducation', 'General education courses', 0),
    ('PhysicalEducation', 'Physical education courses', 0),
    ('BusinessAdministration', 'Business and management courses', 0);
GO

-- Update Subjects table to use lookup
ALTER TABLE Subjects
ADD SubjectTypeId INT NULL,
    CONSTRAINT FK_Subjects_SubjectType 
        FOREIGN KEY (SubjectTypeId) REFERENCES SubjectTypes(Id);
GO

CREATE NONCLUSTERED INDEX IX_Subjects_SubjectTypeId 
ON Subjects(SubjectTypeId);
GO
```

### 3. Add DayPatterns Lookup Table (Recommended)

```sql
CREATE TABLE DayPatterns
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(10) NOT NULL UNIQUE, -- 'MW', 'TTh', etc.
    Description VARCHAR(100) NOT NULL,
    DaysCount INT NOT NULL,
    DaysOfWeek VARCHAR(50) NOT NULL, -- 'Monday,Wednesday'
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CHK_DayPatterns_DaysCount 
        CHECK (DaysCount BETWEEN 1 AND 6)
);
GO

-- Seed common patterns
INSERT INTO DayPatterns (Code, Description, DaysCount, DaysOfWeek)
VALUES 
    ('M', 'Monday only', 1, 'Monday'),
    ('T', 'Tuesday only', 1, 'Tuesday'),
    ('W', 'Wednesday only', 1, 'Wednesday'),
    ('Th', 'Thursday only', 1, 'Thursday'),
    ('F', 'Friday only', 1, 'Friday'),
    ('S', 'Saturday only', 1, 'Saturday'),
    ('MW', 'Monday-Wednesday', 2, 'Monday,Wednesday'),
    ('TTh', 'Tuesday-Thursday', 2, 'Tuesday,Thursday'),
    ('WF', 'Wednesday-Friday', 2, 'Wednesday,Friday'),
    ('MWF', 'Monday-Wednesday-Friday', 3, 'Monday,Wednesday,Friday'),
    ('TThF', 'Tuesday-Thursday-Friday', 3, 'Tuesday,Thursday,Friday'),
    ('MTWTh', 'Monday-Thursday', 4, 'Monday,Tuesday,Wednesday,Thursday'),
    ('MTWTHF', 'Monday-Friday', 5, 'Monday,Tuesday,Wednesday,Thursday,Friday'),
    ('FS', 'Friday-Saturday', 2, 'Friday,Saturday');
GO
```

### 4. Modify ClassSectionSubjectOffering (Critical)

**Option A: Keep Current Design (Multiple Rows per Schedule)**

```sql
-- Add constraints to existing design
ALTER TABLE ClassSectionSubjectOffering
ADD 
    ScheduleGroup INT NULL, -- Group related schedules together
    CONSTRAINT CHK_ClassSectionSubjectOffering_TimeValidation
        CHECK (StartTime < EndTime),
    CONSTRAINT CHK_ClassSectionSubjectOffering_DayOfWeek
        CHECK (DayOfWeek IN ('MON','TUE','WED','THU','FRI','SAT','SUN'));
GO

-- Add index for schedule conflict detection
CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_ScheduleConflict
ON ClassSectionSubjectOffering(RoomId, DayOfWeek, StartTime, EndTime)
WHERE IsActive = 1;
GO

CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_TeacherSchedule
ON ClassSectionSubjectOffering(TeacherId, DayOfWeek, StartTime, EndTime)
WHERE IsActive = 1;
GO
```

**Option B: Normalize Schedule (Recommended)**

```sql
-- Create separate schedule table
CREATE TABLE ClassSchedules
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ClassSectionSubjectOfferingId INT NOT NULL,
    DayOfWeek VARCHAR(3) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_ClassSchedules_Offering 
        FOREIGN KEY (ClassSectionSubjectOfferingId) 
        REFERENCES ClassSectionSubjectOffering(Id),
    CONSTRAINT CHK_ClassSchedules_TimeValidation 
        CHECK (StartTime < EndTime),
    CONSTRAINT CHK_ClassSchedules_DayOfWeek 
        CHECK (DayOfWeek IN ('MON','TUE','WED','THU','FRI','SAT','SUN'))
);
GO

-- Simplify ClassSectionSubjectOffering
ALTER TABLE ClassSectionSubjectOffering
DROP COLUMN DayOfWeek, StartTime, EndTime;
GO

-- Add indexes for conflict detection
CREATE NONCLUSTERED INDEX IX_ClassSchedules_RoomConflict
ON ClassSchedules(DayOfWeek, StartTime, EndTime)
INCLUDE (ClassSectionSubjectOfferingId)
WHERE IsActive = 1;
GO
```

### 5. Create Professor Qualification Validation

```sql
-- This relationship already exists via TeacherSubjects table
-- Add validation function

CREATE FUNCTION dbo.fn_IsProfessorQualified
(
    @TeacherId INT,
    @SubjectId INT
)
RETURNS BIT
AS
BEGIN
    RETURN CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM TeacherSubjects 
            WHERE TeacherId = @TeacherId 
              AND SubjectId = @SubjectId 
              AND IsActive = 1
        ) THEN 1
        ELSE 0
    END;
END;
GO

-- Add computed column to ClassSectionSubjectOffering
ALTER TABLE ClassSectionSubjectOffering
ADD IsTeacherQualified AS dbo.fn_IsProfessorQualified(TeacherId, SubjectId) PERSISTED;
GO

-- Add constraint (optional - if you want to enforce at database level)
-- ALTER TABLE ClassSectionSubjectOffering
-- ADD CONSTRAINT CHK_ClassSectionSubjectOffering_QualifiedTeacher
--     CHECK (IsTeacherQualified = 1);
```

---

## 📋 Data Migration Scripts

### Populate DaysPerWeek and HoursPerDay from Units

```sql
-- Standard mapping: 3 units = 2 days × 1.5 hours or 3 days × 1 hour
UPDATE Subjects
SET 
    DaysPerWeek = CASE 
        WHEN Units = 1 THEN 1
        WHEN Units = 2 THEN 2
        WHEN Units = 3 THEN 2  -- Default to MW or TTh
        WHEN Units = 5 THEN 3  -- MWF
        WHEN Units >= 6 THEN 5 -- MTWTHF
        ELSE 2
    END,
    HoursPerDay = CASE 
        WHEN Units = 1 THEN 1.5
        WHEN Units = 2 THEN 1.5
        WHEN Units = 3 THEN 1.5
        WHEN Units = 5 THEN 1.5
        WHEN Units >= 6 THEN 1.5
        ELSE 1.5
    END,
    PreferredDayPattern = CASE 
        WHEN Units <= 2 THEN 'TTh'
        WHEN Units = 3 THEN 'MW'
        WHEN Units = 5 THEN 'MWF'
        WHEN Units >= 6 THEN 'MTWTHF'
        ELSE 'MW'
    END;
GO

-- Adjust lab courses to require more time
UPDATE s
SET s.HoursPerDay = s.HoursPerDay + 1.0  -- Add lab hour
FROM Subjects s
INNER JOIN Courses c ON s.CourseId = c.Id
INNER JOIN RoomTypes rt ON c.PreferRoomTypeId = rt.Id
WHERE rt.Name LIKE '%Lab%';
GO
```

### Infer RequiresLab from Course Room Preferences

```sql
UPDATE s
SET s.RequiresLab = 1
FROM Subjects s
INNER JOIN Courses c ON s.CourseId = c.Id
INNER JOIN RoomTypes rt ON c.PreferRoomTypeId = rt.Id
WHERE rt.Name IN ('ComputerLab', 'ScienceLab');
GO
```

---

## 🔍 Validation Queries

### Check Subjects Ready for GA

```sql
-- Find subjects missing GA-required fields
SELECT 
    s.Code,
    s.Title,
    s.Units,
    s.DaysPerWeek,
    s.HoursPerDay,
    s.PreferredDayPattern,
    s.RequiresLab,
    CASE 
        WHEN s.DaysPerWeek IS NULL THEN 'Missing DaysPerWeek'
        WHEN s.HoursPerDay IS NULL THEN 'Missing HoursPerDay'
        WHEN s.DaysPerWeek * s.HoursPerDay <> s.Units * 1.5 THEN 'Mismatch: Units vs Schedule'
        ELSE 'OK'
    END AS Status
FROM Subjects s
WHERE s.IsActive = 1;
```

### Check Teacher Qualifications

```sql
-- Find ClassSectionSubjectOfferings with unqualified teachers
SELECT 
    csso.Id,
    c.Code + '-' + cs.Name AS Section,
    s.Code AS SubjectCode,
    s.Title AS SubjectTitle,
    t.FirstName + ' ' + t.LastName AS Teacher,
    'UNQUALIFIED' AS Issue
FROM ClassSectionSubjectOffering csso
INNER JOIN Subjects s ON csso.SubjectId = s.Id
INNER JOIN Teachers t ON csso.TeacherId = t.Id
INNER JOIN ClassSections cs ON csso.ClassSectionId = cs.Id
INNER JOIN Courses c ON cs.CourseId = c.Id
WHERE csso.IsActive = 1
  AND NOT EXISTS (
      SELECT 1 
      FROM TeacherSubjects ts 
      WHERE ts.TeacherId = csso.TeacherId 
        AND ts.SubjectId = csso.SubjectId 
        AND ts.IsActive = 1
  );
```

### Detect Schedule Conflicts

```sql
-- Room conflicts
SELECT 
    csso1.RoomId,
    r.Name AS RoomName,
    csso1.DayOfWeek,
    csso1.StartTime,
    csso1.EndTime,
    COUNT(*) AS ConflictCount
FROM ClassSectionSubjectOffering csso1
INNER JOIN ClassSectionSubjectOffering csso2 
    ON csso1.RoomId = csso2.RoomId
    AND csso1.DayOfWeek = csso2.DayOfWeek
    AND csso1.Id <> csso2.Id
    AND csso1.StartTime < csso2.EndTime
    AND csso1.EndTime > csso2.StartTime
INNER JOIN Rooms r ON csso1.RoomId = r.Id
WHERE csso1.IsActive = 1 AND csso2.IsActive = 1
GROUP BY csso1.RoomId, r.Name, csso1.DayOfWeek, csso1.StartTime, csso1.EndTime
HAVING COUNT(*) > 1;
```

---

## 🎯 Priority Implementation Order

1. **Phase 1 (Critical)** - Required for GA to work:
   - Add `DaysPerWeek`, `HoursPerDay`, `PreferredDayPattern` to Subjects
   - Run data migration scripts
   - Add time validation constraints

2. **Phase 2 (High)** - Improve data quality:
   - Create `SubjectTypes` lookup table
   - Create `DayPatterns` lookup table
   - Add professor qualification validation

3. **Phase 3 (Medium)** - Optimize for scale:
   - Consider normalizing ClassSchedules (Option B)
   - Add conflict detection indexes
   - Create validation functions

---

## 📊 Mapping GA Models to Database

| GA Model | Database Table | Notes |
|----------|----------------|-------|
| `Course` | `Courses` | Direct mapping |
| `Section` | `ClassSections` | Direct mapping |
| `Subject` | `Subjects` | Needs extensions above |
| `Professor` | `Teachers` | Direct mapping |
| `Room` | `Rooms` | Direct mapping |
| `RoomType` | `RoomTypes` | Direct mapping |
| `TimeSlot` | `ClassSectionSubjectOffering` | Time fields |
| `Gene` | `ClassSectionSubjectOffering` | Complete scheduled class |
| `Schedule` | Full set of `ClassSectionSubjectOffering` | Per semester |
| `DayPattern` | `DayPatterns` (new) | Lookup table |
| `SubjectType` | `SubjectTypes` (new) | Lookup table |

---

## 🔗 Integration Points

### From Database to GA

```csharp
// Example: Load subjects from database into GA
var subjects = dbContext.Subjects
    .Include(s => s.Course)
    .Include(s => s.SubjectType)
    .Where(s => s.IsActive && s.CourseId == courseId)
    .Select(s => new Subject(
        s.Id.ToString(),
        s.Code,
        s.Title,
        MapSubjectType(s.SubjectType.Name),
        s.Units,
        s.DaysPerWeek,
        s.HoursPerDay,
        MapDayPattern(s.PreferredDayPattern)
    ))
    .ToList();

// Populate qualified professors
foreach (var subject in subjects)
{
    var qualifiedTeacherIds = dbContext.TeacherSubjects
        .Where(ts => ts.SubjectId == int.Parse(subject.Id) && ts.IsActive)
        .Select(ts => ts.TeacherId.ToString())
        .ToList();
    
    subject.ProfessorIds.AddRange(qualifiedTeacherIds);
}
```

### From GA to Database

```csharp
// Example: Save GA schedule back to database
foreach (var gene in schedule.Genes)
{
    var offering = new ClassSectionSubjectOffering
    {
        SubjectId = int.Parse(gene.Subject.Id),
        TeacherId = int.Parse(gene.ProfessorId),
        ClassSectionId = int.Parse(gene.Section.Id),
        RoomId = int.Parse(gene.Room.Id),
        DayOfWeek = MapDayOfWeekToDb(gene.TimeSlot.Day),
        StartTime = gene.TimeSlot.StartTime,
        EndTime = gene.TimeSlot.EndTime,
        MaxNumberOfStudents = gene.Section.StudentCount,
        IsActive = true
    };
    
    dbContext.ClassSectionSubjectOffering.Add(offering);
}

await dbContext.SaveChangesAsync();
```

---

**Status:** Ready for implementation  
**Next Steps:** 
1. Review and approve schema changes
2. Run Phase 1 migrations in development
3. Test GA integration
4. Deploy to production
