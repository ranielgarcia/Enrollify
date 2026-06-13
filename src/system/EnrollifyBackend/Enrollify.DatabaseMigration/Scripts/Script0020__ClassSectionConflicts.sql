CREATE TABLE ClassSectionConflicts
(
  Id             INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  CollegeId      INT            NOT NULL,
  CourseId       INT            NOT NULL,
  AcademicTermId INT            NOT NULL,
  ClassSectionId INT            NOT NULL,
  OfferingId     INT            NOT NULL,
  ConflictType   VARCHAR(50)    NOT NULL, -- e.g. "TIME_CONFLICT", "INSTRUCTOR_CONFLICT"
  Severity       INT            NOT NULL, -- e.g. 1 = Info, 2 = Warning, 3 = Error
  Message        VARCHAR(255)   NOT NULL, -- Human-readable description
  DayOfWeek      CHAR(3)        NOT NULL,
  StartTime      TIME           NULL,
  EndTime        TIME           NULL,
  ComputedAt     DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionConflicts_College
    FOREIGN KEY (CollegeId) REFERENCES Colleges (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionConflicts_Course
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionConflicts_AcademicTerm
    FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionConflicts_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionConflicts_Offering
    FOREIGN KEY (OfferingId) REFERENCES ClassSectionSubjectOffering (Id) ON DELETE CASCADE,
);

CREATE
  NONCLUSTERED INDEX IX_ClassSectionConflicts_ClassSectionId
  ON ClassSectionConflicts (ClassSectionId);

CREATE
  NONCLUSTERED INDEX IX_ClassSectionConflicts_OfferingId
  ON ClassSectionConflicts(OfferingId);

CREATE NONCLUSTERED INDEX
IX_ClassSectionConflicts_College_Term_Severity
ON ClassSectionConflicts
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


CREATE TABLE ClassSectionConflictAffectedOfferings
(
  Id             INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  ConflictId     INT            NOT NULL,
  OfferingId     INT            NOT NULL,
  CONSTRAINT FK_ClassSectionConflictAffectedOfferings_Conflict
    FOREIGN KEY (ConflictId) REFERENCES ClassSectionConflicts (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionConflictAffectedOfferings_Offering
    FOREIGN KEY (OfferingId) REFERENCES ClassSectionSubjectOffering (Id) ON DELETE NO ACTION,
);

CREATE
  NONCLUSTERED INDEX IX_ClassSectionConflictAffectedOfferings_ConflictId
  ON ClassSectionConflictAffectedOfferings (ConflictId);
