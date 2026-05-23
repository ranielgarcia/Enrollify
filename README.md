# Enrollify — College Enrollment Management System

> [!WARNING]
> **🚧 This project is currently under active development and is not yet complete.**
> Features, APIs, and documentation are subject to change. Some modules are still being implemented and may not be fully functional.

**Enrollify** is a full-stack, web-based enrollment management system designed for colleges and universities. It digitizes and streamlines the entire academic lifecycle — from setting up master data and building class schedules, to managing student enrollments, computing fees, recording grades, and generating official documents.

---

## Table of Contents

- [System Summary](#system-summary)
- [System Architecture](#system-architecture)
- [Domain-Driven Design & Clean Architecture](#domain-driven-design--clean-architecture)
- [Tech Stack & Libraries](#tech-stack--libraries)
- [System Goals](#system-goals)
- [Author](#author)

---

## System Summary

Enrollify replaces fragmented, manual enrollment processes with a unified role-based web system. It supports the complete academic operations cycle across multiple user roles and student types.

### User Roles

| Role | Responsibilities |
|---|---|
| **Registrar** | Academic calendar, enrollment approvals, scheduling, records, overrides |
| **Cashier / Finance Officer** | Fee assessment, payment processing, receipts, clearance |
| **Admissions Officer** | New student applications, classification, enrollment initiation |
| **Dean / Department Head** | Approve course offerings, teaching loads, capacity overrides |
| **Academic Advisor** | Study plan review, curriculum compliance, prerequisite waivers |
| **Teacher / Faculty** | Class rosters, grade encoding, attendance |
| **Student** | Self-enrollment, schedule/grade/payment view, document requests |
| **Parent / Guardian** | Read-only view of grades, attendance, and billing |
| **IT Administrator** | User/role management, system configuration, maintenance |

### Modules & Functional Coverage

#### Master Data
- **Buildings & Rooms** — room type catalog, physical room management (capacity, type, building)
- **Colleges & Departments** — academic organizational hierarchy
- **Courses (Programs)** — degree programs (e.g., BS Computer Science), linked to departments and colleges
- **Subjects** — academic units with units, codes, and discipline assignments
- **Equivalent Subject Mappings** — cross-college or cross-curriculum subject equivalences (used for transferees)

#### Curriculum Management
- Curriculum versioning per course and academic year
- Subject prerequisites (hard and advisory) with prerequisite chain validation
- Curriculum-based subject grouping by year level and semester

#### Academic Calendar
- Academic year and semester/term management
- Enrollment period windows per semester

#### Class Scheduling
- Class section management (e.g., BSCS-2A) linked to courses and year levels
- Subject offerings per section with teacher and room assignment
- Class schedule management with **automatic conflict detection** (room and teacher double-booking)
- Bulk class section initialization

#### Enrollment
Enrollify supports three student enrollment workflows:

- **Regular Students** — select a class section and automatically enroll in all section offerings
- **Irregular Students** — manually add individual subject offerings across year levels or sections
- **Transferee Students** — credit evaluation using equivalent subject mappings; enroll in remaining required subjects

Enrollment features include:
- Prerequisite completion checks (with registrar/advisor override)
- Capacity enforcement (with Dean/Registrar override)
- Outstanding balance validation with assessment clearance flags
- Enrollment status lifecycle: `Pending → Approved → Enrolled`
- Subject Opening Request workflow for under-enrolled subjects
- Certificate of Registration (CoR) generation per term

#### Finance
- Assessment & fee calculation engine (tuition + miscellaneous fees)
- Tuition fee package management per course/year level
- Miscellaneous fee assignment rules (per subject, per course, per semester)
- Payment processing with partial payment support
- Balance tracking and payment history
- Cashier portal with receipt generation (PDF)

#### Academic Records
- Grade encoding portal for teachers (midterm + final grades)
- Registrar-level grade management and override
- Academic standing and GPA computation
- Transcript of Records (ToR) and CoR generation as PDF documents

#### Portals
- **Student Portal** — view enrolled subjects, class schedules, grades, and payment status; request documents
- **Parent/Guardian Portal** — read-only view of academic and financial status

#### Platform & Security
- Role-Based Access Control (RBAC) with fine-grained permission flags per role
- Azure Active Directory (Entra ID) authentication via MSAL
- System audit logs for all critical operations
- File upload with virus scan support

### Glossary

| Term | Definition |
|---|---|
| **Regular Student** | Enrolls by section; subjects are auto-selected from section offerings |
| **Irregular Student** | Enrolls per subject offering manually; may cross year levels or colleges |
| **Transferee** | Student transferring from another institution; uses Equivalent Subject mappings for credit evaluation |
| **Offering** | A scheduled instance of a subject tied to a class section, teacher, and room |
| **CoR** | Certificate of Registration — official list of enrolled subjects and schedule per term |
| **Assessment** | Computed charges (tuition + miscellaneous fees) for a given term |
| **Clearance** | Finance flag that allows a student to enroll despite an outstanding balance |

---

## System Architecture

Enrollify is structured as a **single-page application (SPA)** frontend backed by a **REST API** on .NET 10, with Azure-hosted infrastructure.

```
┌─────────────────────────────────────────────────────────┐
│                    React 19 SPA (Frontend)               │
│              TanStack Router · TanStack Query            │
│               shadcn/ui · Tailwind CSS v4                │
└────────────────────────┬────────────────────────────────┘
                         │ HTTPS (REST / JSON)
┌────────────────────────▼────────────────────────────────┐
│             .NET 10 Web API (Backend)                    │
│         FastEndpoints · Mediator (CQRS) · EF Core        │
└──────┬────────────────────────┬────────────────────────-┘
       │                        │
┌──────▼───────┐       ┌────────▼──────────┐
│  Azure SQL   │       │  Azure Blob        │
│  (SQL Server)│       │  Storage           │
└──────────────┘       └───────────────────┘

Authentication: Azure Active Directory (Entra ID / MSAL)
CI/CD: GitHub Actions → Azure DevOps → Azure App Service
```

### Mono-repo Layout

```
Enrollment-System/
├── src/
│   ├── system/
│   │   ├── EnrollifyBackend/          # .NET 10 Web API (7 projects)
│   │   └── enrollify-frontend/        # React 19 TypeScript SPA
│   └── POCs/
│       ├── Enrollify.CleanArchPOC/    # Clean Architecture reference POC
│       └── Genetic-algorithm/         # GA-based class scheduling solver (POC)
├── docs/
│   ├── user-stories/                  # US-001 → US-031 feature specs
│   ├── requirements/                  # Development phases & domain models
│   ├── sql-notes/                     # DB design notes and validation rules
│   └── Notes/                         # Research and algorithm notes
├── plans/                             # Design & implementation plans
└── research/                          # Technical research documents
```

---

## Domain-Driven Design & Clean Architecture

The backend is built following **Domain-Driven Design (DDD)** and **Clean Architecture** using [Ardalis patterns](https://github.com/ardalis/CleanArchitecture). The design enforces strict separation of concerns through layered projects — domain logic never depends on infrastructure.

### Architectural Layers

| Layer | Project | Responsibilities |
|---|---|---|
| **Domain** | `Enrollify.Core` | Aggregates, value objects, domain events, domain services, interfaces |
| **Application** | `Enrollify.Application` | CQRS use cases (commands/queries), DTOs, specifications, filtering helpers |
| **Infrastructure** | `Enrollify.Infrastructure` | EF Core `DbContext`, Dapper read queries, repositories, Azure Blob integration |
| **Presentation** | `Enrollify.WebAPI` | FastEndpoints HTTP endpoints, authorization policies, request/response models |
| **Shared** | `Enrollify.SharedKernel` | `EntityBase`, `ValueObject`, `IAggregateRoot`, repository interfaces |
| **Migrations** | `Enrollify.DatabaseMigration` | DbUp SQL migration runner |
| **Tests** | `Enrollify.UnitTests` / `Enrollify.IntegrationTests` | xUnit unit tests; Testcontainers-based integration tests |

### Sample File Structure

```
EnrollifyBackend/
├── Enrollify.Core/
│   ├── Aggregates/
│   │   ├── SubjectAggregate/
│   │   │   ├── Subject.cs             # Aggregate root
│   │   │   ├── SubjectId.cs           # Vogen strongly-typed ID
│   │   │   └── SubjectCode.cs         # Vogen value object
│   │   ├── CollegeAggregate/
│   │   ├── CourseAggregate/
│   │   ├── CurriculumAggregate/
│   │   ├── ClassSectionAggregate/
│   │   └── ...                        # 15 aggregates total
│   ├── ValueObjects/
│   ├── DomainExceptions/
│   └── IAuditable.cs
│
├── Enrollify.Application/
│   ├── Features/
│   │   ├── Subjects/
│   │   │   ├── CreateSubject.cs       # Command + Handler (inner class)
│   │   │   ├── GetSubjects.cs         # Query + Handler + Spec
│   │   │   └── SubjectDTO.cs
│   │   ├── Colleges/
│   │   ├── Courses/
│   │   └── ...
│   ├── Filtering/
│   │   ├── FilterExpressionBuilder.cs
│   │   └── SpecSortBuilder.cs
│   └── PagedResult.cs
│
├── Enrollify.Infrastructure/
│   ├── Data/
│   │   └── EnrollifyDbContext.cs
│   ├── Repositories/
│   └── Migrations/
│
├── Enrollify.WebAPI/
│   ├── Features/
│   │   ├── Subjects/
│   │   │   ├── CreateSubjectEndpoint.cs
│   │   │   ├── GetSubjectsEndpoint.cs
│   │   │   └── SubjectsGroup.cs
│   │   └── ...
│   ├── Authorization/
│   └── Program.cs
│
├── Enrollify.SharedKernel/
│   ├── EntityBase.cs
│   ├── IAggregateRoot.cs
│   └── IRepository.cs
│
└── Enrollify.DatabaseMigration/
    └── Scripts/
        └── *.sql
```

### Key Patterns

- **CQRS via [Mediator](https://github.com/martinothamar/Mediator):** Every use case is a `ICommand<T>` / `IQuery<T>` record with an inner `Handler` class.
- **Endpoints via [FastEndpoints](https://fast-endpoints.com/):** Each endpoint inherits `Endpoint<TRequest, TResponse>`; co-located `Validator<TRequest>` for input validation.
- **Result pattern via [Ardalis.Result](https://github.com/ardalis/Result):** Handlers return `Result<T>` or `Result`; the API layer maps them to HTTP responses.
- **Strongly-typed IDs via [Vogen](https://github.com/SteveDunn/Vogen):** Entity IDs (e.g., `SubjectId`, `SubjectCode`) are source-generated structs — no accidental primitive obsession.
- **Specifications via [Ardalis.Specification](https://github.com/ardalis/Specification):** Paginated, filtered, and sorted queries are expressed as composable spec classes.

---

## Tech Stack & Libraries

### Backend

| Category | Technology |
|---|---|
| **Runtime** | .NET 10 (ASP.NET Core) |
| **API Framework** | [FastEndpoints](https://fast-endpoints.com/) |
| **CQRS / Mediator** | [Mediator](https://github.com/martinothamar/Mediator) (source-generated) |
| **ORM** | Entity Framework Core (SQL Server provider) |
| **Read Queries** | Dapper |
| **DB Migrations** | DbUp |
| **Result Pattern** | [Ardalis.Result](https://github.com/ardalis/Result) |
| **Specifications** | [Ardalis.Specification](https://github.com/ardalis/Specification) |
| **Value Objects / IDs** | [Vogen](https://github.com/SteveDunn/Vogen) (source-generated) |
| **Auth** | Microsoft.Identity.Web (Azure AD / Entra ID) |
| **Testing** | xUnit, [Testcontainers](https://dotnet.testcontainers.org/) (MsSQL + Azurite), [Bogus](https://github.com/bchavez/Bogus) |

### Frontend

| Category | Technology |
|---|---|
| **Framework** | React 19 + TypeScript |
| **Build Tool** | Vite |
| **Routing** | [TanStack Router](https://tanstack.com/router) (file-based) |
| **Data Fetching / Caching** | [TanStack Query](https://tanstack.com/query) (`useSuspenseQuery`) |
| **Forms** | [TanStack Form](https://tanstack.com/form) + [Zod](https://zod.dev/) |
| **UI Components** | [shadcn/ui](https://ui.shadcn.com/) (Radix UI primitives) |
| **Styling** | [Tailwind CSS v4](https://tailwindcss.com/) |
| **Icons** | [Lucide React](https://lucide.dev/) (centralized via `module-icons.ts`) |
| **Authentication** | [@azure/msal-react](https://github.com/AzureAD/microsoft-authentication-library-for-js) |
| **URL State** | [nuqs](https://nuqs.47ng.com/) (URL-synced filter/sort/pagination) |
| **API Types** | [openapi-typescript](https://openapi-ts.dev/) (generated from backend OpenAPI schema) |

---

## System Goals

1. **Digitize end-to-end enrollment** — eliminate paper-based and spreadsheet-driven processes with a reliable web system accessible to all stakeholders.

2. **Support all student types** — handle the distinct workflows for regular, irregular, and transferee students without compromising data integrity.

3. **Enforce academic rules automatically** — prerequisite validation, schedule conflict detection, and enrollment capacity limits are enforced by the system, with controlled override capabilities for authorized roles.

4. **Enable accurate financial management** — compute and track tuition fees, miscellaneous charges, and payments with full audit trails and clearance workflows.

5. **Provide role-appropriate portals** — each user type (student, parent, faculty, admin) sees only the information and actions relevant to their role.

6. **Maintain academic records reliably** — grade encoding, GPA computation, and document generation (CoR, ToR) are handled consistently and are always traceable back to enrollment records.

7. **Scale with the institution** — built on Azure infrastructure with automated CI/CD, the system is designed to grow without manual deployment overhead.

---

## Author

**Raniel Garcia**  
Senior Software Engineer · [Accenture Philippines (ATCP)](https://www.accenture.com/ph-en)  
GitHub: [@ranielgarcia](https://github.com/ranielgarcia)
