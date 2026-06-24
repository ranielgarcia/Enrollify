CREATE TABLE ClassSectionValidationIssues
(
  Id                       INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  CourseId                 INT            NOT NULL,
  AcademicTermId           INT            NOT NULL,
  ClassSectionId           INT            NOT NULL,
  OfferingId               INT            NULL,     -- the specific offering that caused the conflict, if applicable
  Type                     VARCHAR(50)    NOT NULL, -- e.g. "TIME_CONFLICT", "INSTRUCTOR_CONFLICT"
  Message                  VARCHAR(255)   NOT NULL, -- Human-readable description
  DayOfWeek                CHAR(3)        NULL,
  StartTime                TIME           NULL,
  EndTime                  TIME           NULL,
  ConflictingOfferingsJson NVARCHAR(MAX)  NULL,     -- json array of affected offerings objects
  ComputedAt               DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionValidationIssues_Course
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_AcademicTerm
    FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_Offering
    FOREIGN KEY (OfferingId) REFERENCES ClassSectionSubjectOffering (Id) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_ClassSectionValidationIssues_ClassSectionId
  ON ClassSectionValidationIssues (ClassSectionId);
GO

CREATE NONCLUSTERED INDEX IX_ClassSectionValidationIssues_Main
  ON ClassSectionValidationIssues
    (
     AcademicTermId,
     CourseId,
     Type,
     ClassSectionId
      );

-- Class Section level issues unique constraint
CREATE UNIQUE NONCLUSTERED INDEX UX_ClassSectionValidationIssues_Section
  ON ClassSectionValidationIssues
    (
     AcademicTermId,
     CourseId,
     ClassSectionId,
     Type
      )
  WHERE OfferingId IS NULL;

-- Offering Level issues unique constraint
CREATE UNIQUE NONCLUSTERED INDEX UX_ClassSectionValidationIssues_Offering
  ON ClassSectionValidationIssues
    (
     AcademicTermId,
     CourseId,
     ClassSectionId,
     OfferingId,
     Type
      )
  WHERE OfferingId IS NOT NULL;
