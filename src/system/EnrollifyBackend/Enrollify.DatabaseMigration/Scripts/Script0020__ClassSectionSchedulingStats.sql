CREATE TABLE ClassSectionSchedulingStats
(
  Id             INT            NOT NULL IDENTITY (1,1) PRIMARY KEY,
  AcademicTermId INT            NOT NULL,
  CourseId       INT            NOT NULL,
  ClassSectionId INT NULL,
  AggregateType  VARCHAR(50)    NOT NULL,
  AggregateCount INT            NOT NULL, -- e.g. number of conflicts of this type for the class section
  ComputedAt     DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  CONSTRAINT FK_ClassSectionSchedulingStats_AcademicTerm
    FOREIGN KEY (AcademicTermId) REFERENCES AcademicTerms (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionSchedulingStats_Course
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
  CONSTRAINT FK_ClassSectionSchedulingStats_ClassSection
    FOREIGN KEY (ClassSectionId) REFERENCES ClassSections (Id) ON DELETE CASCADE
);

CREATE
UNIQUE
NONCLUSTERED INDEX UX_ClassSectionSchedulingStats_CourseLevel
   ON ClassSectionSchedulingStats (AcademicTermId, CourseId, AggregateType)
   WHERE ClassSectionId IS NULL;

 CREATE UNIQUE
NONCLUSTERED INDEX UX_ClassSectionSchedulingStats_SectionLevel
   ON ClassSectionSchedulingStats (AcademicTermId, CourseId, AggregateType, ClassSectionId)
   WHERE ClassSectionId IS NOT NULL;

