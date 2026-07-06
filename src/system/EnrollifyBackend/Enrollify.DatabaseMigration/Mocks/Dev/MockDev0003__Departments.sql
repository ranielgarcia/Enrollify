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
('EE',   'Electrical Engineering', 'Engr. Carlos Cruz',  'Focuses on electrical systems, power generation and distribution, electronics, and telecommunications.', 'COE'),
-- College of Business Administration
('BA',   'Business Administration', 'Dr. Carlos Dela Cruz',   'Develops expertise in management, finance, marketing, operations, and entrepreneurial leadership.', 'CBA'),
('ACC',  'Accountancy',             'Dr. Luningning Martinez','Provides the technical foundation in accounting, auditing, taxation, and financial reporting.', 'CBA'),
-- College of Education
('SE',   'Secondary Education',     'Dr. Teresita Ramos',     'Prepares educators for secondary-level teaching with specialization in various subject areas.', 'CED'),
('ECE',  'Early Childhood Education','Dr. Kristel Santos',   'Equips educators to teach and nurture children from birth to eight years of age.', 'CED'),
-- College of Arts and Sciences
('ENG',  'English Language Studies', 'Dr. Michael Villanueva','Focuses on the English language, its literature, structure, and role in global communication.', 'CAS'),
('PSY',  'Psychology',              'Dr. Patricia Gonzales',  'Studies human behavior, mental processes, and the application of psychological principles.', 'CAS'),
-- College of Nursing
('NUR',  'Nursing',                 'Dr. Rosa Dimaculangan',  'Develops competent, compassionate nurses prepared for diverse healthcare settings.', 'CN'),
-- College of Architecture
('ARCH', 'Architecture',            'Arch. Ricardo Lopez',    'Focuses on architectural design, building technology, urban planning, and sustainable construction.', 'CA')
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
