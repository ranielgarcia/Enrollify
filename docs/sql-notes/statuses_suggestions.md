Based on my analysis of your enrollment system and the genetic algorithm project context, here's my recommendation for the Status field inconsistency:

Recommended Approach: INT with Lookup Tables
Why INT over VARCHAR?
Performance: INT comparisons are faster than string comparisons
Storage: INT (4 bytes) vs VARCHAR(25) (25+ bytes overhead)
Type Safety: Prevents typos like "Pendng" or "APPROVED" vs "Approved"
Database Consistency: Aligns with your Enrollments.Status INT pattern
Future-Proof: Easy to add new statuses without schema changes

Implementation Strategy

```sql
-- ====================================
-- Student Status Lookup Table
-- ====================================
CREATE TABLE StudentStatuses
(
    Id INT NOT NULL PRIMARY KEY,
    Code VARCHAR(20) NOT NULL UNIQUE,
    Name VARCHAR(50) NOT NULL,
    Description VARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    
    CONSTRAINT CHK_StudentStatuses_Code_NotEmpty CHECK (LEN(TRIM(Code)) > 0),
    CONSTRAINT CHK_StudentStatuses_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0)
);
GO

-- Seed data
INSERT INTO StudentStatuses (Id, Code, Name, Description, DisplayOrder) VALUES
(1, 'ACTIVE', 'Active', 'Currently enrolled student', 1),
(2, 'INACTIVE', 'Inactive', 'Not currently enrolled but not withdrawn', 2),
(3, 'LOA', 'Leave of Absence', 'Temporarily not attending', 3),
(4, 'GRADUATED', 'Graduated', 'Completed degree requirements', 4),
(5, 'WITHDRAWN', 'Withdrawn', 'Permanently left the institution', 5),
(6, 'SUSPENDED', 'Suspended', 'Temporarily barred from enrollment', 6),
(7, 'EXPELLED', 'Expelled', 'Permanently barred from institution', 7);
GO

-- ====================================
-- Enrollment Status Lookup Table
-- ====================================
CREATE TABLE EnrollmentStatuses
(
    Id INT NOT NULL PRIMARY KEY,
    Code VARCHAR(20) NOT NULL UNIQUE,
    Name VARCHAR(50) NOT NULL,
    Description VARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    
    CONSTRAINT CHK_EnrollmentStatuses_Code_NotEmpty CHECK (LEN(TRIM(Code)) > 0),
    CONSTRAINT CHK_EnrollmentStatuses_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0)
);
GO

-- Seed data
INSERT INTO EnrollmentStatuses (Id, Code, Name, Description, DisplayOrder) VALUES
(1, 'PENDING', 'Pending', 'Awaiting approval', 1),
(2, 'APPROVED', 'Approved', 'Ready for enrollment', 2),
(3, 'ENROLLED', 'Enrolled', 'Currently enrolled in classes', 3),
(4, 'COMPLETED', 'Completed', 'Semester completed', 4),
(5, 'DROPPED', 'Dropped', 'Student dropped enrollment', 5),
(6, 'FAILED', 'Failed', 'Did not meet requirements', 6),
(7, 'CANCELLED', 'Cancelled', 'Enrollment cancelled by system', 7);
GO

-- ====================================
-- Modify Students Table
-- ====================================
ALTER TABLE Students
ALTER COLUMN Status INT NOT NULL;
GO

ALTER TABLE Students
ADD CONSTRAINT FK_Students_StudentStatus 
    FOREIGN KEY (Status) REFERENCES StudentStatuses(Id);
GO

CREATE NONCLUSTERED INDEX IX_Students_Status 
ON Students(Status);
GO

-- Set default to ACTIVE (Id = 1)
ALTER TABLE Students
ADD CONSTRAINT DF_Students_Status DEFAULT 1 FOR Status;
GO

-- ====================================
-- Modify Enrollments Table (already INT, just add FK)
-- ====================================
ALTER TABLE Enrollments
ADD CONSTRAINT FK_Enrollments_EnrollmentStatus 
    FOREIGN KEY (Status) REFERENCES EnrollmentStatuses(Id);
GO

CREATE NONCLUSTERED INDEX IX_Enrollments_Status 
ON Enrollments(Status);
GO

-- Set default to PENDING (Id = 1)
ALTER TABLE Enrollments
ADD CONSTRAINT DF_Enrollments_Status DEFAULT 1 FOR Status;
GO
```

Usage Examples

```sql
-- Query with readable status names
SELECT 
    s.StudentNumber,
    s.FirstName + ' ' + s.LastName AS FullName,
    st.Name AS Status,
    c.Name AS Course
FROM Students s
INNER JOIN StudentStatuses st ON s.Status = st.Id
INNER JOIN Courses c ON s.CourseId = c.Id
WHERE st.Code = 'ACTIVE';

-- Enrollment report with status breakdown
SELECT 
    es.Name AS EnrollmentStatus,
    COUNT(*) AS Count
FROM Enrollments e
INNER JOIN EnrollmentStatuses es ON e.Status = es.Id
GROUP BY es.Name, es.DisplayOrder
ORDER BY es.DisplayOrder;

-- Application layer usage (C#)
public enum StudentStatus
{
    Active = 1,
    Inactive = 2,
    LeaveOfAbsence = 3,
    Graduated = 4,
    Withdrawn = 5,
    Suspended = 6,
    Expelled = 7
}

public enum EnrollmentStatus
{
    Pending = 1,
    Approved = 2,
    Enrolled = 3,
    Completed = 4,
    Dropped = 5,
    Failed = 6,
    Cancelled = 7
}
```

Benefits for Your GA System

1. Quick Status Filtering: GA can efficiently query active students/enrollments
2. Constraint Validation: Prevents invalid status assignments during schedule generation
3. Reporting: Easy to generate enrollment statistics by status
4. Audit Trail: Status transitions can be logged (add StudentStatusHistory table later)

Alternative: CHECK Constraint (If you prefer VARCHAR)

If you must keep VARCHAR for legacy reasons:

```sql
ALTER TABLE Students
ADD CONSTRAINT CHK_Students_Status 
CHECK (Status IN ('ACTIVE', 'INACTIVE', 'LOA', 'GRADUATED', 'WITHDRAWN', 'SUSPENDED', 'EXPELLED'));

ALTER TABLE Students
ADD CONSTRAINT DF_Students_Status DEFAULT 'ACTIVE' FOR Status;
```

My Recommendation: Go with INT + Lookup Tables
It provides the best balance of:

✅ Performance for queries/joins
✅ Data integrity with foreign keys
✅ Flexibility to add statuses without schema changes
✅ Consistency with existing Enrollments.Status INT pattern
✅ Better integration with your C# genetic algorithm code
Would you like me to create the migration script or apply it directly to your Initial-Tables.sql?