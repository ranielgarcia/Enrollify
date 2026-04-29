DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');

CREATE TABLE #TempTeachers(
	[FirstName]          VARCHAR(50)  NOT NULL,
	[MiddleName]         VARCHAR(50)  NOT NULL,
	[LastName]           VARCHAR(50)  NOT NULL,
	[TeacherIdentifier]  CHAR(20)     NOT NULL,
	[PhoneNumber]        CHAR(11)     NOT NULL,
	[Email]              VARCHAR(255) NOT NULL,
	[DepartmentCode]     VARCHAR(10)  NOT NULL,
	[CollegeCode]        VARCHAR(10)  NOT NULL,
	[AcademicTitle]      VARCHAR(100) NULL,
	[Qualification]      VARCHAR(255) NULL,
	[Specialization]     VARCHAR(255) NULL,
	[OfficeLocation]     VARCHAR(255) NULL,
	[OfficeHours]        VARCHAR(255) NULL,
	[Biography]          TEXT         NULL
)

INSERT INTO #TempTeachers
	([FirstName], [MiddleName], [LastName], [TeacherIdentifier], [PhoneNumber], [Email],
	 [DepartmentCode], [CollegeCode], [AcademicTitle], [Qualification], [Specialization],
	 [OfficeLocation], [OfficeHours], [Biography])
VALUES
-- Computer Science (CCS)
(
	'Ricardo', 'Andres', 'Bautista', 'TCHR-2024-0001', '09171234001', 'r.bautista@enrollify.edu',
	'CS', 'CCS', 'Associate Professor',
	'BS Computer Science, MS Computer Science (University of the Philippines)',
	'Artificial Intelligence, Machine Learning',
	'CCS Building, Room 201', 'MWF 10:00 AM - 12:00 PM',
	'Ricardo Bautista is an Associate Professor in the Computer Science department with over 10 years of teaching experience. His research interests include artificial intelligence and machine learning applications in education.'
),
(
	'Lourdes', 'Mariz', 'Reyes', 'TCHR-2024-0002', '09171234002', 'l.reyes@enrollify.edu',
	'CS', 'CCS', 'Professor',
	'BS Computer Science, MS Computer Science, PhD Computer Science (Ateneo de Manila University)',
	'Software Engineering, Algorithms',
	'CCS Building, Room 202', 'TTh 1:00 PM - 3:00 PM',
	'Dr. Lourdes Reyes is a full Professor and published researcher in software engineering. She has authored numerous papers on algorithm optimization and leads the department''s software development research group.'
),
-- Information Technology (CCS)
(
	'Ferdinand', 'Cruz', 'Santos', 'TCHR-2024-0003', '09171234003', 'f.santos@enrollify.edu',
	'IT', 'CCS', 'Assistant Professor',
	'BS Information Technology, MS Information Technology (De La Salle University)',
	'Network Administration, Cybersecurity',
	'CCS Building, Room 203', 'MWF 2:00 PM - 4:00 PM',
	'Ferdinand Santos is an Assistant Professor specializing in network administration and cybersecurity. He brings industry experience from his previous role as a senior network engineer in a telecommunications company.'
),
(
	'Elena', 'Grace', 'Torres', 'TCHR-2024-0004', '09171234004', 'e.torres@enrollify.edu',
	'IT', 'CCS', 'Instructor',
	'BS Information Technology, MS Information Systems (University of Santo Tomas)',
	'Web Development, Database Management',
	'CCS Building, Room 204', 'TTh 9:00 AM - 11:00 AM',
	'Elena Torres is an Instructor in the Information Technology department. She specializes in full-stack web development and database management, and actively mentors students in industry-aligned capstone projects.'
),
-- Civil Engineering (COE)
(
	'Roberto', 'Domingo', 'Lim', 'TCHR-2024-0005', '09181234001', 'r.lim@enrollify.edu',
	'CE', 'COE', 'Professor',
	'BS Civil Engineering, MS Civil Engineering, PhD Structural Engineering (University of the Philippines)',
	'Structural Engineering, Construction Management',
	'COE Building, Room 101', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Roberto Lim is a licensed civil engineer and full Professor with expertise in structural analysis and construction project management. He has overseen numerous government infrastructure projects before joining academia.'
),
(
	'Patricia', 'Natividad', 'Flores', 'TCHR-2024-0006', '09181234002', 'p.flores@enrollify.edu',
	'CE', 'COE', 'Assistant Professor',
	'BS Civil Engineering, MS Civil Engineering (Mapua University)',
	'Geotechnical Engineering, Environmental Engineering',
	'COE Building, Room 102', 'TTh 2:00 PM - 4:00 PM',
	'Patricia Flores is an Assistant Professor specializing in geotechnical and environmental engineering. Her research focuses on sustainable construction practices and soil stabilization techniques for local terrain conditions.'
),
-- Electrical Engineering (COE)
(
	'Antonio', 'Jose', 'Garcia', 'TCHR-2024-0007', '09181234003', 'a.garcia@enrollify.edu',
	'EE', 'COE', 'Associate Professor',
	'BS Electrical Engineering, MS Electrical Engineering (Mapua University)',
	'Power Systems, Industrial Automation',
	'COE Building, Room 201', 'MWF 1:00 PM - 3:00 PM',
	'Antonio Garcia is an Associate Professor and licensed electrical engineer with expertise in power systems design and industrial automation. He actively consults for energy sector firms while continuing his academic career.'
),
(
	'Carmela', 'Rose', 'Aquino', 'TCHR-2024-0008', '09181234004', 'c.aquino@enrollify.edu',
	'EE', 'COE', 'Associate Professor',
	'BS Electrical Engineering, MS Electrical Engineering, PhD Electrical Engineering (University of the Philippines)',
	'Electronics, Telecommunications',
	'COE Building, Room 202', 'TTh 10:00 AM - 12:00 PM',
	'Dr. Carmela Aquino is an Associate Professor whose research spans electronics design and telecommunications systems. She has received grants for her work on low-cost IoT solutions for rural infrastructure monitoring.'
)
;

MERGE [Teachers] AS [Target]
USING
    (
        SELECT
            t.[FirstName], t.[MiddleName], t.[LastName], t.[TeacherIdentifier], t.[PhoneNumber], t.[Email],
            t.[AcademicTitle], t.[Qualification], t.[Specialization], t.[OfficeLocation], t.[OfficeHours], t.[Biography],
            d.[Id] AS [DepartmentId]
        FROM #TempTeachers t
        INNER JOIN [Colleges]    c ON c.[Code] = t.[CollegeCode]
        INNER JOIN [Departments] d ON d.[Code] = t.[DepartmentCode] AND d.[CollegeId] = c.[Id]
    ) AS [Source]
    ON [Target].[TeacherIdentifier] = [Source].[TeacherIdentifier]
WHEN MATCHED THEN
    UPDATE SET
        [Target].[FirstName]       = [Source].[FirstName],
        [Target].[MiddleName]      = [Source].[MiddleName],
        [Target].[LastName]        = [Source].[LastName],
        [Target].[PhoneNumber]     = [Source].[PhoneNumber],
        [Target].[Email]           = [Source].[Email],
        [Target].[DepartmentId]    = [Source].[DepartmentId],
        [Target].[AcademicTitle]   = [Source].[AcademicTitle],
        [Target].[Qualification]   = [Source].[Qualification],
        [Target].[Specialization]  = [Source].[Specialization],
        [Target].[OfficeLocation]  = [Source].[OfficeLocation],
        [Target].[OfficeHours]     = [Source].[OfficeHours],
        [Target].[Biography]       = [Source].[Biography],
        [Target].[UpdatedBy]       = @InitialUserId,
        [Target].[UpdatedAt]       = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT ([FirstName], [MiddleName], [LastName], [TeacherIdentifier], [PhoneNumber], [Email],
            [DepartmentId], [AcademicTitle], [Qualification], [Specialization],
            [OfficeLocation], [OfficeHours], [Biography], [CreatedBy], [CreatedAt])
    VALUES ([Source].[FirstName], [Source].[MiddleName], [Source].[LastName], [Source].[TeacherIdentifier],
            [Source].[PhoneNumber], [Source].[Email], [Source].[DepartmentId], [Source].[AcademicTitle],
            [Source].[Qualification], [Source].[Specialization], [Source].[OfficeLocation],
            [Source].[OfficeHours], [Source].[Biography], @InitialUserId, GETUTCDATE());

DROP TABLE #TempTeachers;
