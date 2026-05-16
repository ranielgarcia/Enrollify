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
