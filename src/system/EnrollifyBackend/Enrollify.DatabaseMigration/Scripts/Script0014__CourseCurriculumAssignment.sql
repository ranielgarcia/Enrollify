-- CourseCurriculumAssignments: Locks a curriculum to a cohort.
-- A cohort is identified by Course + Entry Academic Year (the AY when Year 1 students first enroll).
-- When creating class sections for Year N in AY X:
--   EntryAcademicYear = the AcademicYear where StartDate.Year = X.StartDate.Year - (Year N - 1)
--   Then look up (CourseId, EntryAcademicYearId) → CurriculumId from this table.
CREATE TABLE CourseCurriculumAssignments
(
    Id                  INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CourseId            INT NOT NULL,
    EntryAcademicYearId INT NOT NULL,  -- The AY when Year 1 students of this cohort start
    CurriculumId        INT NOT NULL,  -- The curriculum locked for this cohort
    IsLocked            BIT NOT NULL DEFAULT 0,  -- If false, the curriculum is unlocked for this cohort. If True, Curriculum is already used by some class sections, so unlocking is only allowed if no class sections exist for this cohort.
    LockRemarks         NVARCHAR(500) NULL,  -- Optional remarks for locking the curriculum for this cohort

    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    IsActive  BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_CourseCurriculumAssignments_Course       FOREIGN KEY (CourseId)            REFERENCES Courses(Id),
    CONSTRAINT FK_CourseCurriculumAssignments_AcademicYear FOREIGN KEY (EntryAcademicYearId) REFERENCES AcademicYears(Id),
    -- Composite FK ensures the curriculum belongs to the same course as this assignment (enforced at DB level).
    CONSTRAINT FK_CourseCurriculumAssignments_Curriculum   FOREIGN KEY (CurriculumId, CourseId) REFERENCES Curriculums(Id, CourseId),
    CONSTRAINT FK_CourseCurriculumAssignments_CreatedBy    FOREIGN KEY (CreatedBy)           REFERENCES Users(Id),
    CONSTRAINT FK_CourseCurriculumAssignments_UpdatedBy    FOREIGN KEY (UpdatedBy)           REFERENCES Users(Id),
    CONSTRAINT FK_CourseCurriculumAssignments_DeletedBy    FOREIGN KEY (DeletedBy)           REFERENCES Users(Id)
);
GO;


-- CourseCurriculumAssignments indexes
CREATE NONCLUSTERED INDEX IX_CourseCurriculumAssignments_CourseId
ON CourseCurriculumAssignments(CourseId);
GO

CREATE NONCLUSTERED INDEX IX_CourseCurriculumAssignments_EntryAcademicYearId
ON CourseCurriculumAssignments(EntryAcademicYearId);
GO

CREATE NONCLUSTERED INDEX IX_CourseCurriculumAssignments_CurriculumId
ON CourseCurriculumAssignments(CurriculumId);
GO

-- Only one active curriculum lock per cohort (Course + EntryAcademicYear)
-- Filtered on IsActive = 1 to allow soft-delete and re-assignment without conflicting
CREATE UNIQUE NONCLUSTERED INDEX UIdx_CourseCurriculumAssignments_Course_Year_IsActive
ON CourseCurriculumAssignments(CourseId, EntryAcademicYearId)
WHERE IsActive = 1;
GO
