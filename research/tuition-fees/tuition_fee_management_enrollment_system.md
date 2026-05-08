# Tuition Fee Management in an Enrollment System

In an enrollment system, tuition fee management is usually handled through a combination of:

- Academic structure
- Billing rules
- Student enrollment data
- Payment tracking
- Financial policies

A well-designed system separates the **fee configuration** from the **student-specific billing records**.

---

# Core Concepts

## 1. Fee Setup / Tuition Configuration

This defines how tuition is calculated.

Typical configurations include:

| Configuration | Example |
|---|---|
| Tuition per unit | ₱1,500 per unit |
| Flat tuition | ₱25,000 per semester |
| Laboratory fees | ₱3,000 |
| Miscellaneous fees | ₱5,500 |
| Course-based fees | Nursing has extra clinical fees |
| Year-level fees | Freshmen package |
| Semester fees | 1st sem vs summer |
| Campus-specific fees | Main campus vs satellite |
| Residency-based fees | Local vs international students |

---

# Common Database Design

## Fee Template Tables

```text
FeeStructure
- Id
- AcademicYearId
- TermId
- ProgramId
- YearLevel
- TuitionCalculationType
- AmountPerUnit
- EffectiveDate
- IsActive
```

```text
FeeItem
- Id
- FeeStructureId
- FeeType
- Description
- Amount
- IsOptional
```

---

# 2. Student Enrollment

When a student enrolls:

- Subjects are selected
- Units are calculated
- Applicable fee templates are resolved
- Billing records are generated

This is important:

> The system should NOT compute tuition dynamically every time the page loads.

Instead:

- Generate a snapshot billing record during enrollment
- Preserve historical accuracy

Because tuition policies change over time.

---

# 3. Assessment / Billing Generation

Once enrollment is finalized, the system creates:

## Student Ledger / Assessment

Example:

| Item | Amount |
|---|---|
| Tuition (21 units × ₱1,500) | ₱31,500 |
| Miscellaneous | ₱5,500 |
| Laboratory Fee | ₱3,000 |
| Library Fee | ₱1,000 |
| Discount | -₱2,000 |
| Total | ₱39,000 |

---

# Suggested Tables

## StudentAssessment

```text
StudentAssessment
- Id
- StudentId
- EnrollmentId
- AssessmentDate
- TotalAmount
- Status
```

## StudentAssessmentItem

```text
StudentAssessmentItem
- Id
- StudentAssessmentId
- FeeType
- Description
- Quantity
- UnitAmount
- TotalAmount
```

This is essentially the accounting snapshot.

---

# 4. Payment Management

The system then tracks:

- Partial payments
- Installments
- Official receipts
- Scholarships
- Discounts
- Refunds
- Penalties
- Outstanding balances

---

# Typical Payment Tables

## Payment

```text
Payment
- Id
- StudentId
- ORNumber
- PaymentDate
- AmountPaid
- PaymentMethod
```

## PaymentAllocation

```text
PaymentAllocation
- Id
- PaymentId
- StudentAssessmentId
- AmountApplied
```

This allows:

- One payment to cover multiple balances
- Flexible accounting

---

# 5. Installment Plans

Many schools support installment schemes.

Example:

| Due Date | Amount |
|---|---|
| Upon Enrollment | ₱10,000 |
| Midterms | ₱15,000 |
| Finals | ₱14,000 |

---

# Recommended Design

## PaymentSchedule

```text
PaymentSchedule
- Id
- StudentAssessmentId
- DueDate
- AmountDue
- Status
```

---

# 6. Discounts and Scholarships

Usually modeled separately.

Examples:

- Academic scholarship
- Athletic scholarship
- Employee dependent discount
- Senior citizen
- Early enrollment discount

---

# Better Design Pattern

Instead of hardcoding discounts:

```text
DiscountRule
- Id
- Name
- Type
- CalculationMethod
- Value
```

Then apply rules during assessment generation.

---

# 7. Important Real-World Considerations

## A. Tuition Snapshoting

Critical requirement.

If tuition changes next semester:

- Existing assessments must remain unchanged.

Never recompute historical tuition from current fee tables.

---

## B. Auditability

Enrollment systems are financial systems.

You need:

- Audit logs
- Payment history
- Voided receipts
- Adjustment records
- Reassessment tracking

---

## C. Reassessment

Students may:

- Add subjects
- Drop subjects
- Change schedules

This affects tuition.

Common approaches:

| Approach | Description |
|---|---|
| Mutable assessment | Update existing billing |
| Versioned assessment | Create new assessment version |

Versioned assessments are safer.

---

## D. Accounting Integration

Large schools integrate with:

- ERP
- Accounting systems
- SAP
- Oracle
- QuickBooks

So your enrollment system may become a subledger.

---

# 8. Recommended Architecture

For a modern system:

## Domain Areas

### Enrollment Domain
- Subjects
- Sections
- Units

### Finance Domain
- Fee rules
- Billing
- Payments
- Discounts

### Student Domain
- Scholarships
- Residency
- Program

---

# Good DDD Aggregate Candidates

## Enrollment Aggregate

```text
Enrollment
- Student
- Subjects
- Units
```

## Assessment Aggregate

```text
Assessment
- AssessmentItems
- Discounts
- Totals
```

## Payment Aggregate

```text
Payment
- Allocations
```

---

# Recommended Workflow

```text
1. Student selects subjects
2. System computes units
3. Fee engine resolves applicable fees
4. Assessment snapshot generated
5. Student pays
6. Ledger updated
7. Enrollment finalized
```

---

# Enterprise-Level Features

Advanced systems also support:

- Multiple currencies
- Tax/VAT
- Online payments
- Payment gateways
- Dynamic fee rules
- Late enrollment penalties
- Promissory notes
- Sponsor billing
- Government subsidy integration
- Student wallets
- Auto-cancellation for unpaid balances

---

# Recommended Technical Approach for Your Stack

Since you are using:
- ASP.NET Core
- DDD
- Clean Architecture
- React/TanStack

I would recommend:

## Backend
- Domain-driven fee engine
- Assessment snapshot entities
- MediatR commands for enrollment
- Event-driven recalculation

## Database
- Immutable assessment records
- Soft deletes avoided for finance
- Ledger-style transactions

## Frontend
- Real-time assessment preview
- Draft enrollment before finalization
- Optimistic UI for subject changes

---

# Important Design Advice

Do NOT tightly couple:

```text
Enrollment = Payment
```

A student may:
- enroll but unpaid
- partially paid
- sponsored
- reassessed later

Keep finance independent from enrollment lifecycle.

---

# Recommended High-Level Model

```text
Student
  -> Enrollment
      -> EnrollmentSubjects

Enrollment
  -> Assessment
      -> AssessmentItems
      -> PaymentSchedules

Assessment
  -> Payments
  -> Discounts
  -> Adjustments
```

This structure scales much better for real-world academic institutions.
