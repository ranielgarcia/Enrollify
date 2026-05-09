CREATE TABLE ClassSections
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL, --  "BSCS-1A", "BSCS-2A"
	YearLevel INT NOT NULL,
	CourseId INT NOT NULL,
	AcademicTermId INT NOT NULL,
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
	CONSTRAINT FK_ClassSections_AcademicTerm FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms(Id),
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

CREATE NONCLUSTERED INDEX IX_ClassSections_AcademicTermId
ON ClassSections(AcademicTermId);
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
	-- DayPattern VARCHAR(10) NULL,      -- 'MW', 'TTh', 'MWF', 'MTWTHF',
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
(4, 'COMPLETED', 'Completed', 'Academic term completed', 4, @InitialUserId),
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
    AcademicTermId INT NOT NULL,
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
    CONSTRAINT FK_Enrollments_AcademicTerm FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms(Id),
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

CREATE NONCLUSTERED INDEX IX_Enrollments_AcademicTermId
ON Enrollments(AcademicTermId);
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
