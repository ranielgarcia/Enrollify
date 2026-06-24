# EventStorming Sticky Notes Reference

Based on the EventStorming methodology popularized by Alberto Brandolini.

## Core Sticky Notes

| Color             | Element           | Purpose                                                                                                         | Example                                    |
| ----------------- | ----------------- | --------------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| 🟧 Orange         | Domain Event      | Something that happened in the business domain. Written in past tense. These are the backbone of EventStorming. | Student Enrolled, Payment Received         |
| 🟦 Blue           | Command           | An intention to perform an action. Usually triggers an event. Written in imperative form.                       | Enroll Student, Process Payment            |
| 🟨 Yellow         | Actor / User      | Person or role that issues a command.                                                                           | Registrar, Student, Cashier                |
| 🟨 Large Yellow   | Aggregate         | Domain object that receives commands and produces events while enforcing business rules.                        | Enrollment, Student Account                |
| 🟪 Purple / Lilac | Policy            | Business rule or automated reaction. Often expressed as "When X happens, do Y."                                 | When Payment Received → Confirm Enrollment |
| 🟩 Green          | Read Model / View | Information needed to make decisions or views shown to users.                                                   | Enrollment Summary, Available Sections     |
| 🩷 Pink           | External System   | System outside your domain boundary.                                                                            | Payment Gateway, SMS Provider, LMS         |
| 🟥 Red            | Hot Spot          | Questions, assumptions, conflicts, risks, or areas requiring investigation.                                     | What if section is already full?           |

## Typical EventStorming Flow

```text
Actor
  ↓
Command
  ↓
Aggregate
  ↓
Domain Event
  ↓
Policy
  ↓
Command
  ↓
Domain Event
```

## Enrollment System Example

```text
[Registrar]
      ↓
Enroll Student
      ↓
Enrollment Aggregate
      ↓
Student Enrolled
      ↓
Policy: Reserve Slot
      ↓
Reserve Class Slot
      ↓
Class Slot Reserved
```

## Additional Notes Often Used

| Color | Meaning |
|---------|---------|
| 🟩 Light Green | Opportunity / Improvement Idea |
| ⚫ Black Marker Circle | Bounded Context |
| 🔶 Cluster of Orange Events | Business Process |
| 🟥 Red Dot or Sticker | Important pain point or risk |

## Recommended Set for an Enrollment and Scheduling System

For a complex college enrollment and scheduling domain, the following sticky notes are usually sufficient:

- Orange: Domain Events
- Blue: Commands
- Yellow: Actors
- Large Yellow: Aggregates
- Purple: Policies
- Red: Hot Spots

These help uncover:

- Scheduling conflicts
- Teacher double-booking constraints
- Room allocation rules
- Section capacity limits
- Enrollment eligibility requirements
- Automated business processes
- Domain boundaries and responsibilities

## Notes

Color conventions can vary slightly between facilitators and organizations, but the concepts remain the same. The most important aspect of EventStorming is maintaining a shared understanding of the domain through collaborative modeling.
