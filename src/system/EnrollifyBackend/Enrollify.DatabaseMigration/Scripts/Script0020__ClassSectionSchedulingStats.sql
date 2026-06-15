CREATE TABLE ClassSectionSchedulingStats
(
  Id             INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  CollegeId      INT            NOT NULL,
  CourseId       INT            NOT NULL,
  AcademicTermId INT            NOT NULL,
  ClassSectionId INT NULL,
  Type           INT            NOT NULL,
  AggregateCount INT            NOT NULL, -- e.g. number of conflicts of this type for the class section
  ComputedAt     DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionSchedulingStats_College
    FOREIGN KEY (CollegeId) REFERENCES Colleges (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionSchedulingStats_Course
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionSchedulingStats_AcademicTerm
    FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionSchedulingStats_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionSchedulingStats_Offering
    FOREIGN KEY (OfferingId) REFERENCES ClassSectionSubjectOffering (Id) ON DELETE CASCADE,
);

CREATE
NONCLUSTERED INDEX IX_ClassSectionSchedulingStats_ClassSectionId
  ON ClassSectionSchedulingStats (ClassSectionId);

CREATE
NONCLUSTERED INDEX IX_ClassSectionSchedulingStats_OfferingId
  ON ClassSectionSchedulingStats(OfferingId);

CREATE
NONCLUSTERED INDEX
IX_ClassSectionSchedulingStats_College_Term_Severity
ON ClassSectionSchedulingStats
(
    CollegeId,
    AcademicTermId
)
INCLUDE
(
    ClassSectionId,
    OfferingId,
    Type,
    ComputedAt
);
