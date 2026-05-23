CREATE TABLE ClassSectionStatuses
(
	Id INT NOT NULL PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	CONSTRAINT UQ_ClassSectionStatuses_Name UNIQUE (Name),
);
GO

CREATE TABLE ClassSections
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL, -- Use course code, e.g. "BSCS", "BSCS"
	CourseId INT NOT NULL,
    AcademicTermId INT NOT NULL, -- Current academic term
	IntendedYearLevel INT NOT NULL, -- Intended year level
    CohortAcademicYearId INT NOT NULL, -- Which cohort the section is intended for
    CurriculumId INT NOT NULL, -- Which curriculum version this section follows, for reporting purposes, e.g. "2024-A", "2024-REV1"
	AdviserId INT NULL, -- to allow bulk creation of sections without immediately assigning advisers, can be updated later
    SectionCode CHAR(1) NOT NULL, -- "A", "B", "C"
    StatusId INT NOT NULL DEFAULT 1, -- References ClassSectionStatuses, default to Draft

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_ClassSections_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id),
	CONSTRAINT FK_ClassSections_AcademicTerm FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms(Id),
	CONSTRAINT FK_ClassSections_CohortAcademicYear FOREIGN KEY (CohortAcademicYearId) REFERENCES AcademicYears(Id),
	CONSTRAINT FK_ClassSections_Curriculum FOREIGN KEY (CurriculumId, CourseId) REFERENCES Curriculums(Id, CourseId),
	CONSTRAINT FK_ClassSections_Adviser FOREIGN KEY (AdviserId) REFERENCES Teachers(Id),
	CONSTRAINT FK_ClassSections_Status FOREIGN KEY (StatusId) REFERENCES ClassSectionStatuses(Id),
	CONSTRAINT FK_ClassSections_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSections_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSections_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT CHK_ClassSections_IntendedYearLevel_Valid CHECK (IntendedYearLevel BETWEEN 1 AND 6)
);
GO;


-- ClassSections indexes
CREATE NONCLUSTERED INDEX IX_ClassSections_CourseId
ON ClassSections(CourseId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSections_CurriculumId
ON ClassSections(CurriculumId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSections_EntryAcademicYearId
ON ClassSections(EntryAcademicYearId);
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
    SubjectUnitsOverride DECIMAL(3,1) NULL, -- Optional override for subject units at this level, for scenarios that require less or more units than the default subject units or curriculum-level override, e.g. a 3-unit subject offered as a 1.5-unit elective
	TeacherId INT NULL, -- Assigned to a Teacher,
	ClassSectionId INT NOT NULL, -- Belongs to a ClassSection
	RoomId INT NULL,
	DaysPerWeek INT NOT NULL DEFAULT 1,              -- 2, 3, 5, etc.
	HoursPerDay DECIMAL(3,1) NOT NULL DEFAULT 1,     -- 1.5, 2.0, 3.0, etc.
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
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ClassSchedules_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSchedules_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_ClassSchedules_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
    CONSTRAINT FK_ClassSchedules_Offering
        FOREIGN KEY (ClassSectionSubjectOfferingId)
        REFERENCES ClassSectionSubjectOffering(Id),

    CONSTRAINT CHK_ClassSchedules_DayOfWeek_Valid
        CHECK (DayOfWeek IN ('MON','TUE','WED','THU','FRI','SAT','SUN')),

    CONSTRAINT CHK_ClassSchedules_Time_Valid
        CHECK (StartTime < EndTime),

    -- Prevent duplicate schedules for same offering
    CONSTRAINT UQ_ClassSchedules_Offering_Day
        UNIQUE (ClassSectionSubjectOfferingId, DayOfWeek),
);
GO;
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

