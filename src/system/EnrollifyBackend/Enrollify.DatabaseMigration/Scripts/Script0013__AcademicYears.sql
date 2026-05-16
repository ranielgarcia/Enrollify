CREATE TABLE AcademicYears
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,

    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_AcademicYears_StartEnd UNIQUE (StartDate, EndDate),
    CONSTRAINT FK_AcademicYears_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_AcademicYears_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_AcademicYears_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_AcademicYears_Valid CHECK (YEAR(StartDate) >= 2000 AND YEAR(EndDate) >= 2000)
);
GO;

CREATE TABLE AcademicTerms
(
	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TermNumber INT NOT NULL, --1 as 1st, 2 as 2nd etc.
	AcademicYearId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,

	CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
	CreatedBy INT NOT NULL,
	UpdatedAt DATETIMEOFFSET NULL,
	UpdatedBy INT NULL,
	DeletedAt DATETIMEOFFSET NULL,
	DeletedBy INT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_AcademicTerms_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_AcademicTerms_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_AcademicTerms_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
	CONSTRAINT FK_AcademicTerms_AcademicYear FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(Id),
	CONSTRAINT CHK_AcademicTerms_TermNumber_Valid CHECK (TermNumber IN (1, 2, 3))
);
GO;
