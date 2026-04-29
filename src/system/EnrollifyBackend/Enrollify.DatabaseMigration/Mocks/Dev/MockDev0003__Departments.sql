DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');

CREATE TABLE #TempDepartments(
	[Code] VARCHAR(10) NOT NULL,
	[Name] VARCHAR(100) NOT NULL,
	[Chairperson] VARCHAR(100) NOT NULL,
	[Description] VARCHAR(255) NULL,
	[CollegeCode] VARCHAR(10) NOT NULL
)

INSERT INTO #TempDepartments ([Code], [Name], [Chairperson], [Description], [CollegeCode])
VALUES
-- College of Computer Studies
('CS',   'Computer Science',       'Dr. Maria Santos',   'Focuses on the theoretical foundations of computation, algorithm design, and software engineering principles.', 'CCS'),
('IT',   'Information Technology', 'Dr. Jose Reyes',     'Prepares students for careers in IT infrastructure, systems administration, and enterprise application development.', 'CCS'),
-- College of Engineering
('CE',   'Civil Engineering',      'Engr. Ana Lim',      'Covers the design, construction, and maintenance of infrastructure such as roads, bridges, and buildings.', 'COE'),
('EE',   'Electrical Engineering', 'Engr. Carlos Cruz',  'Focuses on electrical systems, power generation and distribution, electronics, and telecommunications.', 'COE')
;

MERGE [Departments] AS [Target]
USING
    (
        SELECT t.[Code], t.[Name], t.[Chairperson], t.[Description], c.[Id] AS [CollegeId]
        FROM #TempDepartments t
        INNER JOIN [Colleges] c ON c.[Code] = t.[CollegeCode]
    ) AS [Source]
    ON [Target].[Code] = [Source].[Code] AND [Target].[CollegeId] = [Source].[CollegeId]
WHEN MATCHED THEN
    UPDATE SET
        [Target].[Name]        = [Source].[Name],
        [Target].[Chairperson] = [Source].[Chairperson],
        [Target].[Description] = [Source].[Description],
        [Target].[UpdatedBy]   = @InitialUserId,
        [Target].[UpdatedAt]   = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT ([Code], [Name], [Chairperson], [Description], [CollegeId], [CreatedBy], [CreatedAt])
    VALUES ([Source].[Code], [Source].[Name], [Source].[Chairperson], [Source].[Description], [Source].[CollegeId], @InitialUserId, GETUTCDATE());

DROP TABLE #TempDepartments;
