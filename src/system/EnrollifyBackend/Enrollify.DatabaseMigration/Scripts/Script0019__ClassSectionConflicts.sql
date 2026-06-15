CREATE TABLE ClassSectionValidationIssues
(
  Id                       INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  CollegeId                INT            NOT NULL,
  CourseId                 INT            NOT NULL,
  AcademicTermId           INT            NOT NULL,
  ClassSectionId           INT            NOT NULL,
  OfferingId               INT NULL,                -- the specific offering that caused the conflict, if applicable
  Type                     VARCHAR(50)    NOT NULL, -- e.g. "TIME_CONFLICT", "INSTRUCTOR_CONFLICT"
  Severity                 INT            NOT NULL, -- e.g. 1 = Info, 2 = Warning, 3 = Error
  Message                  VARCHAR(255)   NOT NULL, -- Human-readable description
  DayOfWeek                CHAR(3) NULL,
  StartTime                TIME NULL,
  EndTime                  TIME NULL,
  ConflictingOfferingsJson NVARCHAR(MAX) NULL,      -- json array of affected offerings objects
  ComputedAt               DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionValidationIssues_College
    FOREIGN KEY (CollegeId) REFERENCES Colleges (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_Course
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_AcademicTerm
    FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionValidationIssues_Offering
    FOREIGN KEY (OfferingId) REFERENCES ClassSectionSubjectOffering (Id) ON DELETE CASCADE,
);

CREATE
NONCLUSTERED INDEX IX_ClassSectionValidationIssues_ClassSectionId
  ON ClassSectionValidationIssues (ClassSectionId);

CREATE
NONCLUSTERED INDEX IX_ClassSectionValidationIssues_OfferingId
  ON ClassSectionValidationIssues(OfferingId);

CREATE
NONCLUSTERED INDEX
IX_ClassSectionValidationIssues_College_Term_Severity
ON ClassSectionValidationIssues
(
    CollegeId,
    AcademicTermId,
    Severity
)
INCLUDE
(
    ClassSectionId,
    OfferingId,
    ConflictType,
    StartTime,
    EndTime,
    ComputedAt
);
