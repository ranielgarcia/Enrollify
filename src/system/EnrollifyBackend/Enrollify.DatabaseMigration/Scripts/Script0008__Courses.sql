
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
