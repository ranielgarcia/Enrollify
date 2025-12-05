DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE Name='SystemAdmin');

-- Assign SystemAdmin role to the initial system user
MERGE [UserRolesAssignments] As [Target]
USING (VALUES(@RoleId, @InitialUserId)) AS [Source]([RoleId], [UserId])
	ON [Target].[UserId] = [Source].[UserId] AND [Target].[RoleId] = [Source].[RoleId]
WHEN NOT MATCHED THEN
	INSERT (UserId, RoleId, AssignedAt, CreatedBy) VALUES ([Source].[UserId], [Source].[RoleId], SYSDATETIMEOFFSET(), @InitialUserId);
