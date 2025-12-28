DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');

CREATE TABLE #TempColleges(
	[Code] VARCHAR(255) NOT NULL,
    [Name] VARCHAR(100) NOT NULL,
    [Dean] VARCHAR(100) NOT NULL,
    [Description] TEXT NOT NULL
)

INSERT INTO #TempColleges ([Code], [Name], [Dean], [Description])
VALUES 
('CCS', 'College of Computer Studies', 'Raniel Garcia', 'The College of Computer Studies (CCS) is an educational institution committed to its three-pronged vision of continually sharing knowledge and expertise through teaching, engaging in Computer Science research and Information Technology product development, and rendering service to communities in need.'),
('COE', 'College of Engineering', 'John Doe', 'The College of Engineering aims to develop and produce professional engineers and technologist imbued with desirable work ethics, leadership and entrepreneurial capability with the necessary knowledge and skills for them to become globally competitive and productive citizens of the country.')
;

MERGE [Colleges] As [Target]
USING 
    (SELECT [Code], [Name], [Dean], [Description] FROM #TempColleges) AS [Source]
    ON [Target].[Code] = [Source].[Code]
WHEN MATCHED THEN
    UPDATE SET [Target].[Name] = [Source].[Name], [Target].[Dean] = [Source].[Dean], [Target].[Description] = [Source].[Description], [Target].[UpdatedBy] = @InitialUserId, [Target].[UpdatedAt] = GETUTCDATE() 
WHEN NOT MATCHED THEN
    INSERT ([Code], [Name], [Dean], [Description], [CreatedBy], [CreatedAt])
    VALUES ([Source].[Code], [Source].[Name], [Source].[Dean], [Source].[Description], @InitialUserId, GETUTCDATE());