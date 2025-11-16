# Critical SQL Fixes Required

**Date:** November 11, 2025  
**Priority:** HIGH - Must fix before deployment

---

## 🚨 CRITICAL: Duplicate Foreign Key Constraint Names

**Error:** SQL Server will fail on duplicate constraint names.

### Tables with Conflicts:

#### FK_College (appears 2 times)
```sql
-- In Departments table:
CONSTRAINT FK_Departments_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id)

-- In Courses table:
CONSTRAINT FK_Courses_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id)
```

#### FK_Course (appears 3 times)
```sql
-- In Subjects table:
CONSTRAINT FK_Subjects_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)

-- In Students table:
CONSTRAINT FK_Students_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)

-- In ClassSections table:
CONSTRAINT FK_ClassSections_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
```

#### FK_Subject (appears 3 times)
```sql
-- In SubjectPrerequisites - both columns need unique names
-- In EquivalentSubjectMapping - both columns need unique names
-- In TeacherSubjects table:
CONSTRAINT FK_TeacherSubjects_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id)

-- In ClassSectionSubjectOffering table:
CONSTRAINT FK_ClassSectionSubjectOffering_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id)
```

#### FK_Teacher (appears 2 times)
```sql
-- In TeacherSubjects table:
CONSTRAINT FK_TeacherSubjects_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id)

-- In ClassSectionSubjectOffering table:
CONSTRAINT FK_ClassSectionSubjectOffering_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id)
```

#### FK_Semester (appears 2 times)
```sql
-- In ClassSections table:
CONSTRAINT FK_ClassSections_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id)

-- In Enrollments table:
CONSTRAINT FK_Enrollments_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id)
```

#### FK_ClassSection (appears 2 times)
```sql
-- In ClassSectionSubjectOffering table:
CONSTRAINT FK_ClassSectionSubjectOffering_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id)

-- In Enrollments table:
CONSTRAINT FK_Enrollments_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id)
```

#### FK_Enrollment (appears 2 times)
```sql
-- In EnrollmentAcademicRecords table:
CONSTRAINT FK_EnrollmentAcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id)

-- In EnrollmentPayments table:
CONSTRAINT FK_EnrollmentPayments_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id)
```

#### FK_Room (appears in ClassSectionSubjectOffering)
```sql
CONSTRAINT FK_ClassSectionSubjectOffering_Room FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
```

---

## ✅ Quick Fix Script

Run this script to generate corrected foreign key names:

```sql
-- Pattern: FK_{TableName}_{ReferencedTable}

-- Use this naming convention:
-- FK_Departments_College
-- FK_Courses_College
-- FK_Subjects_Course
-- FK_Students_Course
-- FK_ClassSections_Course
-- FK_ClassSections_Semester
-- FK_ClassSections_Adviser
-- FK_TeacherSubjects_Teacher
-- FK_TeacherSubjects_Subject
-- FK_ClassSectionSubjectOffering_Subject
-- FK_ClassSectionSubjectOffering_Teacher
-- FK_ClassSectionSubjectOffering_ClassSection
-- FK_ClassSectionSubjectOffering_Room
-- FK_Enrollments_Student
-- FK_Enrollments_ClassSection
-- FK_Enrollments_ClassSectionSubjectOffering
-- FK_Enrollments_Semester
-- FK_EnrollmentAcademicRecords_Enrollment
-- FK_EnrollmentPayments_Enrollment
```

---

## 📋 Complete List of Required Constraint Renames

| Table | Old Name | New Name |
|-------|----------|----------|
| Rooms | `FK_Type` | `FK_Rooms_RoomType` |
| Departments | `FK_College` | `FK_Departments_College` |
| Courses | `FK_College` | `FK_Courses_College` |
| Courses | `FK_PreferRoomType` | `FK_Courses_PreferRoomType` |
| Subjects | `FK_Course` | `FK_Subjects_Course` |
| Teachers | `FK_Department` | `FK_Teachers_Department` |
| TeacherSubjects | `FK_Teacher` | `FK_TeacherSubjects_Teacher` |
| TeacherSubjects | `FK_Subject` | `FK_TeacherSubjects_Subject` |
| ClassSections | `FK_Course` | `FK_ClassSections_Course` |
| ClassSections | `FK_Semester` | `FK_ClassSections_Semester` |
| ClassSections | `FK_Adviser` | `FK_ClassSections_Adviser` |
| ClassSectionSubjectOffering | `FK_Subject` | `FK_ClassSectionSubjectOffering_Subject` |
| ClassSectionSubjectOffering | `FK_Teacher` | `FK_ClassSectionSubjectOffering_Teacher` |
| ClassSectionSubjectOffering | `FK_ClassSection` | `FK_ClassSectionSubjectOffering_ClassSection` |
| ClassSectionSubjectOffering | `FK_Room` | `FK_ClassSectionSubjectOffering_Room` |
| Students | `FK_Course` | `FK_Students_Course` |
| Enrollments | `FK_Student` | `FK_Enrollments_Student` |
| Enrollments | `FK_ClassSection` | `FK_Enrollments_ClassSection` |
| Enrollments | `FK_ClassSectionSubjectOffering` | `FK_Enrollments_ClassSectionSubjectOffering` |
| Enrollments | `FK_Semester` | `FK_Enrollments_Semester` |
| EnrollmentAcademicRecords | `FK_Enrollment` | `FK_EnrollmentAcademicRecords_Enrollment` |
| EnrollmentPayments | `FK_Enrollment` | `FK_EnrollmentPayments_Enrollment` |

---

## 🎯 Action Plan

1. **Find & Replace** each constraint name in Initial-Tables.sql
2. **Test** the script in a development environment
3. **Deploy** to production after validation

**Estimated Time:** 15-20 minutes

---

## 🔍 How to Find Issues in Your Script

```bash
# Search for duplicate constraint names
grep -o "CONSTRAINT FK_[A-Za-z]*" Initial-Tables.sql | sort | uniq -d
```

Or in SQL Server Management Studio:
- Use Find All (Ctrl+Shift+F)
- Search for: `CONSTRAINT FK_`
- Review each occurrence

---

**Status:** ❌ BLOCKING - Script will not execute until fixed  
**Next Steps:** Update Initial-Tables.sql with corrected constraint names
