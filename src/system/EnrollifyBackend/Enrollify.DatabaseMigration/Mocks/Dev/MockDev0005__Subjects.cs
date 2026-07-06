using DbUp.Engine;
using Enrollify.Core.Aggregates.SubjectAggregate;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Mocks.Dev;

public class MockDev0005__Subjects : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var subjects = new List<Subject>()
        {
            // General Education Subjects
            new Subject(SubjectCode.From("GE-MATH1"), "Mathematics in the Modern World", 3.0m, "Study of mathematics as a tool for understanding the world.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-ENG1"), "Purposive Communication", 3.0m, "Development of communication skills for various purposes.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-FIL1"), "Kontekswalisadong Komunikasyon sa Filipino", 3.0m, "Filipino communication in various contexts.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-SCI1"), "Science, Technology, and Society", 3.0m, "Study of the interaction between science, technology, and society.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-ETHICS"), "Ethics", 3.0m, "Study of moral principles and ethical decision-making.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-RIZAL"), "Life and Works of Rizal", 3.0m, "Study of the life, works, and writings of Jose Rizal.", "Lecture Room"),
            new Subject(SubjectCode.From("GE-PE1"), "Physical Education 1", 2.0m, "Foundation of physical fitness and wellness.", "Gymnasium"),
            new Subject(SubjectCode.From("GE-PE2"), "Physical Education 2", 2.0m, "Team sports and recreational activities.", "Gymnasium"),

            // Mathematics and Science Subjects
            new Subject(SubjectCode.From("MATH-CALC1"), "Calculus 1", 3.0m, "Differential calculus and its applications.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-CALC2"), "Calculus 2", 3.0m, "Integral calculus and its applications.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-DISCR"), "Discrete Mathematics", 3.0m, "Mathematical structures for computer science.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-LINALG"), "Linear Algebra", 3.0m, "Study of vectors, matrices, and linear transformations.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-PROB"), "Probability and Statistics", 3.0m, "Statistical methods and probability theory.", "Lecture Room"),
            new Subject(SubjectCode.From("MATH-NUMER"), "Numerical Methods", 3.0m, "Computational methods for solving mathematical problems.", "Computer Lab"),

            // Core IT/CS Subjects
            new Subject(SubjectCode.From("CC-PROG1"), "Introduction to Programming", 3.0m, "Fundamentals of programming using a high-level language.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-PROG2"), "Intermediate Programming", 3.0m, "Advanced programming concepts and data structures.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-PROG3"), "Advanced Programming", 3.0m, "Complex programming paradigms and design patterns.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DSTRUC"), "Data Structures and Algorithms", 3.0m, "Study of fundamental data structures and algorithm design.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-OOP"), "Object-Oriented Programming", 3.0m, "Principles and practices of object-oriented programming.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DBMS"), "Database Management Systems", 3.0m, "Design, implementation, and management of database systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-NETW1"), "Fundamentals of Networking", 3.0m, "Introduction to computer networks and data communications.", "Laboratory"),
            new Subject(SubjectCode.From("CC-NETW2"), "Advanced Networking", 3.0m, "Advanced network protocols and configurations.", "Laboratory"),
            new Subject(SubjectCode.From("CC-OS"), "Operating Systems", 3.0m, "Concepts and principles of operating systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-WEBDEV"), "Web Development", 3.0m, "Design and development of web-based applications.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-DIGLOG"), "Digital Logic Design", 3.0m, "Fundamentals of digital circuits and logic design.", "Laboratory"),
            new Subject(SubjectCode.From("CC-COMORG"), "Computer Organization and Architecture", 3.0m, "Study of computer hardware organization and architecture.", "Lecture Room"),
            new Subject(SubjectCode.From("CC-HCI"), "Human-Computer Interaction", 3.0m, "Principles of designing user-friendly interfaces.", "Computer Lab"),
            new Subject(SubjectCode.From("CC-TECHWR"), "Technical Writing", 3.0m, "Writing technical documents and reports.", "Lecture Room"),

            // IT-Specific Subjects
            new Subject(SubjectCode.From("IT-SYSAD"), "Systems Administration", 3.0m, "Administration and management of IT systems.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-INFMGT"), "Information Management", 3.0m, "Management and organization of information resources.", "Lecture Room"),
            new Subject(SubjectCode.From("IT-NETSEC"), "Network Security", 3.0m, "Principles and practices of securing computer networks.", "Laboratory"),
            new Subject(SubjectCode.From("IT-MOBDEV"), "Mobile Application Development", 3.0m, "Development of applications for mobile platforms.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-CLOUD"), "Cloud Computing", 3.0m, "Fundamentals of cloud infrastructure and services.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-DEVOPS"), "DevOps Practices", 3.0m, "Continuous integration, delivery, and deployment practices.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-SYSINT"), "Systems Integration and Architecture", 3.0m, "Enterprise systems integration patterns.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-ITPROJ"), "IT Project Management", 3.0m, "Project management methodologies for IT projects.", "Lecture Room"),
            new Subject(SubjectCode.From("IT-BUSANA"), "Business Analytics", 3.0m, "Data-driven decision making for business.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-ECOMM"), "E-Commerce Technologies", 3.0m, "Technologies and platforms for electronic commerce.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-MULMED"), "Multimedia Systems", 3.0m, "Design and development of multimedia applications.", "Computer Lab"),
            new Subject(SubjectCode.From("IT-QUALTY"), "IT Service Quality Management", 3.0m, "Quality assurance in IT service delivery.", "Lecture Room"),

            // CS-Specific Subjects
            new Subject(SubjectCode.From("CS-ALGO"), "Algorithm Design and Analysis", 3.0m, "Advanced study of algorithm design techniques and analysis.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-AUTOMATA"), "Automata Theory and Formal Languages", 3.0m, "Study of abstract machines and formal languages.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-AI"), "Artificial Intelligence", 3.0m, "Fundamentals of artificial intelligence and machine learning.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-SOFTENG"), "Software Engineering", 3.0m, "Principles and methodologies of software development.", "Lecture Room"),
            new Subject(SubjectCode.From("CS-COMPIL"), "Compiler Design", 3.0m, "Design and implementation of programming language compilers.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-ML"), "Machine Learning", 3.0m, "Statistical learning algorithms and applications.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-DATSCI"), "Data Science", 3.0m, "Data analysis, visualization, and predictive modeling.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-DISTSY"), "Distributed Systems", 3.0m, "Design and implementation of distributed computing systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-PARSYS"), "Parallel Computing", 3.0m, "Parallel algorithms and programming techniques.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-COMPGR"), "Computer Graphics", 3.0m, "Fundamentals of 2D and 3D computer graphics.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-CYBERS"), "Cybersecurity Fundamentals", 3.0m, "Security principles, threats, and countermeasures.", "Laboratory"),
            new Subject(SubjectCode.From("CS-GAMEDEV"), "Game Development", 3.0m, "Design and development of computer games.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-NLPROC"), "Natural Language Processing", 3.0m, "Computational techniques for processing human language.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-IMGPRC"), "Image Processing", 3.0m, "Digital image processing techniques and applications.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-BIGDAT"), "Big Data Analytics", 3.0m, "Processing and analyzing large-scale datasets.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-IOTSYS"), "Internet of Things", 3.0m, "Design and implementation of IoT systems.", "Laboratory"),
            new Subject(SubjectCode.From("CS-BLKCHN"), "Blockchain Technology", 3.0m, "Fundamentals of blockchain and decentralized systems.", "Computer Lab"),
            new Subject(SubjectCode.From("CS-QUANT"), "Quantum Computing", 3.0m, "Introduction to quantum computing concepts.", "Lecture Room"),

            // Professional and Elective Subjects
            new Subject(SubjectCode.From("PROF-ETHICS"), "Professional Ethics in Computing", 3.0m, "Ethical issues and responsibilities in computing profession.", "Lecture Room"),
            new Subject(SubjectCode.From("PROF-LAW"), "IT Laws and Policies", 3.0m, "Legal frameworks governing information technology.", "Lecture Room"),
            new Subject(SubjectCode.From("PROF-ENTREP"), "Technopreneurship", 3.0m, "Entrepreneurship in technology-based ventures.", "Lecture Room"),
            new Subject(SubjectCode.From("PROF-OJT"), "On-the-Job Training", 6.0m, "Industry immersion and practical training.", "Seminar Room"),

            // Capstone/Thesis
            new Subject(SubjectCode.From("CAP-THESIS1"), "Capstone Project 1", 3.0m, "First phase of capstone project development.", "Seminar Room"),
            new Subject(SubjectCode.From("CAP-THESIS2"), "Capstone Project 2", 3.0m, "Second phase of capstone project development.", "Seminar Room"),

            // ===== COLLEGE OF BUSINESS ADMINISTRATION (CBA) =====

            // --- BSBA (Business Administration) Core Subjects ---
            new Subject(SubjectCode.From("BA-ECON1"), "Principles of Economics (Micro)", 3.0m, "Study of individual economic decision-making, supply and demand, and market structures.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-ECON2"), "Principles of Economics (Macro)", 3.0m, "Study of national income, inflation, unemployment, fiscal and monetary policy.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-ACC1"), "Basic Accounting", 3.0m, "Fundamentals of accounting, the accounting cycle, and preparation of financial statements.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-ACC2"), "Intermediate Accounting", 3.0m, "Advanced accounting topics including partnerships, corporations, and special journals.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-PMGT"), "Principles of Management", 3.0m, "Fundamental concepts of planning, organizing, leading, and controlling organizations.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-MKTG"), "Marketing Management", 3.0m, "Principles of marketing, consumer behavior, market research, and promotional strategies.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-FIN1"), "Financial Management", 3.0m, "Principles of financial decision-making, capital budgeting, and working capital management.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-FIN2"), "Investment and Portfolio Management", 3.0m, "Analysis of securities, portfolio theory, risk management, and investment strategies.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-HRM"), "Human Resource Management", 3.0m, "Recruitment, selection, training, compensation, and employee relations.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-OPMGT"), "Operations Management", 3.0m, "Design, operation, and improvement of production systems and processes.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-SCM"), "Supply Chain Management", 3.0m, "Management of material, information, and financial flows across the supply chain.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-BUSLAW"), "Business Law and Regulations", 3.0m, "Legal principles governing contracts, sales, agency, and business organizations.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-BUSETH"), "Business Ethics and Social Responsibility", 3.0m, "Ethical frameworks and corporate social responsibility in business decision-making.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-BUSCOM"), "Business Communication", 3.0m, "Effective written and oral communication for business contexts.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-STAT"), "Business Statistics", 3.0m, "Statistical methods for data analysis and decision-making in business.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-ENTREP"), "Entrepreneurship", 3.0m, "Opportunity identification, business model development, and venture creation.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-ORGDEV"), "Organizational Development", 3.0m, "Planned change interventions to improve organizational effectiveness.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-TQM"), "Total Quality Management", 3.0m, "Quality principles, continuous improvement, and customer-focused management.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-SMGT"), "Strategic Management", 3.0m, "Formulation, implementation, and evaluation of organizational strategies.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-INTLBU"), "International Business and Trade", 3.0m, "Global business environment, trade theories, and cross-cultural management.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-FEASIB"), "Feasibility Study and Business Planning", 3.0m, "Comprehensive business planning, market analysis, financial projections, and risk assessment.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-BUSRES"), "Business Research Methods", 3.0m, "Research design, data collection, and analysis for business problem-solving.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-GOVCOR"), "Governance and Corporate Social Responsibility", 3.0m, "Principles of good corporate governance and responsible business conduct.", "Lecture Room"),
            new Subject(SubjectCode.From("BA-PRAC1"), "Business Practicum 1", 3.0m, "Supervised industry exposure and application of business concepts.", "Seminar Room"),
            new Subject(SubjectCode.From("BA-PRAC2"), "Business Practicum 2", 3.0m, "Advanced industry immersion with mentored business project delivery.", "Seminar Room"),

            // --- BSA (Accountancy) Core Subjects ---
            new Subject(SubjectCode.From("ACC-INTAC1"), "Intermediate Accounting 1", 3.0m, "Comprehensive study of asset valuation, revenue recognition, and financial statement preparation.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-INTAC2"), "Intermediate Accounting 2", 3.0m, "Accounting for liabilities, equity, leases, pensions, and income taxes.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-INTAC3"), "Intermediate Accounting 3", 3.0m, "Advanced topics including derivatives, foreign currency, and earnings per share.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-FAR1"), "Financial Accounting and Reporting 1", 3.0m, "Conceptual framework, accounting standards, and preparation of general-purpose financial statements.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-FAR2"), "Financial Accounting and Reporting 2", 3.0m, "Advanced financial reporting topics including business combinations and segment reporting.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-FAR3"), "Financial Accounting and Reporting 3", 3.0m, "Specialized accounting areas including government, not-for-profit, and international reporting.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-PARA"), "Partnership and Corporation Accounting", 3.0m, "Accounting for partnership formation, operations, dissolution, and corporate transactions.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-COST"), "Cost Accounting and Cost Management", 3.0m, "Cost accumulation, costing methods, budgeting, and variance analysis.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-MGTACC"), "Management Accounting", 3.0m, "Managerial use of accounting information for planning, control, and decision support.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-AUD1"), "Auditing Principles", 3.0m, "Audit framework, risk assessment, internal control, and audit planning.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-AUD2"), "Auditing Practice", 3.0m, "Audit execution, evidence gathering, reporting, and ethical considerations.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-ASSUR"), "Assurance Principles and Practices", 3.0m, "Assurance engagements beyond historical financial audits.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-TAX"), "Income Taxation", 3.0m, "Philippine income tax laws, computation of taxable income, and tax compliance.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-TAX2"), "Business and Transfer Taxes", 3.0m, "Value-added tax, percentage taxes, estate tax, and donor's tax.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-GOVACC"), "Government Accounting", 3.0m, "Accounting and auditing principles specific to government entities.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-ACCSYS"), "Accounting Information Systems", 3.0m, "Design and implementation of accounting systems and internal controls.", "Computer Lab"),
            new Subject(SubjectCode.From("ACC-BUSLAW"), "Business Law for Accountants", 3.0m, "Legal concepts relevant to accounting practice including obligations and contracts.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-CONAC"), "Consolidated Financial Statements", 3.0m, "Preparation of consolidated financial statements for parent-subsidiary entities.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-FSAN"), "Financial Statement Analysis", 3.0m, "Techniques for analyzing financial statements to evaluate business performance.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-ETH"), "Accounting Ethics and Professional Values", 3.0m, "Professional ethics, integrity, and responsibilities of the accounting profession.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-RES"), "Accounting Research Methods", 3.0m, "Research methodologies applied to accounting problems and practice.", "Lecture Room"),
            new Subject(SubjectCode.From("ACC-PRAC"), "Accounting Practicum", 3.0m, "Practical application of accounting skills in a supervised environment.", "Seminar Room"),
            new Subject(SubjectCode.From("ACC-REV"), "Accountancy Comprehensive Review", 3.0m, "Integrated review of accounting concepts in preparation for professional licensure.", "Lecture Room"),

            // ===== COLLEGE OF EDUCATION (CED) =====

            // --- BSEd (Secondary Education) Core Subjects ---
            new Subject(SubjectCode.From("ED-PRIN"), "Principles of Education", 3.0m, "Foundational concepts, philosophies, and theories of education.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-PSYCH"), "Educational Psychology", 3.0m, "Psychological theories and their application to teaching and learning.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-CURR"), "Curriculum Development", 3.0m, "Principles and processes of curriculum design, implementation, and evaluation.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-ASSMT1"), "Assessment of Learning 1", 3.0m, "Assessment principles, test construction, and traditional assessment methods.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-ASSMT2"), "Assessment of Learning 2", 3.0m, "Alternative assessment methods, portfolio assessment, and authentic evaluation.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-TECH"), "Educational Technology", 3.0m, "Integration of technology in teaching and learning environments.", "Computer Lab"),
            new Subject(SubjectCode.From("ED-SPEC"), "Foundations of Special Education", 3.0m, "Principles and practices for teaching learners with special needs.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-TEACH1"), "Teaching Methodologies 1", 3.0m, "Instructional strategies, lesson planning, and classroom delivery techniques.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-TEACH2"), "Teaching Methodologies 2", 3.0m, "Advanced pedagogical approaches including differentiated instruction.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-SOCFND"), "Social Foundations of Education", 3.0m, "Sociological and cultural influences on education systems and practices.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-FILFND"), "Philosophical Foundations of Education", 3.0m, "Philosophical perspectives that shape educational thought and practice.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-HIST"), "History of Education", 3.0m, "Historical development of educational systems and reform movements.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-CLASMG"), "Classroom Management", 3.0m, "Strategies for creating a positive, productive learning environment.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-STATS"), "Educational Statistics", 3.0m, "Statistical methods for analyzing educational data and research findings.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-RES"), "Educational Research", 3.0m, "Quantitative and qualitative research methods in education.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-CHILD"), "Child and Adolescent Development", 3.0m, "Physical, cognitive, social, and emotional development across school-age years.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-FACIL"), "Facilitating Learning", 3.0m, "Learning theories and strategies to facilitate effective student learning.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-MULTIC"), "Multicultural Education", 3.0m, "Diversity, equity, and inclusion in educational settings.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-LIT"), "Children and Adolescent Literature", 3.0m, "Literary works appropriate for children and adolescents across genres.", "Lecture Room"),
            new Subject(SubjectCode.From("ED-FLDS1"), "Field Study 1 (Observations)", 3.0m, "Classroom observation and documentation of teaching-learning processes.", "Seminar Room"),
            new Subject(SubjectCode.From("ED-FLDS2"), "Field Study 2 (Participation)", 3.0m, "Guided participation in classroom teaching and school activities.", "Seminar Room"),
            new Subject(SubjectCode.From("ED-FLDS3"), "Field Study 3 (Teaching Assistantship)", 3.0m, "Assisting mentor teachers in instructional delivery and assessment.", "Seminar Room"),
            new Subject(SubjectCode.From("ED-STUDTE"), "Student Teaching (Internship)", 6.0m, "Full-time supervised teaching experience in partner schools.", "Seminar Room"),
            new Subject(SubjectCode.From("ED-SEM"), "Seminar on Current Issues in Education", 3.0m, "Contemporary trends, issues, and innovations in the Philippine education system.", "Lecture Room"),

            // --- BECEd (Early Childhood Education) Core Subjects ---
            new Subject(SubjectCode.From("ECE-CHILD"), "Child Development (Prenatal to Age 8)", 3.0m, "Physical, cognitive, language, and socio-emotional development from conception to age eight.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-CURR"), "ECE Curriculum and Pedagogy", 3.0m, "Developmentally appropriate curriculum models and teaching approaches for young children.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-LANG"), "Language Development in Children", 3.0m, "Theories and stages of language acquisition and early literacy development.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-PLAY"), "Play-Based Learning", 3.0m, "The role of play in cognitive, social, and physical development with practical facilitation strategies.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-HEALTH"), "Health, Safety, and Nutrition", 3.0m, "Health promotion, safety practices, and nutritional requirements for young children.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-ART"), "Creative Arts and Expression", 3.0m, "Visual arts, music, drama, and movement as vehicles for creative expression in early childhood.", "Laboratory"),
            new Subject(SubjectCode.From("ECE-MATH"), "Mathematics for Young Children", 3.0m, "Number concepts, patterns, measurement, and spatial reasoning through hands-on activities.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-SCI"), "Science for Young Children", 3.0m, "Inquiry-based science exploration and discovery activities for early learners.", "Laboratory"),
            new Subject(SubjectCode.From("ECE-LIT"), "Children's Literature", 3.0m, "Age-appropriate literature and storytelling techniques for early childhood education.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-PARNT"), "Parent and Community Engagement", 3.0m, "Strategies for building partnerships with families and the community.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-SPED"), "Inclusive Education in ECE", 3.0m, "Adapting instruction and environments to include children with diverse learning needs.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-ASSMT"), "Assessment in Early Childhood", 3.0m, "Observation-based assessment, developmental screening, and documentation techniques.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-OBSRV"), "Observation and Documentation", 3.0m, "Systematic observation methods and portfolio documentation for young children.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-TEACH"), "Teaching Strategies in ECE", 3.0m, "Evidence-based instructional strategies tailored to early childhood settings.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-SOCST"), "Social Studies for Young Children", 3.0m, "Foundational concepts in social studies appropriate for early learners.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-MUSIC"), "Music and Movement", 3.0m, "Music-based activities and movement education to support holistic development.", "Laboratory"),
            new Subject(SubjectCode.From("ECE-LANG2"), "Language, Literacy, and Communication", 3.0m, "Emergent literacy skills including phonological awareness, print concepts, and oral language.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-ADMIN"), "ECE Program Administration", 3.0m, "Management of early childhood programs including licensing, staffing, and budgeting.", "Lecture Room"),
            new Subject(SubjectCode.From("ECE-FLDST"), "Field Study in ECE", 3.0m, "Supervised observation and participation in early childhood education settings.", "Seminar Room"),
            new Subject(SubjectCode.From("ECE-PRACT"), "ECE Practice Teaching", 6.0m, "Full-time supervised teaching in early childhood education environments.", "Seminar Room"),
            new Subject(SubjectCode.From("ECE-RES"), "Research in Early Childhood Education", 3.0m, "Research methods and evidence-based practices in early childhood education.", "Lecture Room"),

            // ===== COLLEGE OF ARTS AND SCIENCES (CAS) =====

            // --- ABELS (English Language Studies) Core Subjects ---
            new Subject(SubjectCode.From("ENG-LING1"), "Introduction to Linguistics", 3.0m, "Scientific study of language including phonetics, phonology, morphology, syntax, and semantics.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-LIT1"), "Survey of English Literature", 3.0m, "Major works and literary movements from Old English to the Romantic period.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-LIT2"), "Survey of American Literature", 3.0m, "Major works and literary movements from colonial America to contemporary period.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-GRAM"), "English Grammar and Structure", 3.0m, "Descriptive and prescriptive grammar of English including syntax and usage.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-TRANS"), "Translation Studies", 3.0m, "Theory and practice of translation between English and Filipino.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-DISC"), "Discourse Analysis", 3.0m, "Analysis of language use beyond the sentence level in written and spoken discourse.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-STYL"), "Stylistics", 3.0m, "Linguistic analysis of literary texts and style.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-COMP"), "Composition and Rhetoric", 3.0m, "Advanced writing skills and rhetorical strategies for academic and professional contexts.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-CREAT"), "Creative Writing", 3.0m, "Writing fiction, poetry, creative non-fiction, and drama.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-LANG"), "Language Acquisition", 3.0m, "Theories and processes of first and second language acquisition.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-PHON"), "Phonetics and Phonology", 3.0m, "Speech sounds, phonological processes, and the sound system of English.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-MORPH"), "Morphology", 3.0m, "Word structure, morphemes, and morphological processes in English.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-SYN"), "Syntax", 3.0m, "Sentence structure, phrase structure rules, and syntactic theory.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-SEM"), "Semantics", 3.0m, "Meaning in language including lexical semantics, compositionality, and pragmatics.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-PRAG"), "Pragmatics", 3.0m, "Language use in context including speech acts, implicature, and politeness.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-SOCIOL"), "Sociolinguistics", 3.0m, "Language variation, multilingualism, language policy, and social factors in language use.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-PSYCHOL"), "Psycholinguistics", 3.0m, "Psychological processes underlying language comprehension, production, and representation.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-LIT3"), "Philippine Literature in English", 3.0m, "Seminal works of Philippine literature written in English across historical periods.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-LIT4"), "Survey of World Literature", 3.0m, "Representative literary works from Asia, Europe, Africa, and the Americas in translation.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-LITCR"), "Literary Criticism", 3.0m, "Major schools of literary theory and critical approaches to textual analysis.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-TECHW"), "Technical Writing", 3.0m, "Writing technical reports, manuals, proposals, and documentation for professional audiences.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-ARGUM"), "Argumentation and Debate", 3.0m, "Logical argumentation, debate formats, persuasive speaking, and critical reasoning.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-JOURN"), "Journalism", 3.0m, "News writing, feature writing, editorial writing, and journalistic ethics.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-CONTL"), "Contemporary and Popular Literature", 3.0m, "Recent literary trends, popular fiction, and emerging voices in English literature.", "Lecture Room"),
            new Subject(SubjectCode.From("ENG-RES"), "Research Methods in English Studies", 3.0m, "Qualitative and quantitative research methodologies for language and literature research.", "Lecture Room"),

            // --- BSP (Psychology) Core Subjects ---
            new Subject(SubjectCode.From("PSY-GEN"), "General Psychology", 3.0m, "Overview of psychological science including history, theories, research methods, and major domains.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-BIO"), "Biological Psychology", 3.0m, "Neural and biological bases of behavior including brain structures, neurotransmitters, and genetics.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-DEV"), "Developmental Psychology", 3.0m, "Human development across the lifespan from conception to old age.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-SOC"), "Social Psychology", 3.0m, "Individual behavior in social contexts including attitudes, persuasion, group dynamics, and prejudice.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-COG"), "Cognitive Psychology", 3.0m, "Mental processes including perception, attention, memory, reasoning, and decision-making.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-ABN"), "Abnormal Psychology", 3.0m, "Classification, etiology, and treatment of psychological disorders.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-EXP"), "Experimental Psychology", 3.0m, "Experimental design, laboratory techniques, and data analysis in psychological research.", "Laboratory"),
            new Subject(SubjectCode.From("PSY-TEST"), "Psychological Testing", 3.0m, "Principles of test construction, psychometrics, and ethical use of psychological assessments.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-IND"), "Industrial/Organizational Psychology", 3.0m, "Psychological principles applied to workplace behavior including selection, training, and motivation.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-CLIN"), "Clinical Psychology", 3.0m, "Assessment, diagnosis, and therapeutic interventions for mental health disorders.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-FOREN"), "Forensic Psychology", 3.0m, "Application of psychological principles to legal and criminal justice systems.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-HEAL"), "Health Psychology", 3.0m, "Psychological factors in health, illness, and healthcare behavior.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-MOTIV"), "Motivation and Emotion", 3.0m, "Theories of motivation, emotional processes, and their interplay with behavior.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-PERS"), "Theories of Personality", 3.0m, "Major theoretical perspectives on personality structure, dynamics, and development.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-STAT"), "Statistics for Psychology", 3.0m, "Descriptive and inferential statistics used in psychological research.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-RES"), "Research Methods in Psychology", 3.0m, "Quantitative and qualitative research designs, data collection, and ethical considerations.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-FIELD"), "Field Experience in Psychology", 3.0m, "Supervised field placement in psychological service settings.", "Seminar Room"),
            new Subject(SubjectCode.From("PSY-ETH"), "Ethics in Psychology", 3.0m, "Professional ethics, codes of conduct, and ethical decision-making in psychological practice.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-GENDR"), "Psychology of Gender", 3.0m, "Gender development, gender roles, and psychological perspectives on gender identity.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-HUM"), "Human Resource Psychology", 3.0m, "Personnel selection, performance appraisal, training, and organizational development.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-MULTI"), "Multicultural Psychology", 3.0m, "Cultural influences on behavior and the impact of diversity on psychological processes.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-COUNS"), "Basic Counseling Skills", 3.0m, "Foundational listening, interviewing, and basic counseling techniques.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-SEM"), "Seminar in Psychology", 3.0m, "Contemporary issues, emerging trends, and developments in psychological science.", "Lecture Room"),
            new Subject(SubjectCode.From("PSY-CAP"), "Psychology Capstone", 3.0m, "Integration and application of psychological knowledge through a culminating project.", "Seminar Room"),

            // ===== COLLEGE OF NURSING (CN) =====

            // --- BSN (Nursing) Core Subjects ---
            new Subject(SubjectCode.From("NUR-INTRO"), "Introduction to Nursing", 3.0m, "Historical development, conceptual frameworks, roles, and scope of professional nursing practice.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-ANAT"), "Anatomy and Physiology", 5.0m, "Structure and function of the human body systems with laboratory dissection and study.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-BIOC"), "Biochemistry for Nursing", 3.0m, "Biochemical principles underlying physiological processes and disease states.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-MICRO"), "Microbiology and Parasitology", 3.0m, "Microorganisms and parasites of medical importance, infection control, and immunology basics.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-NUTR"), "Nutrition and Diet Therapy", 3.0m, "Nutritional science, therapeutic diets, and nutritional support across the lifespan.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-HEAL"), "Health Assessment", 3.0m, "Systematic physical examination and health history taking skills.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-FUND"), "Fundamentals of Nursing", 5.0m, "Basic nursing skills, patient care principles, and the nursing process.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-PHARM"), "Pharmacology", 3.0m, "Drug classifications, mechanisms of action, therapeutic uses, and nursing considerations.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-PHYS"), "Pathophysiology", 3.0m, "Functional changes in the body associated with disease processes.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-IMM"), "Immunology", 3.0m, "Immune system function, immune disorders, and immunotherapy.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-MCN"), "Maternal and Child Nursing", 3.0m, "Nursing care for women during pregnancy, childbirth, and postpartum, and newborn care.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-MED"), "Medical-Surgical Nursing", 5.0m, "Nursing care for adult patients with medical and surgical conditions.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-PED"), "Pediatric Nursing", 3.0m, "Nursing care for infants, children, and adolescents with common health conditions.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-PSYCH"), "Psychiatric Nursing", 3.0m, "Nursing care for patients with mental health disorders across psychiatric settings.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-COMM"), "Community Health Nursing", 3.0m, "Population-focused nursing care in community settings including public health programs.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-EPID"), "Epidemiology", 3.0m, "Distribution and determinants of health-related states in populations.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-EMER"), "Emergency and Disaster Nursing", 3.0m, "Emergency care, triage, disaster preparedness, and response in nursing practice.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-GERI"), "Gerontologic Nursing", 3.0m, "Nursing care for the elderly including age-related changes and common geriatric conditions.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-ONCO"), "Oncology Nursing", 3.0m, "Nursing care for cancer patients including treatment modalities and palliative care.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-INT"), "Intensive Care Nursing", 3.0m, "Critical care nursing principles, monitoring, and management of critically ill patients.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-MGMT"), "Nursing Management and Leadership", 3.0m, "Leadership theories, management principles, and quality improvement in nursing.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-RES"), "Nursing Research", 3.0m, "Research methodologies, evidence-based practice, and utilization of research findings.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-ETH"), "Nursing Ethics and Legal Responsibilities", 3.0m, "Ethical principles, legal doctrines, and professional accountability in nursing.", "Lecture Room"),
            new Subject(SubjectCode.From("NUR-RLE1"), "Related Learning Experience 1", 3.0m, "Simulation and skills laboratory practice for foundational nursing procedures.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-RLE2"), "Related Learning Experience 2", 3.0m, "Simulation and skills laboratory practice for maternal and pediatric nursing.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-RLE3"), "Related Learning Experience 3", 3.0m, "Simulation and skills laboratory practice for medical-surgical nursing.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-RLE4"), "Related Learning Experience 4", 3.0m, "Simulation and skills laboratory practice for psychiatric and community health nursing.", "Laboratory"),
            new Subject(SubjectCode.From("NUR-IMMER"), "Clinical Immersion", 6.0m, "Comprehensive supervised clinical practice in hospital and community settings.", "Laboratory"),

            // ===== COLLEGE OF ARCHITECTURE (CA) =====

            // --- BSARCH (Architecture) Core Subjects ---
            new Subject(SubjectCode.From("ARCH-DES1"), "Architectural Design 1", 4.0m, "Fundamentals of design, visual thinking, space organization, and basic design projects.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES2"), "Architectural Design 2", 4.0m, "Design of simple buildings with emphasis on site context and user needs.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES3"), "Architectural Design 3", 4.0m, "Design of moderate-scale buildings with focus on spatial programming and circulation.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES4"), "Architectural Design 4", 4.0m, "Design of multi-story buildings integrating structural, utility, and environmental systems.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES5"), "Architectural Design 5", 4.0m, "Complex building typologies with emphasis on urban context and sustainability.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES6"), "Architectural Design 6", 4.0m, "Comprehensive design of large-scale projects including site planning and master planning.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES7"), "Architectural Design 7", 4.0m, "Design of specialized facilities with advanced technical integration.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-DES8"), "Architectural Design 8", 4.0m, "Pre-thesis comprehensive design studio synthesizing all architectural knowledge.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-VIS1"), "Visual Techniques 1 (Freehand Drawing)", 3.0m, "Freehand sketching, perspective drawing, and architectural rendering techniques.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-VIS2"), "Visual Techniques 2 (Rendering)", 3.0m, "Advanced rendering media, color theory, and presentation graphics.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-VIS3"), "Visual Techniques 3 (Presentation)", 3.0m, "Digital and physical presentation techniques including model-making.", "Laboratory"),
            new Subject(SubjectCode.From("ARCH-HIST1"), "History of Architecture 1 (Ancient to Medieval)", 3.0m, "Architectural history from ancient civilizations through the medieval period.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-HIST2"), "History of Architecture 2 (Renaissance to Modern)", 3.0m, "Architectural history from the Renaissance through the modern movement.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-HIST3"), "History of Architecture 3 (Philippine Architecture)", 3.0m, "Development of Philippine architecture from pre-colonial to contemporary.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-THEO"), "Architectural Theory", 3.0m, "Theoretical frameworks, design philosophies, and critical discourse in architecture.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-STRUC1"), "Building Structures 1 (Statics)", 3.0m, "Principles of statics, equilibrium, and structural analysis applied to buildings.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-STRUC2"), "Building Structures 2 (Steel and Timber)", 3.0m, "Structural design of steel and timber building systems.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-STRUC3"), "Building Structures 3 (Reinforced Concrete)", 3.0m, "Structural design of reinforced concrete systems for buildings.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-MATL"), "Building Materials", 3.0m, "Properties, specifications, and appropriate application of building construction materials.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-UTIL1"), "Building Utilities 1 (Plumbing and Sanitary)", 3.0m, "Water supply, plumbing, drainage, and sanitary systems in buildings.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-UTIL2"), "Building Utilities 2 (Electrical and Mechanical)", 3.0m, "Electrical power distribution, lighting, HVAC, and mechanical systems in buildings.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-UTIL3"), "Building Utilities 3 (Acoustics and Lighting)", 3.0m, "Acoustical design and natural/artificial lighting principles for buildings.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-CAD1"), "Computer-Aided Design 1", 3.0m, "2D CAD drafting, documentation, and digital presentation techniques.", "Computer Lab"),
            new Subject(SubjectCode.From("ARCH-CAD2"), "Computer-Aided Design 2 (BIM)", 3.0m, "Building Information Modeling (BIM) software and 3D parametric modeling.", "Computer Lab"),
            new Subject(SubjectCode.From("ARCH-PLAN"), "Urban Planning and Design", 3.0m, "Principles of urban design, land use planning, zoning, and community development.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-SUST"), "Sustainable and Green Architecture", 3.0m, "Environmentally responsive design, passive strategies, and green building certification.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-CONS"), "Construction Management", 3.0m, "Project management, construction contracts, procurement, and project delivery methods.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-EST"), "Cost Estimating and Quantity Surveying", 3.0m, "Construction cost estimation, quantity take-offs, and budgeting.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-BLDGC"), "Building Codes and Professional Practice", 3.0m, "National Building Code, accessibility laws, and ethical professional practice.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-RES"), "Research Methods in Architecture", 3.0m, "Research methodologies applied to architectural design and planning problems.", "Lecture Room"),
            new Subject(SubjectCode.From("ARCH-THES"), "Architectural Thesis", 6.0m, "Independent comprehensive architectural design project demonstrating mastery.", "Laboratory"),
        };

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            """);

        Dictionary<string, int> roomTypes = [];

        scriptBuilder.Append("""
                MERGE [Subjects] AS [Target]
                USING ( VALUES
            """);

        var values = new List<string>();
        foreach (var subject in subjects)
        {
            if (!roomTypes.TryGetValue(subject.preferRoomTypeName, out int roomTypeId))
            {
                var getRoomTypeCommand = dbCommandFactory();
                getRoomTypeCommand.CommandText = $"SELECT Id FROM RoomTypes WHERE Name='{subject.preferRoomTypeName}'";
                roomTypeId = (int)getRoomTypeCommand.ExecuteScalar();
                roomTypes.Add(subject.preferRoomTypeName, roomTypeId);
            }

            var descriptionValue = subject.description != null ? $"'{subject.description.Replace("'", "''")}'" : "NULL";
            var unitsValue = subject.units.HasValue ? subject.units.Value.ToString("0.0") : "NULL";

            values.Add(string.Format("('{0}', '{1}', {2}, {3}, {4})",
                subject.code,
                subject.title.Replace("'", "''"),
                unitsValue,
                descriptionValue,
                roomTypeId));
        }

        scriptBuilder.Append(string.Join(',', values));

        scriptBuilder.Append("""
            ) AS [Source] ([Code], [Title], [Units], [Description], [PreferRoomTypeId])
            ON [Target].[Code] = [Source].[Code]
            WHEN NOT MATCHED THEN
                INSERT ([Code], [Title], [Units], [Description], [PreferRoomTypeId], [CreatedBy], [CreatedAt])
                VALUES ([Source].[Code], [Source].[Title], [Source].[Units], [Source].[Description], [Source].[PreferRoomTypeId], @InitialUserId, GETUTCDATE())
            WHEN MATCHED THEN
                UPDATE SET [Target].[Title] = [Source].[Title],
                           [Target].[Units] = [Source].[Units],
                           [Target].[Description] = [Source].[Description],
                           [Target].[PreferRoomTypeId] = [Source].[PreferRoomTypeId],
                           [Target].[UpdatedBy] = @InitialUserId,
                           [Target].[UpdatedAt] = GETUTCDATE();
            """);

        return scriptBuilder.ToString();
    }

    public record Subject(SubjectCode code, string title, decimal? units, string? description, string preferRoomTypeName);
}
