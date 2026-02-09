CREATE TABLE RoomTypes
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	Description VARCHAR(255),

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_RoomTypes_Name UNIQUE (Name),
	CONSTRAINT FK_RoomTypes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_RoomTypes_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_RoomTypes_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);


CREATE TABLE Colleges
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
	Name VARCHAR(100) NOT NULL,
	Dean VARCHAR(100) NOT NULL, -- Hard coded name for now
	Description TEXT,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_Colleges_Code UNIQUE (Code),
	CONSTRAINT UQ_Colleges_Name UNIQUE (Name),
	CONSTRAINT FK_Colleges_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Colleges_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Colleges_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO;


CREATE TABLE Buildings
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	Description VARCHAR(255),
	Address VARCHAR(255),
	CollegeId INT NOT NULL,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_Buildings_Name UNIQUE (Name),
	CONSTRAINT FK_Buildings_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
	CONSTRAINT FK_Buildings_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Buildings_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Buildings_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);


CREATE TABLE Rooms
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	RoomNumber VARCHAR(50) NOT NULL,
	Capacity INT NOT NULL,
	RoomTypeId INT NOT NULL,
	BuildingId INT NOT NULL,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Rooms_RoomType FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id),
	CONSTRAINT FK_Rooms_Building FOREIGN KEY (BuildingId) REFERENCES Buildings(Id),
	CONSTRAINT FK_Rooms_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Rooms_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Rooms_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_Rooms_Capacity_Positive CHECK (Capacity > 0)
);
GO;

-- Rooms indexes
CREATE NONCLUSTERED INDEX IX_Rooms_RoomTypeId 
ON Rooms(RoomTypeId);
GO


CREATE UNIQUE NONCLUSTERED INDEX UIdx_Rooms_RoomNumber_Building_IsActive
ON Rooms(RoomNumber, BuildingId)
WHERE IsActive = 1;
GO

-- ************************************

CREATE TABLE Departments
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
	Name VARCHAR(100) NOT NULL,
	Chairperson VARCHAR(100) NOT NULL, -- Hard coded name for now
	Description VARCHAR(255) NULL,
	CollegeId INT NOT NULL,
	
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Departments_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
	CONSTRAINT FK_Departments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Departments_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Departments_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	--CONSTRAINT UQ_Departments_Code_College UNIQUE (Code, CollegeId)
);
GO;

CREATE UNIQUE NONCLUSTERED INDEX UIdx_Departments_Code_College_IsActive
ON Departments(Code, CollegeId)
WHERE IsActive = 1;
GO

CREATE UNIQUE NONCLUSTERED INDEX UIdx_Departments_Name_College_IsActive
ON Departments(Name, CollegeId)
WHERE IsActive = 1;
GO

-- Departments indexes
CREATE NONCLUSTERED INDEX IX_Departments_CollegeId 
ON Departments(CollegeId);
GO

-- ************************************


CREATE TABLE Courses -- Also known program
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(10) NOT NULL,
	Name VARCHAR(100) NOT NULL,
	DurationYears INT NOT NULL,
	Description TEXT NULL,
	CollegeId INT NOT NULL, -- Or department, but for now use collegeId,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_Courses_Code UNIQUE (Code),
	CONSTRAINT FK_Courses_College FOREIGN KEY (CollegeId) REFERENCES Colleges(Id),
	CONSTRAINT FK_Courses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Courses_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Courses_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_Courses_DurationYears_Valid CHECK (DurationYears > 0 AND DurationYears <= 10)
);
GO;

CREATE UNIQUE NONCLUSTERED INDEX UIdx_Course_Code_College_IsActive
ON Courses(Code, CollegeId)
WHERE IsActive = 1;
GO

CREATE UNIQUE NONCLUSTERED INDEX UIdx_Course_Name_College_IsActive
ON Courses(Name, CollegeId)
WHERE IsActive = 1;
GO


-- Courses indexes
CREATE NONCLUSTERED INDEX IX_Courses_CollegeId 
ON Courses(CollegeId);
GO

-- ************************************

CREATE TABLE CurriculumStatuses
(
	Id INT NOT NULL PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	CONSTRAINT UQ_CurriculumStatuses_Name UNIQUE (Name),
);
GO

-- Curriculum versioning - each course can have multiple curriculum versions
-- Students are assigned to a curriculum when they enroll
-- Prerequisites are defined at the curriculum level, not the subject level
CREATE TABLE Curriculums
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	CourseId INT NOT NULL,
	EffectiveYear INT NOT NULL,           -- Academic year when this curriculum takes effect (e.g., 2024)
	Version VARCHAR(20) NOT NULL,          -- Version identifier (e.g., '2024-A', '2024-REV1')
	StatusId INT NOT NULL DEFAULT 1, -- default Draft
	Description VARCHAR(500) NULL,
	ApprovedDate DATETIMEOFFSET NULL,                -- When the curriculum was officially approved

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,

	CONSTRAINT FK_Curriculums_Status FOREIGN KEY (StatusId) REFERENCES CurriculumStatuses(Id),
	CONSTRAINT FK_Curriculums_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
	CONSTRAINT FK_Curriculums_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Curriculums_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Curriculums_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_Curriculums_EffectiveYear_Valid CHECK (EffectiveYear >= 2000)
);
GO;

-- Curriculums indexes
CREATE NONCLUSTERED INDEX IX_Curriculums_CourseId 
ON Curriculums(CourseId);
GO

CREATE NONCLUSTERED INDEX IX_Curriculums_Status 
ON Curriculums(StatusId);
GO

-- Unique curriculum version per course (only for active records)
CREATE UNIQUE NONCLUSTERED INDEX UIdx_Curriculums_Course_Version_IsActive
ON Curriculums(CourseId, Version)
WHERE IsActive = 1;
GO

-- ************************************

-- Subjects are now course-agnostic catalog entries
-- They can be reused across multiple Curriculums/courses
-- The relationship to a course is through CurriculumSubjects
CREATE TABLE Subjects
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(20) NOT NULL,
	Title VARCHAR(100) NOT NULL,
	Units DECIMAL(3,1) NOT NULL,
	Description VARCHAR(255) NULL,
	PreferRoomTypeId INT NOT NULL,
	-- Note: CourseId removed - subjects are now course-agnostic
	-- The relationship to courses is through CurriculumSubjects

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	
	CONSTRAINT UQ_Subjects_Code UNIQUE (Code),
	CONSTRAINT CHK_Subjects_Units_Valid CHECK (Units > 0 AND Units <= 12),
	CONSTRAINT FK_Subjects_RoomType FOREIGN KEY (PreferRoomTypeId) REFERENCES RoomTypes(Id),
	CONSTRAINT FK_Subjects_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Subjects_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Subjects_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
);

-- Subjects indexes
CREATE NONCLUSTERED INDEX IX_Subjects_PreferRoomTypeId
ON Subjects(PreferRoomTypeId);
GO;


-- ************************************

-- CurriculumSubjects: Links subjects to a specific curriculum with year/TermNumber placement
-- This is where subjects become part of a course's curriculum
CREATE TABLE CurriculumSubjects
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	CurriculumId INT NOT NULL,
	SubjectId INT NOT NULL,
	YearLevel INT NOT NULL,                -- Which year this subject is typically taken (1-6)
	TermNumber INT NOT NULL,				-- 1st, 2nd, 3rd term in the academic year
	IsElective BIT NOT NULL DEFAULT 0,     -- Whether this is an elective slot
	ElectiveGroupName VARCHAR(50) NULL,    -- Group name for electives (e.g., 'Major Elective', 'Free Elective')

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,

	CONSTRAINT FK_CurriculumSubjects_Curriculum FOREIGN KEY (CurriculumId) REFERENCES Curriculums(Id),
	CONSTRAINT FK_CurriculumSubjects_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_CurriculumSubjects_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_CurriculumSubjects_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_CurriculumSubjects_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_CurriculumSubjects_YearLevel_Valid CHECK (YearLevel BETWEEN 1 AND 6),
	CONSTRAINT CHK_CurriculumSubjects_TermNumber_Valid CHECK (TermNumber IN (1, 2, 3))
);
GO;

-- CurriculumSubjects indexes
CREATE NONCLUSTERED INDEX IX_CurriculumSubjects_CurriculumId 
ON CurriculumSubjects(CurriculumId);
GO

CREATE NONCLUSTERED INDEX IX_CurriculumSubjects_SubjectId 
ON CurriculumSubjects(SubjectId);
GO

-- Ensure a subject appears only once per curriculum (active records only)
CREATE UNIQUE NONCLUSTERED INDEX UIdx_CurriculumSubjects_Curriculum_Subject_IsActive
ON CurriculumSubjects(CurriculumId, SubjectId)
WHERE IsActive = 1;
GO


-- ************************************

-- Prerequisites are now scoped to the curriculum level
-- This allows the same subject to have different prerequisites in different curriculum versions
-- Example: "Data Structures" in Curriculum 2023 requires only "Programming 1"
--          "Data Structures" in Curriculum 2024 requires "Programming 1" AND "Discrete Math"
CREATE TABLE CurriculumSubjectPrerequisites
(
	CurriculumSubjectId INT NOT NULL,              -- The subject that has prerequisites
	PrerequisiteCurriculumSubjectId INT NOT NULL,  -- The prerequisite subject (must be in same curriculum)
	MinimumGrade DECIMAL(3,2) NULL,                -- Optional: minimum grade required (e.g., 2.0)

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,

	CONSTRAINT PK_CurriculumSubjectPrerequisites PRIMARY KEY(CurriculumSubjectId, PrerequisiteCurriculumSubjectId),
	CONSTRAINT FK_CurriculumSubjectPrereqs_CurriculumSubject FOREIGN KEY (CurriculumSubjectId) REFERENCES CurriculumSubjects(Id),
	CONSTRAINT FK_CurriculumSubjectPrereqs_PrereqCurriculumSubject FOREIGN KEY (PrerequisiteCurriculumSubjectId) REFERENCES CurriculumSubjects(Id),
	CONSTRAINT FK_CurriculumSubjectPrereqs_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_CurriculumSubjectPrereqs_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_CurriculumSubjectPrereqs_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_CurriculumSubjectPrereqs_NoSelfReference CHECK (CurriculumSubjectId <> PrerequisiteCurriculumSubjectId),
	CONSTRAINT CHK_CurriculumSubjectPrereqs_MinGrade_Valid CHECK (MinimumGrade IS NULL OR (MinimumGrade >= 1.0 AND MinimumGrade <= 5.0))
);
GO;

-- CurriculumSubjectPrerequisites indexes
CREATE NONCLUSTERED INDEX IX_CurriculumSubjectPrereqs_CurriculumSubjectId 
ON CurriculumSubjectPrerequisites(CurriculumSubjectId);
GO

CREATE NONCLUSTERED INDEX IX_CurriculumSubjectPrereqs_PrereqCurriculumSubjectId 
ON CurriculumSubjectPrerequisites(PrerequisiteCurriculumSubjectId);
GO

-- Supports multiple prerequisites per curriculum subject
-- It only applies the uniqueness check to rows where IsActive = 1.
-- You can still insert historical/inactive rows (IsActive = 0), so soft-deletion works.
CREATE UNIQUE NONCLUSTERED INDEX UIdx_CurriculumSubjectPrereqs_IsActive
ON CurriculumSubjectPrerequisites(CurriculumSubjectId, PrerequisiteCurriculumSubjectId)
WHERE IsActive = 1;


-- ************************************

CREATE TABLE SubjectEquivalenceGroups
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(255) NOT NULL,
	
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,

	CONSTRAINT UQ_SubjectEquivalenceGroups_Name UNIQUE (Name),
	CONSTRAINT FK_EquivalentSubjectMapping_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_EquivalentSubjectMapping_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_EquivalentSubjectMapping_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
)

-- Links multiple Subjects across Colleges or Department
CREATE TABLE SubjectEquivalences
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	SubjectId INT NOT NULL,
	EquivalenceGroupId INT NOT NULL,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_SubjectEquivalences_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_SubjectEquivalences_EquivalenceGroup FOREIGN KEY (EquivalenceGroupId) REFERENCES SubjectEquivalenceGroups(Id),
	CONSTRAINT FK_SubjectEquivalences_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_SubjectEquivalences_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_SubjectEquivalences_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO;

--Supports multiple equivalent subjects per subject
--It only applies the uniqueness check to rows where IsActive = 1.
--You can still insert historical/inactive rows (IsActive = 0), so soft-deletion works.
--Ensures that at most one active mapping per (SourceSubjectId, EquivalentSubjectId) exists.
CREATE UNIQUE NONCLUSTERED INDEX UIdx_Subject_EquivalentSubject_IsActive
ON SubjectEquivalences(SubjectId, EquivalenceGroupId)
WHERE IsActive = 1;


-- SubjectEquivalence indexes
CREATE NONCLUSTERED INDEX IX_SubjectEquivalences_SubjectId 
ON SubjectEquivalences(SubjectId);
GO

CREATE NONCLUSTERED INDEX IX_SubjectEquivalences_EquivalenceGroupId 
ON SubjectEquivalences(EquivalenceGroupId);
GO


-- ************************************
CREATE TABLE Teachers
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	Email VARCHAR(255) NOT NULL,
	DepartmentId INT NOT NULL,
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_Teachers_Email UNIQUE (Email),
	CONSTRAINT FK_Teachers_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
	CONSTRAINT FK_Teachers_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Teachers_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Teachers_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO;


-- Teachers indexes
CREATE NONCLUSTERED INDEX IX_Teachers_DepartmentId 
ON Teachers(DepartmentId);
GO



CREATE TABLE TeacherSubjects
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	TeacherId INT NOT NULL,
	SubjectId INT NOT NULL,
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_TeacherSubjects_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id),
	CONSTRAINT FK_TeacherSubjects_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_TeacherSubjects_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_TeacherSubjects_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_TeacherSubjects_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO;


-- TeacherSubjects indexes
CREATE NONCLUSTERED INDEX IX_TeacherSubjects_TeacherId 
ON TeacherSubjects(TeacherId);
GO

CREATE NONCLUSTERED INDEX IX_TeacherSubjects_SubjectId 
ON TeacherSubjects(SubjectId);
GO

-- ************************************

CREATE TABLE Semesters
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Semester INT NOT NULL, -- 1 for 1st Semester, 2 for 2nd Semester
	Name VARCHAR(50) NOT NULL,
	Description VARCHAR(50) NOT NULL,
	SchoolYear INT NOT NULL,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Semesters_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Semesters_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Semesters_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_Semesters_Semester_Valid CHECK (Semester IN (1, 2, 3)),
	CONSTRAINT CHK_Semesters_SchoolYear_Valid CHECK (SchoolYear >= 2000)
);
GO;



-- ************************************

--  "BSCS-2A", "ENG101-A"
CREATE TABLE ClassSections
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	YearLevel INT NOT NULL,
	CourseId INT NOT NULL,
	SemesterId INT NOT NULL,
	AdviserId INT NOT NULL,
	StudentCapacity INT NOT NULL, -- Soft rule

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_ClassSections_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
	CONSTRAINT FK_ClassSections_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id),
	CONSTRAINT FK_ClassSections_Adviser FOREIGN KEY (AdviserId) REFERENCES Teachers(Id),
	CONSTRAINT FK_ClassSections_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSections_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSections_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_ClassSections_YearLevel_Valid CHECK (YearLevel BETWEEN 1 AND 6),
	CONSTRAINT CHK_ClassSections_StudentCapacity_Positive CHECK (StudentCapacity > 0)
);
GO;


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
	DayPattern VARCHAR(10) NULL,      -- 'MW', 'TTh', 'MWF', 'MTWTHF',
	DaysPerWeek INT NULL,              -- 2, 3, 5, etc.
	HoursPerDay DECIMAL(3,1) NULL,     -- 1.5, 2.0, 3.0, etc.
	MaxNumberOfStudents INT NULL, -- Optional, soft rule, this to allow us to override the room student capacity

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_ClassSectionSubjectOffering_Subject FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_Room FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSectionSubjectOffering_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO;

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


-- Separate schedule details table (multiple rows for multi-day subjects)
CREATE TABLE ClassSchedules
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ClassSectionSubjectOfferingId INT NOT NULL,
    DayOfWeek CHAR(3) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAt DATETIMEOFFSET NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_ClassSchedules_Offering 
        FOREIGN KEY (ClassSectionSubjectOfferingId) 
        REFERENCES ClassSectionSubjectOffering(Id),
    
    CONSTRAINT CHK_ClassSchedules_DayOfWeek_Valid 
        CHECK (DayOfWeek IN ('MON','TUE','WED','THU','FRI','SAT','SUN')),
    
    CONSTRAINT CHK_ClassSchedules_Time_Valid 
        CHECK (StartTime < EndTime),
    
    -- Prevent duplicate schedules for same offering
    CONSTRAINT UQ_ClassSchedules_Offering_Day 
        UNIQUE (ClassSectionSubjectOfferingId, DayOfWeek)
);

-- **Example Data:**

-- **ClassSectionSubjectOffering:**
-- | Id | SubjectId | TeacherId | ClassSectionId | RoomId |
-- |----|-----------|-----------|----------------|--------|
-- | 1  | 101       | 5         | 10             | 301    |

-- **ClassSchedules:**
-- | Id | ClassSectionSubjectOfferingId | DayOfWeek | StartTime | EndTime |
-- |----|------------------------------|-----------|-----------|---------|
-- | 1  | 1                            | MON       | 09:00:00  | 10:30:00|
-- | 2  | 1                            | WED       | 09:00:00  | 10:30:00|


-- ====================================
-- STUDENT STATUS LOOKUP TABLE
-- ====================================
CREATE TABLE StudentStatuses
(
	Id INT NOT NULL PRIMARY KEY,
	Code VARCHAR(20) NOT NULL,
	Name VARCHAR(50) NOT NULL,
	Description VARCHAR(255) NULL,
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	DisplayOrder INT NOT NULL DEFAULT 0,

	CONSTRAINT UQ_StudentStatuses_Code UNIQUE (Code),
	CONSTRAINT FK_StudentStatuses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_StudentStatuses_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_StudentStatuses_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_StudentStatuses_Code_NotEmpty CHECK (LEN(TRIM(Code)) > 0),
    CONSTRAINT CHK_StudentStatuses_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0)
);
GO

-- Seed data for StudentStatuses
DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
INSERT INTO StudentStatuses (Id, Code, Name, Description, DisplayOrder, CreatedBy) VALUES
(1, 'ACTIVE', 'Active', 'Currently enrolled student', 1, @InitialUserId),
(2, 'INACTIVE', 'Inactive', 'Not currently enrolled but not withdrawn', 2, @InitialUserId),
(3, 'LOA', 'Leave of Absence', 'Temporarily not attending', 3, @InitialUserId),
(4, 'GRADUATED', 'Graduated', 'Completed degree requirements', 4, @InitialUserId),
(5, 'WITHDRAWN', 'Withdrawn', 'Permanently left the institution', 5, @InitialUserId),
(6, 'SUSPENDED', 'Suspended', 'Temporarily barred from enrollment', 6, @InitialUserId),
(7, 'EXPELLED', 'Expelled', 'Permanently barred from institution', 7, @InitialUserId);
GO


-- ************************************

CREATE TABLE Students
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	StudentNumber VARCHAR(13) NOT NULL,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	Email VARCHAR(255) NOT NULL,
	CourseId INT NOT NULL,
	CurriculumId INT NOT NULL,             -- Which curriculum version the student follows
	YearLevel INT NOT NULL,
	Status INT NOT NULL DEFAULT 1, -- References StudentStatuses, default to ACTIVE
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_Students_StudentNumber UNIQUE (StudentNumber),
	CONSTRAINT UQ_Students_Email UNIQUE (Email),
	CONSTRAINT FK_Students_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
	CONSTRAINT FK_Students_Curriculum FOREIGN KEY (CurriculumId) REFERENCES Curriculums(Id),
    CONSTRAINT FK_Students_StudentStatus FOREIGN KEY (Status) REFERENCES StudentStatuses(Id),
    CONSTRAINT FK_Students_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Students_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Students_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_Students_YearLevel_Valid CHECK (YearLevel BETWEEN 1 AND 6)
);
GO


-- Students indexes
CREATE NONCLUSTERED INDEX IX_Students_CourseId 
ON Students(CourseId);
GO

CREATE NONCLUSTERED INDEX IX_Students_CurriculumId 
ON Students(CurriculumId);
GO

CREATE NONCLUSTERED INDEX IX_Students_Status 
ON Students(Status);
GO


-- ************************************


-- ====================================
-- ENROLLMENT STATUS LOOKUP TABLE
-- ====================================
CREATE TABLE EnrollmentStatuses
(
	Id INT NOT NULL PRIMARY KEY,
	Code VARCHAR(20) NOT NULL,
	Name VARCHAR(50) NOT NULL,
	Description VARCHAR(255) NULL,
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	DisplayOrder INT NOT NULL DEFAULT 0,

	CONSTRAINT UQ_EnrollmentStatuses_Code UNIQUE (Code),
	CONSTRAINT FK_EnrollmentStatuses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_EnrollmentStatuses_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_EnrollmentStatuses_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_EnrollmentStatuses_Code_NotEmpty CHECK (LEN(TRIM(Code)) > 0),
    CONSTRAINT CHK_EnrollmentStatuses_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0)
);
GO

-- Seed data for EnrollmentStatuses
DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
INSERT INTO EnrollmentStatuses (Id, Code, Name, Description, DisplayOrder, CreatedBy) VALUES
(1, 'PENDING', 'Pending', 'Awaiting approval', 1, @InitialUserId),
(2, 'APPROVED', 'Approved', 'Ready for enrollment', 2, @InitialUserId),
(3, 'ENROLLED', 'Enrolled', 'Currently enrolled in classes', 3, @InitialUserId),
(4, 'COMPLETED', 'Completed', 'Semester completed', 4, @InitialUserId),
(5, 'DROPPED', 'Dropped', 'Student dropped enrollment', 5, @InitialUserId),
(6, 'FAILED', 'Failed', 'Did not meet requirements', 6, @InitialUserId),
(7, 'CANCELLED', 'Cancelled', 'Enrollment cancelled by system', 7, @InitialUserId);
GO


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
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Enrollments_ClassSection FOREIGN KEY (ClassSectionId) REFERENCES ClassSections(Id),
    CONSTRAINT FK_Enrollments_ClassSectionSubjectOffering FOREIGN KEY (ClassSectionSubjectOfferingId) REFERENCES ClassSectionSubjectOffering(Id),
    CONSTRAINT FK_Enrollments_Semester FOREIGN KEY (SemesterId) REFERENCES Semesters(Id),
    CONSTRAINT FK_Enrollments_EnrollmentStatus FOREIGN KEY (Status) REFERENCES EnrollmentStatuses(Id),
    CONSTRAINT FK_Enrollments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Enrollments_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Enrollments_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
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


-- ************************************

-- Linked indirectly to a Student, Subject, and Teacher
CREATE TABLE EnrollmentAcademicRecords
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	EnrollmentId INT NOT NULL,
	MidtermGrade DECIMAL(5, 2),
	FinalGrade DECIMAL(5, 2),
	Remarks VARCHAR(255),
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_EnrollmentAcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id),
	CONSTRAINT FK_EnrollmentAcademicRecords_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_EnrollmentAcademicRecords_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_EnrollmentAcademicRecords_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_EnrollmentAcademicRecords_MidtermGrade_Valid CHECK (MidtermGrade IS NULL OR (MidtermGrade BETWEEN 0 AND 100)),
	CONSTRAINT CHK_EnrollmentAcademicRecords_FinalGrade_Valid CHECK (FinalGrade IS NULL OR (FinalGrade BETWEEN 0 AND 100))
);
GO;


-- EnrollmentAcademicRecords indexes
CREATE NONCLUSTERED INDEX IX_EnrollmentAcademicRecords_EnrollmentId 
ON EnrollmentAcademicRecords(EnrollmentId);
GO

CREATE TABLE EnrollmentPayments
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	EnrollmentId INT NOT NULL,
	Amount DECIMAL(10, 2) NOT NULL,
	PaymentDate DATETIME2 NOT NULL,
	PaymentMethod VARCHAR(50) NOT NULL,
	ReferenceNumber VARCHAR(100) NULL,
	PaymentStatus VARCHAR(50) NOT NULL,
	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_EnrollmentPayments_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id),
	CONSTRAINT FK_EnrollmentPayments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_EnrollmentPayments_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_EnrollmentPayments_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO;

-- EnrollmentPayments indexes
CREATE NONCLUSTERED INDEX IX_EnrollmentPayments_EnrollmentId 
ON EnrollmentPayments(EnrollmentId);
GO
