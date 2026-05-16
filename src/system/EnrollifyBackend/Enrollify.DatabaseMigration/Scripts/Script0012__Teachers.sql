CREATE TABLE Teachers
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	FirstName VARCHAR(50) NOT NULL,
	MiddleName VARCHAR(50) NULL,
	LastName VARCHAR(50) NOT NULL,
    TeacherIdentifier CHAR(20) NOT NULL,
    PhoneNumber CHAR(11) NOT NULL, -- format: 09xxxxxxxxx
	Email VARCHAR(255) NOT NULL,
	DepartmentId INT NOT NULL,
    AcademicTitle VARCHAR(100) NULL,
    Qualification VARCHAR(255) NULL,
    Specialization VARCHAR(255) NULL,
    OfficeLocation VARCHAR(255) NULL,
    OfficeHours VARCHAR(255) NULL,
    Biography TEXT NULL,

    PhotoFileName VARCHAR(255) NULL,
    PhotoContentType VARCHAR(50) NULL,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT UQ_Teachers_TeacherIdentifier UNIQUE (TeacherIdentifier),
	CONSTRAINT UQ_Teachers_PhoneNumber UNIQUE (PhoneNumber),
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

CREATE UNIQUE NONCLUSTERED INDEX UIdx_TeacherSubjects_IsActive
ON TeacherSubjects(TeacherId, SubjectId)
WHERE IsActive = 1;
