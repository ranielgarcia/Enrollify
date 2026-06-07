### 1. **College / Department**

- **Description:** Represents a major academic division (e.g., College of Engineering, College of Business).
    
- **Attributes:** `Id`, `Name`, `Code`, `Dean`, `Description`
    
- **Relationships:**
    - One **College** has many **Departments**
    - One **College** has many **Courses**
---

### 2. **Department**

- **Description:** A subdivision within a College focusing on a specific academic discipline (e.g., Department of Computer Science).
    
- **Attributes:** `Id`, `Name`, `Code`, `Chairperson`, `Description`
    
- **Relationships:**
    - Belongs to one **College**
    - Has many **Courses**
    - Has many **Faculty Members (Teachers)**
    - May have many **Subjects**

---

### 3. **Course (Program)**

- **Description:** Represents a degree program or academic track (e.g., BS Computer Science, BA Psychology).
    
- **Attributes:** `Id`, `Code`, `Name`, `DurationYears`, `Description`, `CollegeId`
    
- **Relationships:**
    
    - Belongs to one **Department** (or directly to a **College**)
    - Has many **Subjects** (core and elective)
    - Has many **Students** enrolled
    - Has many **Sections**

---

### 4. **Subject**

- **Description:** Represents a single academic unit (e.g., “Calculus 1”, “Programming 101”).
    
- **Attributes:** `Id`, `Code`, `Title`, `Description`, `Units`, `PrerequisiteSubjectIds`
    
- **Relationships:**
    
    - Belongs to one or more **Courses**
    - Taught by one or more **Teachers**
    - Has many **Class Schedules / Sections**
    - Can have **Equivalent Subjects** (used for cross-college enrollment)
---

### 5. **Teacher / Faculty**

- **Description:** Represents academic staff who teach subjects.
    
- **Attributes:** `Id`, `FirstName`, `LastName`, `Email`, `DepartmentId`, `EmploymentStatus`
    
- **Relationships:**
    
    - Belongs to a **Department
    - Teaches many **Subjects
    - Manages many **Sections
    - Linked to **Class Schedules**

---

### 6. **Student**

- **Description:** Represents a learner enrolled in the institution.
    
- **Attributes:** `Id`, `StudentNumber`, `FirstName`, `LastName`, `Email`, `CourseId`, `YearLevel`, `Status`
    
- **Relationships:**
    
    - Belongs to one **Course**
    - Enrolls in many **Subjects** (through `Enrollment` entity)
    - Has many **Enrollment Records**
    - Linked to **Payments**, **Grades**, and **Sections**

---

### 7. **Section / Class**

- **Description:** Represents a group of students assigned to the same subject offering (e.g., “BSCS-2A”, “ENG101-A”).
    
- **Attributes:** `Id`, `Name`, `CourseId`, `YearLevel`, `Semester`, `SchoolYear`
    
- **Relationships:**
    
    - Belongs to one **Course**
    - Has many **Students**
    - Has many **Subject Offerings (Schedules)**
    - Managed by one **Teacher (Adviser)**

---

### 8. **Subject Offering / Class Schedule**

- **Description:** Represents the specific schedule and teacher assignment for a subject.
    
- **Attributes:** `Id`, `SubjectId`, `SectionId`, `TeacherId`, `Day`, `TimeStart`, `TimeEnd`, `Room`
    
- **Relationships:**
    
    - Belongs to a **Subject**
    - Belongs to a **Section**
    - Assigned to a **Teacher**
    - Contains many **Enrollments**
        


---

### 9. **Enrollment**

- **Description:** Tracks a student’s registration for subjects in a given term/semester.
    
- **Attributes:** `Id`, `StudentId`, `SectionId`, `SubjectOfferingId`, `Semester`, `SchoolYear`, `Status` (Pending, Approved, Enrolled)
    
- **Relationships:**
    
    - Belongs to a **Student
    - References a **Subject Offering**
    - Has related **Payment** and **Grade** records
        

---

### 10. **Grade / Academic Record**

- **Description:** Represents the student’s performance in a specific subject.
    
- **Attributes:** `Id`, `EnrollmentId`, `MidtermGrade`, `FinalGrade`, `Remarks`
    
- **Relationships:**
    
    - Belongs to an **Enrollment**
    - Linked indirectly to a **Student**, **Subject**, and **Teacher

---

### 11. **Payment / Billing**

- **Description:** Represents financial transactions related to enrollment.
    
- **Attributes:** `Id`, `StudentId`, `Amount`, `PaymentDate`, `Status`, `Semester`, `SchoolYear`
    
- **Relationships:**
    
    - Belongs to a **Student**
    - Linked to **Enrollment** records
    - May have **Payment Details** (breakdown of fees)

---

### 12. **User / Role / Permission (Access Control)**

- **Description:** Handles system authentication and authorization.
    
- **Attributes:** `Id`, `Username`, `Email`, `RoleId`
    
- **Relationships:**
    
    - Each **User** has one **Role** (e.g., Student, Teacher, Admin, Admission Staff)
    - Roles define access to **Pages/Modules
    - Linked to domain entities via `PersonId` (StudentId or TeacherId)

---

### 13. **EquivalentSubject / SubjectMapping** (Optional but useful)

- **Description:** Used to map equivalent subjects across different colleges or curricula.
    
- **Attributes:** `Id`, `SourceSubjectId`, `EquivalentSubjectId`, `Reason`
    
- **Relationships:**
    
    - Links multiple **Subjects** across **Colleges** or **Departments**

---

### 14. **Curriculum**

- **Description:** Represents the official list of subjects required for a course in a specific academic year/version.
    
- **Attributes:** `Id`, `CourseId`, `CurriculumYear`, `EffectiveDate`, `Status`
    
- **Relationships:**
    
    - Belongs to a **Course**
    - Has many **CurriculumSubjects**
    - Referenced during **Enrollment** validation

---

### 15. **CurriculumSubject**

- **Description:** Links subjects to a specific curriculum and year level/semester.
    
- **Attributes:** `Id`, `CurriculumId`, `SubjectId`, `YearLevel`, `Semester`, `IsElective`
    
- **Relationships:**
    
    - Belongs to a **Curriculum**
    - References a **Subject**
        

## 🧩 High-Level Relationship Summary

| Entity                | Related Entities                                 |
| --------------------- | ------------------------------------------------ |
| **College**           | Departments, Courses                             |
| **Department**        | College, Courses, Teachers                       |
| **Course**            | Department, Subjects, Students, Sections         |
| **Subject**           | Courses, Teachers, Schedules, EquivalentSubjects |
| **Teacher**           | Department, Subjects, Schedules                  |
| **Student**           | Course, Enrollments, Payments, Grades            |
| **Section**           | Course, Students, SubjectOfferings               |
| **SubjectOffering**   | Subject, Section, Teacher, Enrollments           |
| **Enrollment**        | Student, SubjectOffering, Grade, Payment         |
| **Grade**             | Enrollment                                       |
| **Payment**           | Student, Enrollment                              |
| **Curriculum**        | Course, CurriculumSubjects                       |
| **EquivalentSubject** | Subjects (cross-link)                            |