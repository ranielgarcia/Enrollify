DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');

CREATE TABLE #TempUsers(
	[Email] VARCHAR(255) NOT NULL,
    [FirstName] VARCHAR(100) NOT NULL,
    [LastName] VARCHAR(100) NOT NULL
)

INSERT INTO #TempUsers ([Email], [FirstName], [LastName])
VALUES 
('ranielgarcia101@gmail.com', 'Raniel', 'Garcia');

MERGE [Users] As [Target]
USING 
    (SELECT [Email], [FirstName], [LastName] FROM #TempUsers) AS [Source]
    ON [Target].[Email] = [Source].[Email]
WHEN MATCHED THEN
    UPDATE SET [Target].[FirstName] = [Source].[FirstName], [Target].[LastName] = [Source].[LastName], [Target].[CreatedBy] = @InitialUserId
WHEN NOT MATCHED THEN
    INSERT ([Email], [FirstName], [LastName], [CreatedBy])
    VALUES ([Source].[Email], [Source].[FirstName], [Source].[LastName], @InitialUserId);