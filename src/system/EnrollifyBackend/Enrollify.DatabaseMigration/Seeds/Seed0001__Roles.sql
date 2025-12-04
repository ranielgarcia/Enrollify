DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');

CREATE TABLE #TempRoles
(
	[Id] INT,
	[Name] NVARCHAR(255) NOT NULL,
	[Description] TEXT
)

INSERT INTO #TempRoles ([Id], [Name], [Description]) 
VALUES
(1, 'SystemAdmin', 'The Super Admin holds the highest level of access within the system and has unrestricted control over all modules, settings, and data. This role is responsible for configuring global system parameters, managing all user accounts and permissions, overseeing security policies, and performing critical administrative operations.'),
(2, 'Admin', 'Responsible for managing day-to-day system operations within the scope defined by the Super Admin. This role oversees users, content, or configurations for their assigned tenant, department, or environment but does not have access to system-level settings. '),
(3, 'Registrar', 'Manage academic calendars and enrollment periods
Approve/reject enrollment requests and course add/drops
Handle course section creation and scheduling
Process transcript requests and academic records
Manage student status (active, LOA, graduated, etc.)
Override system blocks and restrictions'),
(4, 'FinanceOfficer', 'Process tuition and fee payments
Generate billing statements and assessment of fees
Apply discounts, scholarships, and financial aid
Issue official receipts
Manage payment plans and installments
Clear/block students based on payment status'),
(5, 'AdmissionOfficer', 'Process new student applications
Evaluate admission requirements
Assign student numbers
Handle student type classification (freshman, transferee, etc.)
Manage application status workflow'),
(6, 'DepartmentHead', 'Approve course offerings for their department
Manage faculty teaching loads
Override enrollment capacity limits
Handle special enrollment cases (overload, prerequisites waiver)
Review and approve class schedules'),
(7, 'AcademicAdvisor', 'Review and approve student study plans
Monitor academic progress and standing
Advise on course selection and prerequisites
Handle curriculum compliance checks'),
(8, 'ScholarshipCoordinator', 'Manage scholarship programs and eligibility
Process scholarship applications
Apply scholarship discounts to student accounts
Monitor scholarship retention requirements'),
(9, 'Teacher', 'View class rosters and student information
Input and manage grades
Mark attendance
Drop students from classes
View teaching schedules and room assignments'),
(10, 'ProgramCoordinator', 'Manage curriculum and course offerings for specific programs
Monitor program enrollment numbers
Handle program-specific requirements'),
(11, 'Student', 'Enroll in courses during enrollment period
View assessment and payment status
View class schedules, grades, and attendance
Request for documents (COR, grades, etc.)
Add/drop courses within allowed period'),
(12, 'ParentOrGuardian', 'View student grades and attendance
View billing and payment status
Receive notifications about academic performance')

SET IDENTITY_INSERT dbo.Roles ON;

MERGE [Roles] As [Target]
USING
	(SELECT [Id], [Name], [Description] FROM #TempRoles) AS [Source]
	ON [Target].[Id] = [Source].[Id]
WHEN MATCHED THEN
	UPDATE SET [Target].[Name] = [Source].[Name], [Target].[Description] = [Source].[Description]
WHEN NOT MATCHED THEN
	INSERT ([Id], [Name], [Description], [CreatedBy]) VALUES ([Source].[Id], [Source].[Name], [Source].[Description], @InitialUserId);

SET IDENTITY_INSERT dbo.Roles OFF;

-- Assign SystemAdmin role to the initial system user
MERGE [UserRolesAssignments] As [Target]
USING (VALUES(1, @InitialUserId)) AS [Source]([RoleId], [UserId])
	ON [Target].[UserId] = [Source].[UserId] AND [Target].[RoleId] = [Source].[RoleId]
WHEN NOT MATCHED THEN
	INSERT (UserId, RoleId, AssignedAt, CreatedBy) VALUES ([Source].[UserId], [Source].[RoleId], SYSDATETIMEOFFSET(), @InitialUserId);
