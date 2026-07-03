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
),
-- ===== COLLEGE OF BUSINESS ADMINISTRATION (CBA) =====

-- --- Business Administration Department ---
(
	'Maria', 'Concepcion', 'Villanueva', 'TCHR-2024-0009', '09181234005', 'mc.villanueva@enrollify.edu',
	'BA', 'CBA', 'Professor',
	'BS Business Administration, MBA, DBA (University of the Philippines)',
	'Strategic Management, Entrepreneurship, Organizational Development',
	'CBA Building, Room 301', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Maria Concepcion Villanueva is a full Professor of Business Administration with extensive experience in strategic management and entrepreneurship. She has founded two successful startups and brings real-world business acumen to her teaching.'
),
(
	'Jose', 'Mari', 'Reyes', 'TCHR-2024-0010', '09181234006', 'jm.reyes@enrollify.edu',
	'BA', 'CBA', 'Associate Professor',
	'BS Business Administration, MBA (Ateneo de Manila University)',
	'Marketing Management, Operations Management, Supply Chain Management',
	'CBA Building, Room 302', 'TTh 9:00 AM - 11:00 AM',
	'Jose Mari Reyes is an Associate Professor specializing in marketing and operations. He previously held senior marketing roles in multinational consumer goods companies before transitioning to academia.'
),
(
	'Luis', 'Antonio', 'Fernandez', 'TCHR-2024-0011', '09181234007', 'la.fernandez@enrollify.edu',
	'BA', 'CBA', 'Assistant Professor',
	'BS Business Administration, MBA (De La Salle University), MSc Finance (London School of Economics)',
	'Financial Management, Investments, International Business',
	'CBA Building, Room 303', 'MWF 1:00 PM - 3:00 PM',
	'Luis Antonio Fernandez is an Assistant Professor with a strong background in finance and international business. He brings international experience from his work in investment banking and trade finance.'
),
(
	'Marlene', 'Dela Cruz', 'Santos', 'TCHR-2024-0012', '09181234008', 'mdc.santos@enrollify.edu',
	'BA', 'CBA', 'Associate Professor',
	'BS Accounting, MBA, LLB (University of Santo Tomas)',
	'Business Law, Business Ethics, Governance and CSR',
	'CBA Building, Room 304', 'TTh 2:00 PM - 4:00 PM',
	'Marlene Dela Cruz Santos is an Associate Professor who combines legal and business expertise. She is a member of the Philippine Bar and previously practiced corporate law before pursuing her passion for teaching business law and ethics.'
),
(
	'Ramon', 'Carlos', 'Dimaano', 'TCHR-2024-0013', '09181234009', 'rc.dimaano@enrollify.edu',
	'BA', 'CBA', 'Instructor',
	'BS Business Administration, MS Statistics (University of the Philippines)',
	'Business Statistics, Business Research, Feasibility Study',
	'CBA Building, Room 305', 'MWF 10:00 AM - 12:00 PM',
	'Ramon Carlos Dimaano is an Instructor specializing in quantitative methods and business research. His background in statistics and data analytics supports students in developing evidence-based business solutions.'
),

-- --- Accountancy Department ---
(
	'Gloria', 'Santiago', 'Bautista', 'TCHR-2024-0014', '09181234010', 'g.santiago@enrollify.edu',
	'ACC', 'CBA', 'Professor',
	'BS Accountancy, MS Accountancy, PhD Business Administration (University of the Philippines)',
	'Financial Accounting, Financial Reporting, Consolidated Financial Statements',
	'CBA Building, Room 401', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Gloria Santiago Bautista is a Professor of Accountancy and a Certified Public Accountant (CPA). She has over 20 years of teaching experience and is a renowned author of accounting textbooks used nationwide.'
),
(
	'Antonio', 'Miguel', 'Mercado', 'TCHR-2024-0015', '09181234011', 'am.mercado@enrollify.edu',
	'ACC', 'CBA', 'Associate Professor',
	'BS Accountancy, MS Taxation, CPA (De La Salle University)',
	'Income Taxation, Business and Transfer Taxes, Government Accounting',
	'CBA Building, Room 402', 'TTh 9:00 AM - 11:00 AM',
	'Antonio Miguel Mercado is an Associate Professor and CPA with extensive expertise in taxation. He previously served as a tax consultant for one of the Big Four accounting firms and continues to advise on tax policy.'
),
(
	'Carmina', 'Villanueva', 'Martinez', 'TCHR-2024-0016', '09181234012', 'cv.martinez@enrollify.edu',
	'ACC', 'CBA', 'Associate Professor',
	'BS Accountancy, MS Accountancy, CPA (University of Santo Tomas)',
	'Intermediate Accounting, Partnership and Corporation Accounting, Accounting Information Systems',
	'CBA Building, Room 403', 'MWF 1:00 PM - 3:00 PM',
	'Carmina Villanueva Martinez is an Associate Professor and CPA specializing in intermediate accounting topics. She is known for her rigorous teaching approach that prepares students for the CPA licensure examination.'
),
(
	'Rogelio', 'Lopez', 'Rivera', 'TCHR-2024-0017', '09181234013', 'rl.rivera@enrollify.edu',
	'ACC', 'CBA', 'Assistant Professor',
	'BS Accountancy, MS Management Accounting, CPA (University of the East)',
	'Cost Accounting, Management Accounting, Assurance Principles',
	'CBA Building, Room 404', 'TTh 2:00 PM - 4:00 PM',
	'Rogelio Lopez Rivera is an Assistant Professor and CPA with industry experience in cost management and internal auditing. He previously managed the accounting department of a major manufacturing firm.'
),
(
	'Ma.', 'Lourdes', 'Gutierrez', 'TCHR-2024-0018', '09181234014', 'ml.gutierrez@enrollify.edu',
	'ACC', 'CBA', 'Instructor',
	'BS Accountancy, CPA (University of the Philippines)',
	'Accounting Ethics, Accounting Research, Practicum, Comprehensive Review',
	'CBA Building, Room 405', 'MWF 10:00 AM - 12:00 PM',
	'Ma. Lourdes Gutierrez is an Instructor and CPA who coordinates the accountancy practicum and review programs. She is passionate about mentoring students through their licensure examination journey.'
),

-- ===== COLLEGE OF EDUCATION (CED) =====

-- --- Secondary Education Department ---
(
	'Luzviminda', 'Mercado', 'Castillo', 'TCHR-2024-0019', '09181234015', 'lm.castillo@enrollify.edu',
	'SE', 'CED', 'Professor',
	'BS Education, MA Education, PhD Educational Management (University of the Philippines)',
	'Principles of Education, Social Foundations, Philosophical Foundations, History of Education',
	'CED Building, Room 101', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Luzviminda Mercado Castillo is a Professor of Education with deep expertise in the theoretical and historical foundations of the Philippine education system. She has served as a consultant to the Department of Education on curriculum reform.'
),
(
	'Ma.', 'Cristina', 'Mendoza', 'TCHR-2024-0020', '09181234016', 'mc.mendoza@enrollify.edu',
	'SE', 'CED', 'Assistant Professor',
	'BS Psychology, MA Educational Psychology, PhD Educational Psychology (Ateneo de Manila University)',
	'Educational Psychology, Facilitating Learning, Teaching Methodologies, Classroom Management',
	'CED Building, Room 102', 'TTh 9:00 AM - 11:00 AM',
	'Ma. Cristina Mendoza is an Assistant Professor specializing in educational psychology and pedagogical methods. Her research focuses on student motivation and effective teaching strategies for diverse learners.'
),
(
	'Ronaldo', 'Simeon', 'Dizon', 'TCHR-2024-0021', '09181234017', 'rs.dizon@enrollify.edu',
	'SE', 'CED', 'Associate Professor',
	'BS Education, MA Curriculum Development, PhD Curriculum Studies (University of the Philippines)',
	'Curriculum Development, Educational Technology, Special Education, Multicultural Education',
	'CED Building, Room 103', 'MWF 1:00 PM - 3:00 PM',
	'Dr. Ronaldo Simeon Dizon is an Associate Professor who leads curriculum innovation initiatives. He has developed competency-based curricula for multiple teacher education institutions and is a strong advocate for inclusive education.'
),
(
	'Divina', 'Pilar', 'Vergara', 'TCHR-2024-0022', '09181234018', 'dp.vergara@enrollify.edu',
	'SE', 'CED', 'Associate Professor',
	'BS Education, MA Measurement and Evaluation (University of Santo Tomas)',
	'Assessment of Learning 1, Assessment of Learning 2, Educational Statistics, Educational Research',
	'CED Building, Room 104', 'TTh 2:00 PM - 4:00 PM',
	'Divina Pilar Vergara is an Associate Professor specializing in educational assessment and research methodology. She has published widely on assessment practices and trains educators on outcomes-based evaluation.'
),
(
	'Ferdinand', 'Jose', 'Ramos', 'TCHR-2024-0023', '09181234019', 'fj.ramos@enrollify.edu',
	'SE', 'CED', 'Instructor',
	'BS Education, MA Education (Philippine Normal University)',
	'Child and Adolescent Development, Children and Adolescent Literature, Field Studies, Student Teaching, Seminar',
	'CED Building, Room 105', 'MWF 10:00 AM - 12:00 PM',
	'Ferdinand Jose Ramos is an Instructor who coordinates the field study and student teaching program. He has extensive experience supervising pre-service teachers in partner schools across the region.'
),

-- --- Early Childhood Education Department ---
(
	'Karen', 'Marie', 'Gomez', 'TCHR-2024-0024', '09181234020', 'km.gomez@enrollify.edu',
	'ECE', 'CED', 'Associate Professor',
	'BS Early Childhood Education, MA Child Development, PhD Early Childhood Education (University of the Philippines)',
	'Child Development, ECE Curriculum and Pedagogy, Health Safety and Nutrition, Special Education in ECE',
	'CED Building, Room 201', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Karen Marie Gomez is an Associate Professor in Early Childhood Education with extensive research on child development from prenatal to age eight. She established the university''s first ECE demonstration center.'
),
(
	'Angelica', 'Marie', 'Santos', 'TCHR-2024-0025', '09181234021', 'am.santos@enrollify.edu',
	'ECE', 'CED', 'Instructor',
	'BS Early Childhood Education, MA ECE (Philippine Normal University)',
	'Play-Based Learning, Language Development, Teaching Strategies in ECE, Observation and Documentation',
	'CED Building, Room 202', 'TTh 9:00 AM - 11:00 AM',
	'Angelica Marie Santos is an Instructor specializing in play-based pedagogy and early language development. She previously managed an early learning center and brings hands-on experience with young children.'
),
(
	'Roderick', 'Cruz', 'Valdez', 'TCHR-2024-0026', '09181234022', 'rc.valdez@enrollify.edu',
	'ECE', 'CED', 'Assistant Professor',
	'BS Early Childhood Education, MA ECE (University of Santo Tomas)',
	'Creative Arts and Expression, Math for Young Children, Science for Young Children, Music and Movement',
	'CED Building, Room 203', 'MWF 1:00 PM - 3:00 PM',
	'Roderick Cruz Valdez is an Assistant Professor who specializes in creative and content-area instruction for young learners. He integrates arts, music, and inquiry-based science into holistic ECE programs.'
),
(
	'Sofia', 'Cristina', 'Agustin', 'TCHR-2024-0027', '09181234023', 'sc.agustin@enrollify.edu',
	'ECE', 'CED', 'Assistant Professor',
	'BS Early Childhood Education, MA Reading Education (University of the Philippines)',
	'Children''s Literature, Social Studies for Young Children, Language Literacy and Communication, Assessment in ECE',
	'CED Building, Room 204', 'TTh 2:00 PM - 4:00 PM',
	'Sofia Cristina Agustin is an Assistant Professor with expertise in early literacy and assessment. She developed the university''s early literacy intervention program and trains teachers on developmentally appropriate assessment practices.'
),
(
	'Jennica', 'Paula', 'Reyes', 'TCHR-2024-0028', '09181234024', 'jp.reyes@enrollify.edu',
	'ECE', 'CED', 'Instructor',
	'BS Early Childhood Education (Philippine Normal University)',
	'Parent and Community Engagement, ECE Program Administration, Field Study, Practice Teaching, Research in ECE',
	'CED Building, Room 205', 'MWF 10:00 AM - 12:00 PM',
	'Jennica Paula Reyes is an Instructor who coordinates the ECE practicum and field study programs. She is passionate about building strong school-family-community partnerships in early childhood settings.'
),

-- ===== COLLEGE OF ARTS AND SCIENCES (CAS) =====

-- --- English Language Studies Department ---
(
	'Emmanuel', 'Cruz', 'Rivera', 'TCHR-2024-0029', '09191234001', 'ec.rivera@enrollify.edu',
	'ENG', 'CAS', 'Professor',
	'BA English, MA Linguistics, PhD Linguistics (University of the Philippines)',
	'Introduction to Linguistics, Phonetics and Phonology, Morphology, Syntax, Semantics',
	'CAS Building, Room 101', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Emmanuel Cruz Rivera is a Professor of Linguistics with a specialization in theoretical and descriptive linguistics. His research on Philippine languages has been published in international linguistics journals.'
),
(
	'Patricia', 'Anne', 'Rivera', 'TCHR-2024-0030', '09191234002', 'pa.rivera@enrollify.edu',
	'ENG', 'CAS', 'Assistant Professor',
	'BA English, MA Literature (Ateneo de Manila University)',
	'Survey of English Literature, Survey of American Literature, Philippine Literature in English, Survey of World Literature, Literary Criticism',
	'CAS Building, Room 102', 'TTh 9:00 AM - 11:00 AM',
	'Patricia Anne Rivera is an Assistant Professor of Literature who brings postcolonial and feminist perspectives to her teaching. She is currently completing her doctoral dissertation on contemporary Philippine anglophone fiction.'
),
(
	'Luigi', 'Miguel', 'Tolentino', 'TCHR-2024-0031', '09191234003', 'lm.tolentino@enrollify.edu',
	'ENG', 'CAS', 'Associate Professor',
	'BA English, MA English Studies, PhD Applied Linguistics (De La Salle University)',
	'Pragmatics, Sociolinguistics, Psycholinguistics, Discourse Analysis, Stylistics',
	'CAS Building, Room 103', 'MWF 1:00 PM - 3:00 PM',
	'Dr. Luigi Miguel Tolentino is an Associate Professor of Applied Linguistics. His interdisciplinary research bridges linguistics, psychology, and sociology to understand language use in social contexts.'
),
(
	'Catherine', 'Marie', 'Domingo', 'TCHR-2024-0032', '09191234004', 'cm.domingo@enrollify.edu',
	'ENG', 'CAS', 'Assistant Professor',
	'BA English, MA Creative Writing (University of Santo Tomas)',
	'English Grammar and Structure, Composition and Rhetoric, Creative Writing, Translation Studies, Language Acquisition',
	'CAS Building, Room 104', 'TTh 2:00 PM - 4:00 PM',
	'Catherine Marie Domingo is an Assistant Professor and published author specializing in creative writing and composition. Her short story collection won the Philippine National Book Award.'
),
(
	'Danilo', 'Fernando', 'Sison', 'TCHR-2024-0033', '09191234005', 'df.sison@enrollify.edu',
	'ENG', 'CAS', 'Instructor',
	'BA English, MA English Studies (University of the Philippines)',
	'Technical Writing, Argumentation and Debate, Journalism, Contemporary and Popular Literature, Research in English Studies',
	'CAS Building, Room 105', 'MWF 10:00 AM - 12:00 PM',
	'Danilo Fernando Sison is an Instructor with a background in journalism and professional writing. He worked as a senior correspondent for a major Philippine broadsheet before joining the academe.'
),

-- --- Psychology Department ---
(
	'Jonathan', 'David', 'Tan', 'TCHR-2024-0034', '09191234006', 'jd.tan@enrollify.edu',
	'PSY', 'CAS', 'Associate Professor',
	'BS Psychology, MA Clinical Psychology, PhD Clinical Psychology (Ateneo de Manila University)',
	'General Psychology, Biological Psychology, Developmental Psychology, Motivation and Emotion, Theories of Personality',
	'CAS Building, Room 201', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Jonathan David Tan is an Associate Professor of Psychology with a clinical specialization. He maintains a limited private practice and integrates clinical insights into his teaching of foundational psychology courses.'
),
(
	'Isabelita', 'de Jesus', 'Cruz', 'TCHR-2024-0035', '09191234007', 'idj.cruz@enrollify.edu',
	'PSY', 'CAS', 'Assistant Professor',
	'BS Psychology, MA Industrial Psychology, PhD Psychology (University of the Philippines)',
	'Social Psychology, Abnormal Psychology, Clinical Psychology, Health Psychology, Forensic Psychology',
	'CAS Building, Room 202', 'TTh 9:00 AM - 11:00 AM',
	'Isabelita de Jesus Cruz is an Assistant Professor whose research spans clinical, social, and forensic psychology. She has served as an expert witness in court cases involving psychological evaluation.'
),
(
	'Jose', 'Ramon', 'Villanueva', 'TCHR-2024-0036', '09191234008', 'jr.villanueva@enrollify.edu',
	'PSY', 'CAS', 'Associate Professor',
	'BS Psychology, MA Experimental Psychology, PhD Cognitive Science (University of the Philippines)',
	'Cognitive Psychology, Experimental Psychology, Psychological Testing, Statistics for Psychology',
	'CAS Building, Room 203', 'MWF 1:00 PM - 3:00 PM',
	'Dr. Jose Ramon Villanueva is an Associate Professor specializing in experimental and cognitive psychology. He directs the university''s psychometrics laboratory and leads research on cognitive assessment instruments.'
),
(
	'Maria', 'Cecilia', 'Fernandez', 'TCHR-2024-0037', '09191234009', 'mc.fernandez@enrollify.edu',
	'PSY', 'CAS', 'Assistant Professor',
	'BS Psychology, MA Industrial Psychology (De La Salle University)',
	'Industrial/Organizational Psychology, Human Resource Psychology, Psychology of Gender, Multicultural Psychology, Basic Counseling Skills',
	'CAS Building, Room 204', 'TTh 2:00 PM - 4:00 PM',
	'Maria Cecilia Fernandez is an Assistant Professor specializing in industrial-organizational psychology. She consults for corporations on employee selection, training, and organizational development programs.'
),
(
	'Rafael', 'Mateo', 'Garcia', 'TCHR-2024-0038', '09191234010', 'rm.garcia@enrollify.edu',
	'PSY', 'CAS', 'Instructor',
	'BS Psychology, MA Psychology (University of Santo Tomas)',
	'Research Methods in Psychology, Field Experience in Psychology, Ethics in Psychology, Seminar in Psychology, Psychology Capstone',
	'CAS Building, Room 205', 'MWF 10:00 AM - 12:00 PM',
	'Rafael Mateo Garcia is an Instructor who coordinates the psychology practicum and capstone programs. He ensures students gain meaningful field experience through partnerships with mental health organizations.'
),

-- ===== COLLEGE OF NURSING (CN) =====

-- --- Nursing Department ---
(
	'Ma.', 'Fatima', 'Alcantara', 'TCHR-2024-0039', '09191234011', 'mf.alcantara@enrollify.edu',
	'NUR', 'CN', 'Professor',
	'BS Nursing, MAN, PhD Nursing (University of the Philippines)',
	'Fundamentals of Nursing, Medical-Surgical Nursing, Anatomy and Physiology, Biochemistry',
	'CN Building, Room 101', 'MWF 8:00 AM - 10:00 AM',
	'Dr. Ma. Fatima Alcantara is a Professor of Nursing and a registered nurse (RN). She has over 25 years of clinical and academic experience and is recognized for her contributions to nursing education and curriculum development.'
),
(
	'Rafael', 'Mercado', 'Santos', 'TCHR-2024-0040', '09191234012', 'rm.santos@enrollify.edu',
	'NUR', 'CN', 'Associate Professor',
	'BS Nursing, MAN, RN (University of Santo Tomas)',
	'Community Health Nursing, Epidemiology, Psychiatric Nursing, Nursing Management and Leadership',
	'CN Building, Room 102', 'TTh 9:00 AM - 11:00 AM',
	'Rafael Mercado Santos is an Associate Professor and RN with expertise in community and psychiatric nursing. He previously served as a public health nurse coordinator for a provincial health office.'
),
(
	'Maria', 'Teresa', 'Dela Cruz', 'TCHR-2024-0041', '09191234013', 'mt.delacruz@enrollify.edu',
	'NUR', 'CN', 'Assistant Professor',
	'BS Nursing, MAN, RN (University of the Philippines)',
	'Maternal and Child Nursing, Pediatric Nursing, Gerontologic Nursing, Oncology Nursing',
	'CN Building, Room 103', 'MWF 1:00 PM - 3:00 PM',
	'Maria Teresa Dela Cruz is an Assistant Professor and RN specializing in maternal-child and pediatric nursing. She previously worked as a head nurse in the obstetrics and pediatrics ward of a tertiary hospital.'
),
(
	'Juan', 'Carlos', 'Villamor', 'TCHR-2024-0042', '09191234014', 'jc.villamor@enrollify.edu',
	'NUR', 'CN', 'Assistant Professor',
	'BS Nursing, MAN, RN (De La Salle University)',
	'Health Assessment, Pharmacology, Pathophysiology, Immunology, Microbiology',
	'CN Building, Room 104', 'TTh 2:00 PM - 4:00 PM',
	'Juan Carlos Villamor is an Assistant Professor and RN specializing in the basic sciences underpinning nursing practice. His teaching emphasizes the integration of pathophysiology and pharmacology in patient care.'
),
(
	'Anna', 'Liza', 'Bernardo', 'TCHR-2024-0043', '09191234015', 'al.bernardo@enrollify.edu',
	'NUR', 'CN', 'Instructor',
	'BS Nursing, RN, MAN candidate (University of Santo Tomas)',
	'Nutrition and Diet Therapy, Introduction to Nursing, Emergency and Disaster Nursing, Intensive Care Nursing',
	'CN Building, Room 105', 'MWF 10:00 AM - 12:00 PM',
	'Anna Liza Bernardo is an Instructor and RN with experience in emergency and critical care nursing. She worked in the ICU and emergency department of a major hospital before joining the faculty.'
),
(
	'Catherine', 'Marie', 'Lopez', 'TCHR-2024-0044', '09191234016', 'cm.lopez@enrollify.edu',
	'NUR', 'CN', 'Instructor',
	'BS Nursing, RN (University of the Philippines)',
	'Nursing Research, Nursing Ethics, RLE 1-4, Clinical Immersion',
	'CN Building, Room 106', 'TTh 10:00 AM - 12:00 PM',
	'Catherine Marie Lopez is an Instructor and RN who coordinates the Related Learning Experience (RLE) program. She ensures that nursing students develop clinical competencies through structured simulation and hospital-based training.'
),

-- ===== COLLEGE OF ARCHITECTURE (CA) =====

-- --- Architecture Department ---
(
	'Eduardo', 'Ramon', 'Mercado', 'TCHR-2024-0045', '09201234001', 'er.mercado@enrollify.edu',
	'ARCH', 'CA', 'Associate Professor',
	'BS Architecture, MA Architecture (University of the Philippines), UAP',
	'Architectural Design 1-4, Visual Techniques 1-2, Architectural Theory',
	'CA Building, Room 101', 'MWF 8:00 AM - 11:00 AM',
	'Arch. Eduardo Ramon Mercado is an Associate Professor and registered architect (UAP). He brings extensive design studio experience and has won multiple design awards for his residential and commercial projects.'
),
(
	'Maria', 'Elena', 'Vergara', 'TCHR-2024-0046', '09201234002', 'me.vergara@enrollify.edu',
	'ARCH', 'CA', 'Professor',
	'BS Architecture, MA Urban Design, PhD Urban Planning (University of the Philippines), UAP, PIEP',
	'Architectural Design 5-8, Visual Techniques 3, Urban Planning, Sustainable Architecture',
	'CA Building, Room 102', 'TTh 9:00 AM - 12:00 PM',
	'Arch. Maria Elena Vergara is a Professor and registered architect with a specialization in urban planning and sustainable design. She has led major urban master planning projects for Philippine cities.'
),
(
	'Ricardo', 'Nicolas', 'Castillo', 'TCHR-2024-0047', '09201234003', 'rn.castillo@enrollify.edu',
	'ARCH', 'CA', 'Associate Professor',
	'BS Architecture, MS Structural Engineering (Mapua University), UAP, ASEP',
	'History of Architecture 1-3, Architectural Theory, Building Structures 1-3, Sustainable Architecture',
	'CA Building, Room 103', 'MWF 1:00 PM - 4:00 PM',
	'Arch. Ricardo Nicolas Castillo is an Associate Professor and registered architect with dual expertise in architectural history and building structures. He is a member of the Association of Structural Engineers of the Philippines.'
),
(
	'Ma.', 'Lourdes', 'Natividad', 'TCHR-2024-0048', '09201234004', 'ml.natividad@enrollify.edu',
	'ARCH', 'CA', 'Assistant Professor',
	'BS Architecture, MS Architecture (University of Santo Tomas), UAP',
	'Building Materials, Building Utilities 1-3, Building Codes and Professional Practice, Cost Estimating',
	'CA Building, Room 104', 'TTh 2:00 PM - 5:00 PM',
	'Ma. Lourdes Natividad is an Assistant Professor and registered architect with industry experience in construction documentation and project management. She specializes in building technology and professional practice.'
),
(
	'Michael', 'Angelo', 'Samonte', 'TCHR-2024-0049', '09201234005', 'ma.samonte@enrollify.edu',
	'ARCH', 'CA', 'Instructor',
	'BS Architecture, UAP',
	'CAD 1-2, Construction Management, Research Methods in Architecture, Architectural Thesis',
	'CA Building, Room 105', 'MWF 10:00 AM - 12:00 PM',
	'Michael Angelo Samonte is an Instructor and registered architect specializing in digital design technology and construction management. He coordinates the architectural thesis program and mentors graduating students.'
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
