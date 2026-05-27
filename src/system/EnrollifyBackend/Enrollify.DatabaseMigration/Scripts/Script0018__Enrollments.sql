
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
