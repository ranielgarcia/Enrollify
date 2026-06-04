CREATE TABLE ClassSectionEnrollmentEligibilityValidationMessages
(
  Id             INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  Severity       INT            NOT NULL, -- e.g. 1 = Info, 2 = Warning, 3 = Error
  ClassSectionId INT            NOT NULL,
  OfferingId     INT            NULL,
  Code           VARCHAR(50)    NOT NULL, -- e.g. "CLASS_SECTION_ADVISER_REQUIRED"
  Message        VARCHAR(255)   NOT NULL, -- Human-readable description
  ComputedAt     DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionEligibilityMessages_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionEligibilityMessages_Offering
    FOREIGN KEY (OfferingId) REFERENCES ClassSectionSubjectOffering (Id) ON DELETE CASCADE,
);
GO

CREATE
  NONCLUSTERED INDEX IX_ClassSectionEligibilityMessages_ClassSectionId
  ON ClassSectionEnrollmentEligibilityValidationMessages (ClassSectionId);
GO

