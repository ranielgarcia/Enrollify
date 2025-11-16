# SQL Schema Validation and Suggestions

**Date:** November 11, 2025  
**File:** Initial-Tables.sql  
**Database:** Microsoft SQL Server

---

## ✅ Overall Assessment

The SQL schema is **syntactically valid** and follows good practices for SQL Server. The database design is well-structured for an enrollment system with proper normalization and relationships.

---

## 🔍 Issues Found

### 1. **Foreign Key Naming Conflicts** (Critical)

**Issue:** Multiple tables use the same foreign key constraint name `FK_Course`, causing conflicts.

**Affected Tables:**
- `Departments` table: `CONSTRAINT FK_College FOREIGN KEY (CollegeId)`
- `Courses` table: `CONSTRAINT FK_College FOREIGN KEY (CollegeId)`
- `Subjects` table: `CONSTRAINT FK_Course FOREIGN KEY (CourseId)`
- `Students` table: `CONSTRAINT FK_Course FOREIGN KEY (CourseId)`
- `ClassSections` table: `CONSTRAINT FK_Course FOREIGN KEY (CourseId)`

**Impact:** SQL Server will throw an error when creating the second constraint with the same name.

**Solution:** Use unique, descriptive constraint names following pattern: `FK_TableName_ReferencedTable`

```sql
-- Example fixes:
CONSTRAINT FK_Departments_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id)
CONSTRAINT FK_Courses_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id)
CONSTRAINT FK_Subjects_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
CONSTRAINT FK_Students_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
CONSTRAINT FK_ClassSections_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
```

### 2. **Incomplete Table: EnrollmentPayments**

**Issue:** The `EnrollmentPayments` table only has `Id`, `EnrollmentId`, and audit fields but no payment-related columns.

**Missing Fields:**
- Amount
- PaymentDate
- PaymentMethod
- TransactionReference
- PaymentStatus

**Recommendation:** Add necessary payment columns or remove if not yet designed.

### 3. **Missing Indexes on Foreign Keys**

**Issue:** Foreign key columns lack non-clustered indexes, which can severely impact query performance.

**Affected Columns:**
- `Rooms.RoomTypeId`
- `Departments.CollegeId`
- `Courses.CollegeId`, `Courses.PreferRoomTypeId`
- `Subjects.CourseId`
- `SubjectPrerequisites.SourceSubjectId`, `SubjectPrerequisites.PrerequisiteSubjectId`
- `EquivalentSubjectMapping.SourceSubjectId`, `EquivalentSubjectMapping.EquivalentSubjectId`
- `Teachers.DepartmentId`
- `TeacherSubjects.TeacherId`, `TeacherSubjects.SubjectId`
- `ClassSections.CourseId`, `ClassSections.SemesterId`, `ClassSections.AdviserId`
- `ClassSectionSubjectOffering.SubjectId`, `ClassSectionSubjectOffering.TeacherId`, `ClassSectionSubjectOffering.ClassSectionId`, `ClassSectionSubjectOffering.RoomId`
- `Students.CourseId`
- `Enrollments.StudentId`, `Enrollments.ClassSectionId`, `Enrollments.ClassSectionSubjectOfferingId`, `Enrollments.SemesterId`
- `EnrollmentAcademicRecords.EnrollmentId`
- `EnrollmentPayments.EnrollmentId`

### 4. **Missing Unique Constraints**

**Issue:** Several columns that should be unique lack constraints.

**Recommendations:**
- `RoomTypes.Name` - Should be unique
- `Colleges.Code` - Should be unique
- `Departments.Code` - Should be unique per college
- `Courses.Code` - Should be unique
- `Subjects.Code` - Should be unique per course
- `Teachers.Email` - Should be unique
- `Students.StudentNumber` - Should be unique
- `Students.Email` - Should be unique

### 5. **Data Type Issues**

**a) Email Fields Too Short**
```sql
Email VARCHAR(50) -- Modern emails can exceed 50 characters
```
**Recommendation:** Use `VARCHAR(255)` for email fields.

**b) Grade Precision**
```sql
MidtermGrade DECIMAL(3, 2) -- Max value: 9.99
FinalGrade DECIMAL(3, 2)   -- Max value: 9.99
```
**Issue:** If grades range 0-100, this won't work. If 0-5, needs DECIMAL(3,2) is fine but max is 9.99.

**Recommendation:** Use `DECIMAL(5, 2)` for 0-100 scale or clarify grading scale.

**c) Status as VARCHAR**
```sql
Students.Status VARCHAR(25) -- TBD
Enrollments.Status INT      -- Enum: Pending, Approved, Enrolled
```
**Inconsistency:** One uses VARCHAR, other uses INT for similar purpose.

**Recommendation:** Use consistent approach (preferably INT with lookup table or CHECK constraint).

### 6. **Missing CHECK Constraints**

**Recommended Validations:**
- `Rooms.StudentCapacity` > 0
- `Courses.DurationYears` > 0 AND <= 10
- `Subjects.Units` > 0 AND <= 12
- `ClassSections.YearLevel` BETWEEN 1 AND 6
- `ClassSections.StudentCapacity` > 0
- `Students.YearLevel` BETWEEN 1 AND 6
- `Semesters.Semester` IN (1, 2, 3)
- `Semesters.SchoolYear` >= 2000
- `EnrollmentAcademicRecords.MidtermGrade` BETWEEN 0 AND 100
- `EnrollmentAcademicRecords.FinalGrade` BETWEEN 0 AND 100

### 7. **ClassSectionSubjectOffering Schedule Issues**

**Issue:** The design stores only one day/time per row, requiring multiple rows per subject for multi-day schedules.

**Current Design:**
```sql
DayOfWeek CHAR(3),  -- MON, TUE, WED, THU, FRI, SAT, SUN
StartTime TIME,
EndTime TIME,
```

**Problems:**
- No validation for valid day codes
- Difficult to query for schedule conflicts
- No pattern validation (MW, TTh, MWF)

**Recommendations:**
- Add CHECK constraint: `DayOfWeek IN ('MON','TUE','WED','THU','FRI','SAT','SUN')`
- Add CHECK constraint: `StartTime < EndTime`
- Consider separate `ClassSchedules` table for better normalization
- Add `DaysPerWeek` and `HoursPerDay` columns (as per your genetic algorithm needs)

### 8. **Missing Self-Referential Check in SubjectPrerequisites**

**Issue:** No prevention of circular prerequisites or self-references.

**Example Bad Data:**
- Subject A requires Subject B
- Subject B requires Subject A (circular)
- Subject A requires Subject A (self-reference)

**Recommendation:** Add CHECK constraint:
```sql
ALTER TABLE SubjectPrerequisites
ADD CONSTRAINT CHK_NoSelfReference 
CHECK (SourceSubjectId <> PrerequisiteSubjectId);
```

**Note:** Circular dependency detection requires triggers or application logic.

### 9. **Missing Default Values**

**Recommendations:**
- `Enrollments.Status` - Should default to Pending (e.g., 0 or 1)
- `ClassSectionSubjectOffering.MaxNumberOfStudents` - Consider defaulting to Room capacity

---

## 📋 Enhancement Suggestions

### 1. **Add Audit Trail Enhancement**

Current audit columns are basic. Consider:
- `CreatedBy` (INT, references Users)
- `UpdatedBy` (INT, references Users)
- `DeletedAt` (DATETIME2, for soft deletes)
- `DeletedBy` (INT, references Users)

### 2. **Add Versioning to Subjects**

For curriculum changes over time:
```sql
ALTER TABLE Subjects
ADD EffectiveDate DATETIME2,
    VersionNumber INT DEFAULT 1;
```

### 3. **Create Lookup Tables**

Instead of hardcoded enums, create:
- `EnrollmentStatuses` table
- `StudentStatuses` table
- `DayOfWeek` lookup table

### 4. **Add Computed Columns**

```sql
-- Students full name
ALTER TABLE Students
ADD FullName AS (FirstName + ' ' + LastName) PERSISTED;

-- Teachers full name
ALTER TABLE Teachers
ADD FullName AS (FirstName + ' ' + LastName) PERSISTED;
```

### 5. **Add Scheduling Integration Fields**

For genetic algorithm integration, add to `ClassSectionSubjectOffering`:
```sql
ALTER TABLE ClassSectionSubjectOffering
ADD DaysPerWeek INT,
    HoursPerDay DECIMAL(3,1),
    PreferredDayPattern VARCHAR(10); -- 'MW', 'TTh', 'MWF'
```

### 6. **Add Cascade Delete Considerations**

Currently, no cascade rules specified. Consider:
- Courses → Subjects: `ON DELETE RESTRICT` (protect data)
- ClassSections → Enrollments: `ON DELETE RESTRICT` (protect enrolled students)
- Rooms → ClassSectionSubjectOffering: `ON DELETE RESTRICT` (protect scheduled classes)

### 7. **Add Table-Level Documentation**

Use SQL Server extended properties:
```sql
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Stores course offerings with room and schedule assignments',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE', @level1name = 'ClassSectionSubjectOffering';
```

---

## 🎯 Priority Action Items

### High Priority (Must Fix)
1. ✅ **Fix duplicate foreign key constraint names** - Will cause deployment failure
2. ✅ **Add unique constraints on business keys** (StudentNumber, Email, Codes)
3. ✅ **Add indexes on all foreign keys** - Critical for performance

### Medium Priority (Should Fix)
4. ✅ **Add CHECK constraints for data validation**
5. ✅ **Increase Email field sizes to VARCHAR(255)**
6. ✅ **Complete EnrollmentPayments table design**
7. ✅ **Add self-reference check on SubjectPrerequisites**

### Low Priority (Nice to Have)
8. ✅ **Add computed columns for full names**
9. ✅ **Create lookup tables for status enums**
10. ✅ **Add extended properties for documentation**
11. ✅ **Consider audit trail enhancements**

---

## 📝 Example Corrected Table (Departments)

```sql
CREATE TABLE Departments
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(10) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Chairperson VARCHAR(100) NOT NULL,
    Description VARCHAR(255) NULL,
    CollegeId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Fixed constraint name
    CONSTRAINT FK_Departments_College 
        FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
    
    -- Added unique constraint
    CONSTRAINT UQ_Departments_Code_College 
        UNIQUE (Code, CollegeId),
    
    -- Added check constraint
    CONSTRAINT CHK_Departments_Code_NotEmpty 
        CHECK (LEN(TRIM(Code)) > 0)
);
GO

-- Added index on foreign key
CREATE NONCLUSTERED INDEX IX_Departments_CollegeId 
ON Departments(CollegeId);
GO
```

---

## 🔗 Related to Genetic Algorithm Project

### Alignment with ScheduleData.cs

Your genetic algorithm uses:
- `DaysPerWeek` - Not in database
- `HoursPerDay` - Not in database  
- `PreferredDayPattern` - Not in database
- `RequiresLab` - Not in database (could use RoomType)

**Recommendation:** Extend `Subjects` table to include GA-required fields:
```sql
ALTER TABLE Subjects
ADD DaysPerWeek INT DEFAULT 2,
    HoursPerDay DECIMAL(3,1) DEFAULT 1.5,
    PreferredDayPattern VARCHAR(10) NULL, -- 'MW', 'TTh', 'MWF'
    RequiresLab BIT DEFAULT 0;
```

---

## 📚 Additional Resources

- SQL Server Naming Conventions: [Microsoft Docs](https://docs.microsoft.com/sql/)
- Index Design Guidelines: Query performance optimization
- Soft Delete Patterns: IsActive flag implementation best practices
- Foreign Key Indexing: Performance impact analysis

---

**Generated by:** GitHub Copilot  
**Review Status:** Pending manual review  
**Next Steps:** Apply critical fixes before database deployment
