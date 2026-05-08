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

