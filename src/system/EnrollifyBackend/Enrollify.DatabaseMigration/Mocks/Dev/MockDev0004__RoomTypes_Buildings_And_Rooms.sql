DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');


-- ######### Room Types ##############

CREATE TABLE #RoomTypes(
	[Name] VARCHAR(255) NOT NULL,
    [Description] VARCHAR(100) NOT NULL,
);

INSERT INTO #RoomTypes ([Name], [Description])
VALUES 
('Lecture Hall', 'Test'),
('Lecture Room', 'Test'),
('Laboratory', 'Test'),
('Seminar Room', 'Test'),
('Computer Lab', 'Test');

MERGE [RoomTypes] As [Target]
USING 
	(SELECT [Name], [Description] FROM #RoomTypes) AS [Source]
	ON [Target].[Name] = [Source].[Name]
WHEN MATCHED THEN
	UPDATE SET [Target].[Description] = [Source].[Description], [Target].[UpdatedBy] = @InitialUserId
WHEN NOT MATCHED THEN
	INSERT ([Name], [Description], [CreatedBy]) VALUES ([Source].[Name], [Source].[Description], @InitialUserId);

-- ######### Buildings ##############
CREATE TABLE #Buildings(
	[Name] VARCHAR(255) NOT NULL,
    [Description] VARCHAR(100) NOT NULL,
	[Address] VARCHAR(100) NOT NULL,
	[CollegeCode] CHAR(10) NOT NULL
);

INSERT INTO #Buildings ([Name], [Description], [Address], [CollegeCode])
VALUES ('CCS Main Building','CCS Main Building', 'San Isidro Campus', 'CCS');


MERGE [Buildings] As [Target]
USING 
	(
		SELECT B.Name, B.Description, B.Address, C.Id
		FROM #Buildings AS B
		JOIN Colleges C ON C.Code = B.CollegeCode
	) AS [Source] ([Name], [Description], [Address], [CollegeId])
	ON [Target].[Name] = [Source].[Name] AND [Target].[CollegeId] = [Source].[CollegeId]
WHEN MATCHED THEN
	UPDATE SET 
		[Target].[Name] = [Source].[Name], 
		[Target].[Description] = [Source].[Description], 
		[Target].[Address] = [Source].[Address], 
		[Target].[CollegeId] = [Source].[CollegeId],
		[Target].[UpdatedBy] = @InitialUserId, 
		[Target].[UpdatedAt] = GETUTCDATE()
WHEN NOT MATCHED THEN
	INSERT ([Name], [Description], [Address], [CollegeId], [CreatedBy], [CreatedAt])
	VALUES ([Source].[Name], [Source].[Description], [Source].[Address], [Source].[CollegeId], @InitialUserId, GETUTCDATE());


-- ######### Rooms ##############

CREATE TABLE #Rooms(
	[RoomNumber] VARCHAR(255) NOT NULL,
	[Capacity] INT,
	[RoomType] VARCHAR(100),
	[Building] VARCHAR(100),
);
INSERT INTO #Rooms ([RoomNumber], [Capacity], [RoomType], [Building])
VALUES 
('Lecture Room 101', 30, 'Lecture Room', 'CCS Main Building'),
('Lecture Room 102', 30, 'Lecture Room', 'CCS Main Building'),
('Computer Lab 202', 30, 'Computer Lab', 'CCS Main Building')
;


MERGE [Rooms] As [Target]
USING 
	(
		SELECT R.RoomNumber, R.Capacity, RT.Id As RoomTypeId, B.Id As BuildingId
		FROM #Rooms AS R 
		JOIN RoomTypes AS RT ON RT.Name = R.RoomType
		JOIN Buildings AS B ON B.Name = R.Building
	) As [Source]([RoomNumber], [Capacity], [RoomTypeId], [BuildingId])
	ON [Target].[RoomNumber] = [Source].[RoomNumber]
WHEN MATCHED THEN
	UPDATE SET 
		[Target].[Capacity] = [Source].[Capacity], 
		[Target].[RoomTypeId] = [Source].[RoomTypeId], 
		[Target].[BuildingId] = [Source].[BuildingId], 
		[Target].[UpdatedBy] = @InitialUserId,
		[Target].[UpdatedAt] = GETUTCDATE()
WHEN NOT MATCHED THEN
	INSERT ([RoomNumber], [Capacity], [RoomTypeId], [BuildingId], [CreatedBy])
	VALUES ([Source].[RoomNumber], [Source].[Capacity], [Source].[RoomTypeId], [Source].[BuildingId], @InitialUserId);


DROP TABLE #RoomTypes;
DROP TABLE #Buildings
DROP TABLE #Rooms

