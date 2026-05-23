
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


CREATE TABLE StudentTypes
(
    Id          INT NOT NULL PRIMARY KEY,
    Code        VARCHAR(20) NOT NULL,
    Name        VARCHAR(50) NOT NULL,
    Description VARCHAR(255) NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedAt   DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy   INT NOT NULL,
    CONSTRAINT UQ_StudentTypes_Code UNIQUE (Code),
    CONSTRAINT FK_StudentTypes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);
GO

-- ************************************

CREATE TABLE Students
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentTypeId INT NOT NULL DEFAULT 1, -- References StudentTypes, default to Regular
	StudentNumber VARCHAR(13) NOT NULL,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	Email VARCHAR(255) NOT NULL,
	CourseId INT NOT NULL,
	CurriculumId INT NOT NULL,             -- Which curriculum version the student follows
    EntryAcademicYearId INT NOT NULL,      -- The AY when Year 1 students of this cohort start, used to determine curriculum lock and class section assignment
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
    CONSTRAINT FK_Students_Curriculum FOREIGN KEY (CurriculumId, CourseId) REFERENCES Curriculums(Id, CourseId),
    CONSTRAINT FK_Students_EntryAcademicYear FOREIGN KEY (EntryAcademicYearId) REFERENCES AcademicYears(Id),
    CONSTRAINT FK_Students_StudentStatus FOREIGN KEY (Status) REFERENCES StudentStatuses(Id),
    CONSTRAINT FK_Students_StudentType FOREIGN KEY (StudentTypeId) REFERENCES StudentTypes(Id),
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

CREATE NONCLUSTERED INDEX IX_Students_StudentTypeId
    ON Students(StudentTypeId);
GO
