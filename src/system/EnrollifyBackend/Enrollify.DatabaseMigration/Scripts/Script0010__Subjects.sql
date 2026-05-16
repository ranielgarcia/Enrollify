-- Subjects are now course-agnostic catalog entries
-- They can be reused across multiple Curriculums/courses
-- The relationship to a course is through CurriculumSubjects
CREATE TABLE Subjects
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(20) NOT NULL,
	Title VARCHAR(100) NOT NULL,
	Units DECIMAL(3,1) NOT NULL,
    -- Default Subject'' units, but can be overridden at the curriculum level or at class section subject offering if needed
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

CREATE NONCLUSTERED INDEX IX_Subjects_Code
ON Subjects(Code);
GO;



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
	CONSTRAINT FK_SubjectEquivalenceGroups_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_SubjectEquivalenceGroups_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_SubjectEquivalenceGroups_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
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
