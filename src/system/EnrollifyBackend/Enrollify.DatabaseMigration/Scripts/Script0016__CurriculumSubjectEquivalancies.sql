-- ====================================
-- CURRICULUM SUBJECT EQUIVALENCIES
-- ====================================
-- Defines equivalency relationships between curriculum-specific subjects.
--
-- Supports:
-- - curriculum migration
-- - returnees
-- - subject substitutions
-- - teach-out operations
-- - cross-curriculum enrollment
--
-- Example:
-- Curriculum 2022:
--   CS301 - Operating Systems
--
-- Curriculum 2024:
--   CS340 - Advanced Operating Systems
--
-- These can be marked as FULL or PARTIAL equivalents.
CREATE TABLE CurriculumSubjectEquivalencies
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,

    -- Old / original curriculum subject
    SourceCurriculumSubjectId INT NOT NULL,

    -- New / equivalent curriculum subject
    EquivalentCurriculumSubjectId INT NOT NULL,

    -- FULL = fully interchangeable
    -- PARTIAL = requires additional bridging/completion
    EquivalencyType VARCHAR(20) NOT NULL,

    -- When this equivalency became valid
    EffectiveAcademicYearId INT NOT NULL,

    -- Registrar/Admin who approved the equivalency
    ApprovedBy INT NOT NULL,

    Remarks VARCHAR(500) NULL,

    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_CurriculumSubjectEquivalencies_Source
        FOREIGN KEY (SourceCurriculumSubjectId)
        REFERENCES CurriculumSubjects(Id),

    CONSTRAINT FK_CurriculumSubjectEquivalencies_Equivalent
        FOREIGN KEY (EquivalentCurriculumSubjectId)
        REFERENCES CurriculumSubjects(Id),

    CONSTRAINT FK_CurriculumSubjectEquivalencies_EffectiveAcademicYear
        FOREIGN KEY (EffectiveAcademicYearId)
        REFERENCES AcademicYears(Id),

    CONSTRAINT FK_CurriculumSubjectEquivalencies_ApprovedBy
        FOREIGN KEY (ApprovedBy)
        REFERENCES Users(Id),

    CONSTRAINT FK_CurriculumSubjectEquivalencies_CreatedBy
        FOREIGN KEY (CreatedBy)
        REFERENCES Users(Id),

    CONSTRAINT FK_CurriculumSubjectEquivalencies_UpdatedBy
        FOREIGN KEY (UpdatedBy)
        REFERENCES Users(Id),

    CONSTRAINT FK_CurriculumSubjectEquivalencies_DeletedBy
        FOREIGN KEY (DeletedBy)
        REFERENCES Users(Id),

    -- Prevent self-reference
    CONSTRAINT CHK_CurriculumSubjectEquivalencies_NoSelfReference
        CHECK (SourceCurriculumSubjectId <> EquivalentCurriculumSubjectId),

    -- Restrict allowed equivalency types
    CONSTRAINT CHK_CurriculumSubjectEquivalencies_EquivalencyType
        CHECK (EquivalencyType IN ('FULL', 'PARTIAL'))
);
GO

-- ====================================
-- INDEXES
-- ====================================

CREATE NONCLUSTERED INDEX IX_CurriculumSubjectEquivalencies_Source
ON CurriculumSubjectEquivalencies(SourceCurriculumSubjectId);
GO

CREATE NONCLUSTERED INDEX IX_CurriculumSubjectEquivalencies_Equivalent
ON CurriculumSubjectEquivalencies(EquivalentCurriculumSubjectId);
GO

CREATE NONCLUSTERED INDEX IX_CurriculumSubjectEquivalencies_EffectiveAcademicYear
ON CurriculumSubjectEquivalencies(EffectiveAcademicYearId);
GO

-- Prevent duplicate active equivalencies
CREATE UNIQUE NONCLUSTERED INDEX UIdx_CurriculumSubjectEquivalencies_IsActive
ON CurriculumSubjectEquivalencies
(
    SourceCurriculumSubjectId,
    EquivalentCurriculumSubjectId
)
WHERE IsActive = 1;
GO


--Example:
-- Old Curriculum	                New Curriculum
-----------------------------+---------------------------------------
-- Intro to Computing	            Computer Fundamentals
-- Data Structures	                Data Structures and Algorithms
-- Operating Systems                Advanced Operating Systems
