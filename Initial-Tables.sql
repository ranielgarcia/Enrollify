
CREATE TABLE RoomTypes
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL UNIQUE
);


CREATE TABLE Rooms
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	StudentCapacity INT NOT NULL,
	RoomTypeId INT NOT NULL, 
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Rooms_RoomType FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id)
);
GO;



CREATE TABLE Colleges
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL UNIQUE,
	Name VARCHAR(100) NOT NULL,
	Dean VARCHAR(100) NOT NULL, -- Hard coded name for now
	Description VARCHAR(255) NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1
);
GO;

-- ************************************

CREATE TABLE Departments
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
	Name VARCHAR(100) NOT NULL,
	Chairperson VARCHAR(100) NOT NULL, -- Hard coded name for now
	Description VARCHAR(255) NULL,
	CollegeId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Departments_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
	CONSTRAINT UQ_Departments_Code_College UNIQUE (Code, CollegeId)
);
GO;

-- ************************************

CREATE TABLE Courses -- Also known program
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL UNIQUE,
	Name VARCHAR(100) NOT NULL,
	DurationYears INT NOT NULL,
	Description VARCHAR(255) NULL,
	CollegeId INT NOT NULL, -- Or department, but for now use collegeId,
	PreferRoomTypeId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Courses_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
	CONSTRAINT FK_Courses_RoomType FOREIGN KEY (PreferRoomTypeId) REFERENCES RoomTypes(Id)
);
GO;

-- ************************************

CREATE TABLE Subjects
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
	Title VARCHAR(100) NOT NULL,
	Units INT NOT NULL,
	Description VARCHAR(255) NULL,
	CourseId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Subjects_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
	CONSTRAINT UQ_Subjects_Code_Course UNIQUE (Code, CourseId)
);
GO;

-- ************************************

CREATE TABLE SubjectPrerequisites
(
	SourceSubjectId INT NOT NULL,
	PrerequisiteSubjectId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT PK_SubjectPrerequisiteSubjectsMappings PRIMARY KEY(SourceSubjectId, PrerequisiteSubjectId),
	CONSTRAINT FK_SubjectPrerequisites_SourceSubject FOREIGN KEY (SourceSubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_SubjectPrerequisites_PrerequisiteSubject FOREIGN KEY (PrerequisiteSubjectId) REFERENCES Subjects(Id)
);
GO;

--Supports multiple prerequisites subjects per subject
--It only applies the uniqueness check to rows where IsActive = 1.
--You can still insert historical/inactive rows (IsActive = 0), so soft-deletion works.
--Ensures that at most one active mapping per (SourceSubjectId, PrerequisiteSubjectId) exists.
CREATE UNIQUE NONCLUSTERED INDEX UIdx_Subject_PrerequisiteSubject_IsActive
ON SubjectPrerequisites(SourceSubjectId, PrerequisiteSubjectId)
WHERE IsActive = 1;


-- ************************************

-- Links multiple Subjects across Colleges or Department
CREATE TABLE EquivalentSubjectMapping
(
	SourceSubjectId INT NOT NULL,
	EquivalentSubjectId INT NOT NULL,
	Reason VARCHAR(255) NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT PK_EquivalentSubjectMappings PRIMARY KEY(SourceSubjectId, EquivalentSubjectId),
	CONSTRAINT FK_EquivalentSubjectMapping_SourceSubject FOREIGN KEY (SourceSubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_EquivalentSubjectMapping_EquivalentSubject FOREIGN KEY (EquivalentSubjectId) REFERENCES Subjects(Id)
);
GO;

--Supports multiple equivalent subjects per subject
--It only applies the uniqueness check to rows where IsActive = 1.
--You can still insert historical/inactive rows (IsActive = 0), so soft-deletion works.
--Ensures that at most one active mapping per (SourceSubjectId, EquivalentSubjectId) exists.
CREATE UNIQUE NONCLUSTERED INDEX UIdx_Subject_EquivalentSubject_IsActive
ON EquivalentSubjectMapping(SourceSubjectId, EquivalentSubjectId)
WHERE IsActive = 1;

-- ************************************
CREATE TABLE Teachers
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	Email VARCHAR(255) NOT NULL UNIQUE,
	DepartmentId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Teachers_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);
GO;


CREATE TABLE TeacherSubjects
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	TeacherId INT NOT NULL,
	SubjectId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_TeacherSubjects_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id),
	CONSTRAINT FK_TeacherSubjects_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
);
GO;



-- ************************************

CREATE TABLE Semesters
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Semester INT NOT NULL, -- 1 for 1st Semester, 2 for 2nd Semester
	Name VARCHAR(50) NOT NULL,
	Description VARCHAR(50) NOT NULL,
	SchoolYear INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1
);
GO;



-- ************************************

--  “BSCS-2A”, “ENG101-A”
CREATE TABLE ClassSections
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	YearLevel INT NOT NULL,
	CourseId INT NOT NULL,
	SemesterId INT NOT NULL,
	AdviserId INT NOT NULL,
	StudentCapacity INT NOT NULL, -- Soft rule
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_ClassSections_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
	CONSTRAINT FK_ClassSections_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id),
	CONSTRAINT FK_ClassSections_Adviser FOREIGN KEY (AdviserId) REFERENCES Teachers(Id)
);
GO;


--✅ Each subject offering has its own room capacity / group size
--✅ Teachers may have multiple schedules (with different room constraints)
--✅ Irregular students enroll per subject (not per section)
CREATE TABLE ClassSectionSubjectOffering
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	SubjectId INT NOT NULL, -- Belongs to a Subject
	TeacherId INT NOT NULL, -- Assigned to a Teacher,
	ClassSectionId INT NOT NULL, -- Belongs to a ClassSection
	RoomId INT NOT NULL,
	DayOfWeek CHAR(3), -- MON, TUE, WED, THU, FRI, SAT, SUN,
	StartTime TIME,
    EndTime TIME,
	MaxNumberOfStudents INT NULL, -- Optional, soft rule, this to allow us to override the room student capacity
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_ClassSectionSubjectOffering_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_Room FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
);
GO;

-- ====================================
-- STUDENT STATUS LOOKUP TABLE
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

-- Seed data for StudentStatuses
INSERT INTO StudentStatuses (Id, Code, Name, Description, DisplayOrder) VALUES
(1, 'ACTIVE', 'Active', 'Currently enrolled student', 1),
(2, 'INACTIVE', 'Inactive', 'Not currently enrolled but not withdrawn', 2),
(3, 'LOA', 'Leave of Absence', 'Temporarily not attending', 3),
(4, 'GRADUATED', 'Graduated', 'Completed degree requirements', 4),
(5, 'WITHDRAWN', 'Withdrawn', 'Permanently left the institution', 5),
(6, 'SUSPENDED', 'Suspended', 'Temporarily barred from enrollment', 6),
(7, 'EXPELLED', 'Expelled', 'Permanently barred from institution', 7);
GO


-- ************************************

CREATE TABLE Students
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentNumber VARCHAR(13) NOT NULL UNIQUE,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    CourseId INT NOT NULL,
    YearLevel INT NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- References StudentStatuses, default to ACTIVE
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Students_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
    CONSTRAINT FK_Students_StudentStatus FOREIGN KEY (Status) REFERENCES StudentStatuses(Id)
);
GO

-- ************************************

-- SectionId is optional
-- Regular students - auto-selects Section-based offerings
-- Irregular students - Manual selection of ANY open subject offerings
CREATE TABLE Enrollments
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    ClassSectionId INT NULL, -- optional, null for irregular students
    ClassSectionSubjectOfferingId INT NOT NULL,
    SemesterId INT NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- References EnrollmentStatuses, default to PENDING
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Enrollments_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id),
    CONSTRAINT FK_Enrollments_ClassSectionSubjectOffering FOREIGN KEY (ClassSectionSubjectOfferingId) REFERENCES ClassSectionSubjectOffering(Id),
    CONSTRAINT FK_Enrollments_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id),
    CONSTRAINT FK_Enrollments_EnrollmentStatus FOREIGN KEY (Status) REFERENCES EnrollmentStatuses(Id)
);
GO

-- ====================================
-- ENROLLMENT STATUS LOOKUP TABLE
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

-- Seed data for EnrollmentStatuses
INSERT INTO EnrollmentStatuses (Id, Code, Name, Description, DisplayOrder) VALUES
(1, 'PENDING', 'Pending', 'Awaiting approval', 1),
(2, 'APPROVED', 'Approved', 'Ready for enrollment', 2),
(3, 'ENROLLED', 'Enrolled', 'Currently enrolled in classes', 3),
(4, 'COMPLETED', 'Completed', 'Semester completed', 4),
(5, 'DROPPED', 'Dropped', 'Student dropped enrollment', 5),
(6, 'FAILED', 'Failed', 'Did not meet requirements', 6),
(7, 'CANCELLED', 'Cancelled', 'Enrollment cancelled by system', 7);
GO



-- ************************************

-- Linked indirectly to a Student, Subject, and Teacher
CREATE TABLE EnrollmentAcademicRecords
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	EnrollmentId INT NOT NULL,
	MidtermGrade DECIMAL(5, 2),
	FinalGrade DECIMAL(5, 2),
	Remarks VARCHAR(255),
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_EnrollmentAcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id)
);
GO;


CREATE TABLE EnrollmentPayments
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	EnrollmentId INT NOT NULL,
	Amount DECIMAL(10, 2) NOT NULL,
	PaymentDate DATETIME2 NOT NULL,
	PaymentMethod VARCHAR(50) NOT NULL,
	ReferenceNumber VARCHAR(100) NULL,
	PaymentStatus VARCHAR(50) NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_EnrollmentPayments_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id)
);
GO;

-- ************************************
-- INDEXES ON FOREIGN KEYS FOR PERFORMANCE
-- ************************************

-- Rooms indexes
CREATE NONCLUSTERED INDEX IX_Rooms_RoomTypeId 
ON Rooms(RoomTypeId);
GO

-- Departments indexes
CREATE NONCLUSTERED INDEX IX_Departments_CollegeId 
ON Departments(CollegeId);
GO

-- Courses indexes
CREATE NONCLUSTERED INDEX IX_Courses_CollegeId 
ON Courses(CollegeId);
GO

CREATE NONCLUSTERED INDEX IX_Courses_PreferRoomTypeId 
ON Courses(PreferRoomTypeId);
GO

-- Subjects indexes
CREATE NONCLUSTERED INDEX IX_Subjects_CourseId 
ON Subjects(CourseId);
GO

-- SubjectPrerequisites indexes
CREATE NONCLUSTERED INDEX IX_SubjectPrerequisites_SourceSubjectId 
ON SubjectPrerequisites(SourceSubjectId);
GO

CREATE NONCLUSTERED INDEX IX_SubjectPrerequisites_PrerequisiteSubjectId 
ON SubjectPrerequisites(PrerequisiteSubjectId);
GO

-- EquivalentSubjectMapping indexes
CREATE NONCLUSTERED INDEX IX_EquivalentSubjectMapping_SourceSubjectId 
ON EquivalentSubjectMapping(SourceSubjectId);
GO

CREATE NONCLUSTERED INDEX IX_EquivalentSubjectMapping_EquivalentSubjectId 
ON EquivalentSubjectMapping(EquivalentSubjectId);
GO

-- Teachers indexes
CREATE NONCLUSTERED INDEX IX_Teachers_DepartmentId 
ON Teachers(DepartmentId);
GO

-- TeacherSubjects indexes
CREATE NONCLUSTERED INDEX IX_TeacherSubjects_TeacherId 
ON TeacherSubjects(TeacherId);
GO

CREATE NONCLUSTERED INDEX IX_TeacherSubjects_SubjectId 
ON TeacherSubjects(SubjectId);
GO

-- ClassSections indexes
CREATE NONCLUSTERED INDEX IX_ClassSections_CourseId 
ON ClassSections(CourseId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSections_SemesterId 
ON ClassSections(SemesterId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSections_AdviserId 
ON ClassSections(AdviserId);
GO

-- ClassSectionSubjectOffering indexes
CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_SubjectId 
ON ClassSectionSubjectOffering(SubjectId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_TeacherId 
ON ClassSectionSubjectOffering(TeacherId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_ClassSectionId 
ON ClassSectionSubjectOffering(ClassSectionId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSectionSubjectOffering_RoomId 
ON ClassSectionSubjectOffering(RoomId);
GO

-- Students indexes
CREATE NONCLUSTERED INDEX IX_Students_CourseId 
ON Students(CourseId);
GO

CREATE NONCLUSTERED INDEX IX_Students_Status 
ON Students(Status);
GO

-- Enrollments indexes
CREATE NONCLUSTERED INDEX IX_Enrollments_StudentId 
ON Enrollments(StudentId);
GO

CREATE NONCLUSTERED INDEX IX_Enrollments_ClassSectionId 
ON Enrollments(ClassSectionId);
GO

CREATE NONCLUSTERED INDEX IX_Enrollments_ClassSectionSubjectOfferingId 
ON Enrollments(ClassSectionSubjectOfferingId);
GO

CREATE NONCLUSTERED INDEX IX_Enrollments_SemesterId 
ON Enrollments(SemesterId);
GO

CREATE NONCLUSTERED INDEX IX_Enrollments_Status 
ON Enrollments(Status);
GO

-- EnrollmentAcademicRecords indexes
CREATE NONCLUSTERED INDEX IX_EnrollmentAcademicRecords_EnrollmentId 
ON EnrollmentAcademicRecords(EnrollmentId);
GO

-- EnrollmentPayments indexes
CREATE NONCLUSTERED INDEX IX_EnrollmentPayments_EnrollmentId 
ON EnrollmentPayments(EnrollmentId);
GO