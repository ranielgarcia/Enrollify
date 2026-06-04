CREATE TABLE ClassSectionEnrollmentEligibilityValidationMessages
(
  Id             INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  ClassSectionId INT            NOT NULL,
  Code           VARCHAR(50)    NOT NULL, -- e.g. "CLASS_SECTION_ADVISER_REQUIRED"
  Message        VARCHAR(255)   NOT NULL, -- Human-readable description
  ComputedAt     DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionEligibilityMessages_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE,
);
GO

CREATE NONCLUSTERED INDEX IX_ClassSectionEligibilityMessages_ClassSectionId
  ON ClassSectionEnrollmentEligibilityValidationMessages (ClassSectionId);
GO

