
CREATE TABLE RoomTypes
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL
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
	CONSTRAINT FK_Room_Type FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id)
);
GO;



CREATE TABLE Colleges
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
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
	CONSTRAINT FK_Departments_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id)
);
GO;

-- ************************************

CREATE TABLE Courses -- Also known program
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
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
	CONSTRAINT FK_Students_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
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
	Email VARCHAR(50) NOT NULL,
	DepartmentId INT NOT NULL,
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
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


-- ************************************

CREATE TABLE Students
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	StudentNumber VARCHAR(13) NOT NULL,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	Email VARCHAR(50) NOT NULL,
	CourseId INT NOT NULL,
	YearLevel INT NOT NULL,
	Status VARCHAR(25), -- TBD
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Students_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);
GO;


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
	Status INT NOT NULL, -- Enum: Pending, Approved, Enrolled
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedAt DATETIME2 NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId) REFERENCES Students(Id),
	CONSTRAINT FK_Enrollments_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id),
	CONSTRAINT FK_Enrollments_ClassSectionSubjectOffering FOREIGN KEY (ClassSectionSubjectOfferingId) REFERENCES ClassSectionSubjectOffering(Id),
	CONSTRAINT FK_Enrollments_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id)
);
GO;

-- ************************************

-- Linked indirectly to a Student, Subject, and Teacher
CREATE TABLE EnrollmentAcademicRecords
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	EnrollmentId INT NOT NULL,
	MidtermGrade DECIMAL(3, 2),
	FinalGrade DECIMAL(3, 2),
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