
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
