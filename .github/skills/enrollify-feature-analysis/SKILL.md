---
name: enrollify-feature-analysis
description: >
  Comprehensive skill for analyzing research documents, technical specifications, or existing markdown files that describe features and their implementation status.
  Automatically categorizes features into IMPLEMENTED, PARTIALLY_IMPLEMENTED, and PENDING folders, creates detailed documentation for each feature,
  generates a summary README, and produces an INDEX file for quick reference.
  Reusable for any domain (data-locking, API security, performance optimization, etc.).
  Use when you have a research document or specification that needs to be broken down into organized, actionable feature documentation.
---

# Enrollify Feature Analysis & Documentation Skill

This skill guides you through analyzing technical research documents and transforming them into organized, categorized feature documentation suitable for developers, architects, and project managers.

**When to use this skill:**
- You have a research document describing features (some implemented, some pending)
- You need to break down a specification into separate, focused markdown files
- You want to organize implementation status across a feature set
- You're documenting architectural decisions or technical capabilities

---

## What This Skill Produces

### Output Structure
```
<output-folder>/
├── done/                          # Fully implemented features
│   ├── 01-feature-name.md
│   ├── 02-feature-name.md
│   └── ... (N features)
├── pending/                       # Not implemented or partial
│   ├── 01-feature-name.md
│   ├── 02-feature-name.md
│   └── ... (M features)
├── README.md                      # Comprehensive architecture guide (600-1000 lines)
└── INDEX.md                       # Quick reference table (300-500 lines)
```

### Each Feature File Includes
- **Clear status** (✅ IMPLEMENTED / ⚠️ PARTIAL / ❌ PENDING)
- **File locations** with specific line numbers from backend codebase
- **Guard rules & constraints** in easy-to-scan tables
- **Real-world scenarios** demonstrating the feature
- **Related features** and dependencies
- **Testing approach** (unit + integration test strategies)
- **Implementation steps** (for pending/partial features with full code snippets)

### README.md Includes
- **Core concepts** (problem statement, solutions, patterns)
- **Implementation status by feature** with detailed summaries
- **Usage guide** for different roles (developers, QA, admins, architects)
- **Key architectural patterns** (state machines, snapshots, guards)
- **Backend file organization** reference
- **Common Q&A** section
- **Next steps & roadmap**

### INDEX.md Includes
- **Quick feature table** with status, lines, priority, effort
- **Feature categorization** (done/partial/pending)
- **Key concepts** summary
- **Implementation completeness** percentage
- **Pro tips** for reviewers and testers

---

## Phase 0 — Pre-Flight: Input Analysis

Before starting the analysis, understand your input document.

### Step 0.1: Identify Document Type

**Is your input:**
1. **Research report** — Long-form analysis with confidence assessments, scenarios, citations
2. **Technical specification** — Formal requirements with must/should/may haves
3. **Status document** — List of features with explicit implementation status
4. **Meeting notes or design doc** — Informal descriptions of planned features

**Why it matters:** Affects how you extract feature information and implementation status.

### Step 0.2: Scan for Key Sections

Typical research documents include:

- **Executive summary** → Concise overview of status
- **Current state / What's implemented** → Look for ✅ checkmarks, "already exists"
- **Gaps / What's missing** → Look for ❌, "not yet implemented", "TODO"
- **Implementation guidance** → Look for code examples, design patterns
- **Confidence assessment** → Verification of implementation claims
- **Citations / evidence** → File paths, line numbers proving implementation

### Step 0.3: Extract Feature List

Create a preliminary list of all features mentioned:

```
From research document:

FEATURES FOUND:
1. Curriculum Status Enum — "verified from source"
2. ClassSection State Machine — "verified from source"
3. UpdateCourse Guards — "critical gap"
4. PhaseOut/Archive Commands — "unreachable states"
...
```

**Note the status claim** (verified, implemented, gap, missing, partial).

---

## Phase 1 — Feature Categorization

Classify each feature into one of three categories.

### Step 1.1: Define Implementation Status

**✅ IMPLEMENTED (Fully)**
- Feature is complete and working in the codebase
- Guards/rules are enforced
- Tests exist and pass
- No gaps or TODOs remain
- **Where to find:** Specific file paths and line numbers verified in source code

**⚠️ PARTIALLY IMPLEMENTED**
- Feature exists but is incomplete
- Missing guards or enforcement
- Partial tests or coverage gaps
- Contains TODO comments or acknowledged limitations
- **Example:** UpdateCourse exists but no guards implemented

**❌ PENDING (Not Implemented)**
- Feature is designed but no code exists
- Specification or requirements written but not coded
- Statuses/states defined but unreachable (no commands)
- No tests
- **Example:** PhaseOut/Archive commands designed but not coded

### Step 1.2: Verify Implementation Status

For each feature, answer:

1. **Does code exist?** File path + line numbers
2. **Are guards enforced?** Look for `Guard.Against...`, `Result.Forbidden`, validation rules
3. **Are tests present?** Unit tests and/or integration tests
4. **Are there TODOs or gaps?** Comments in code indicating incomplete work

**Research document → Code verification:**

```
Research claim: "UpdateSubject has no guard for active ClassSectionSubjectOfferings"
Code check: Find UpdateSubject.cs
           Line 64-76: No subject guard check ← CLAIM VERIFIED
           Decision: ✅ CONFIRMED as gap (missing guard)
```

### Step 1.3: Collect Evidence

For each feature, gather:

- **File path** — e.g., `Enrollify.Application/Features/Subjects/Commands/UpdateSubject.cs`
- **Line numbers** — e.g., lines 64-76
- **Guard rules** — Table of conditions and results
- **Related tests** — e.g., `Enrollify.UnitTests/Features/Subjects/Commands/UpdateSubjectTests.cs`
- **Implementation percentage** — How complete is this feature? (0-100%)

---

## Phase 2 — Feature Documentation

For each feature, write a detailed markdown file.

### Step 2.1: Choose Output Folder

- **✅ Implemented** → `done/01-feature-name.md`
- **⚠️ Partial** → `pending/01-feature-name.md`
- **❌ Pending** → `pending/02-feature-name.md`

**Naming convention:** Use numbers for sorting (01, 02, 03...) so folders show features in logical order.

### Step 2.2: Structure of Done/ Feature File

**Template for fully implemented features:**

```markdown
# Feature Name

## Overview
1-2 sentence description of what the feature does.

## Status
✅ FULLY IMPLEMENTED

## Current Implementation
- File: Path with line numbers
- Implementation details
- How it works

## Guard Rules
| Operation | Guard Condition | Result |
|-----------|-----------------|--------|
| Update | Condition X | Forbidden |

## Related Features
- Link to other features
- Dependencies

## Testing
- Where tests are located
- Test strategies

## Notes
- Key implementation details
- Why it's designed this way
```

**Content Guidelines:**
- Use file paths: `Enrollify.Core/Constants/CurriculumStatusEnum.cs` (lines 1-13)
- Include code snippets showing key logic
- Create guard rule tables (3-5 columns)
- Real-world scenarios (2-3 examples)
- State machine diagrams (if applicable)
- 150-300 words per feature (longer for complex features)

### Step 2.3: Structure of Pending/ Feature File

**Template for partial/not-implemented features:**

```markdown
# Feature Name - STATUS (PARTIAL / NOT IMPLEMENTED)

## Overview
Description of feature.

## Status
⚠️ PARTIALLY IMPLEMENTED (or ❌ NOT IMPLEMENTED)

## Current State
What exists: ✅ code, ❌ missing
What's missing: [list of gaps]

## Missing Guard/Specification Needed
If partial: What needs to be added?

## Proposed Implementation
- Full code snippets for missing pieces
- Step-by-step implementation plan
- Files to create/modify
- Testing approach

## Priority & Effort
- Priority: Low/Medium/High
- Estimated effort: 2h, 4h, 1 day, etc.

## Related Features
- Dependencies
```

**Content Guidelines:**
- Be specific: "UpdateCourse.cs lines 30-52 has no guard"
- Provide implementation code snippets (copy-paste ready)
- Implementation steps should be actionable (10-15 steps)
- Include test templates
- Effort estimates help prioritization
- 250-450 words per feature

---

## Phase 3 — Create README.md

This is the comprehensive architecture guide (600-1000 lines).

### Section 3.1: Overview & Quick Status

```markdown
## Overview
[Describe the domain/feature set being analyzed]
This folder contains documentation of [what].

**Source:** Based on [research document path]

## Quick Status Summary
| Feature | Status | Impact | Priority |
|---------|--------|--------|----------|
| ... | ✅ DONE | High | - |
| ... | ⚠️ PARTIAL | Medium | Medium |
| ... | ❌ PENDING | Low | Low |
```

### Section 3.2: Core Concepts

Explain the **why** behind the features:

- What problem do they solve?
- What patterns are used? (state machines, snapshots, guards)
- How do they work together?
- Reference diagrams (ASCII or mermaid)

**Example:** Explain the "stale data problem" and how snapshots solve it.

### Section 3.3: Implementation Status Breakdown

For each feature:
- 1 paragraph summary
- Where it's implemented
- What makes it work (key mechanisms)
- Impact on system
- Link to detailed doc

### Section 3.4: Usage Guide

Different sections for different roles:

**For Developers:**
- Code patterns to follow
- Where to add guards/snapshots
- How to write tests

**For API Designers:**
- Endpoint patterns
- Status codes for guard violations
- Error message guidelines

**For QA/Testers:**
- What to test in each feature
- Testing strategies
- Edge cases

**For System Admins:**
- Rules to remember
- Common operations
- Gotchas

### Section 3.5: Key Files Reference

Map features to actual file paths:

```markdown
## Key Files in Backend

### Core Domain
Enrollify.Core/
├── Constants/
│   ├── CurriculumStatusEnum.cs
│   └── ClassSectionStatusEnum.cs
├── Aggregates/
│   ├── CurriculumAggregate/
│   └── ClassSectionAggregate/
```

### Section 3.6: Related Documentation

Link to:
- Original research document
- Other architecture guides
- Testing documentation
- AGENTS.md

### Section 3.7: Common Q&A

Answer 3-5 frequently asked questions:

```markdown
### Q: Why do we need both snapshots AND guards?
**A:** Guards prevent future mutations...
```

### Section 3.8: Next Steps / Roadmap

```markdown
## Next Steps

### Priority 1 (Should implement soon)
- [ ] Feature X — 2h effort

### Priority 2 (Can implement later)
- [ ] Feature Y — 4h effort

### Priority 3 (Future enhancements)
- [ ] Feature Z
```

---

## Phase 4 — Create INDEX.md

Quick reference table and implementation summary (300-500 lines).

### Section 4.1: Overview Table

```markdown
## Overview

| Feature | Status | Lines | Priority | Effort |
|---------|--------|-------|----------|--------|
| Feature 1 | ✅ | 75 | - | - |
| Feature 2 | ⚠️ | 251 | Medium | 2h |
| Feature 3 | ❌ | 432 | Low | 4-6h |
| **TOTAL** | **92%** | **2,016** | | **6-8h remaining** |
```

### Section 4.2: File Organization Tree

Show the complete folder structure with brief descriptions.

### Section 4.3: Key Concepts Summary

1-2 sentence summaries of main concepts:
- The stale data problem
- The four lock points
- Three enforcement mechanisms

### Section 4.4: Feature Checklist

```markdown
## Implementation Checklist

- [x] Feature 1 ✅
- [x] Feature 2 ✅
- [ ] Feature 3 ⚠️ — Missing guard
- [ ] Feature 4 ❌ — Not started
```

### Section 4.5: Next Steps

Brief action items with effort estimates.

---

## Phase 5 — Final Assembly

### Step 5.1: Organize Files

```
output-folder/
├── done/
│   ├── 01-*.md
│   ├── 02-*.md
│   └── ...
├── pending/
│   ├── 01-*.md
│   └── ...
├── README.md
└── INDEX.md
```

### Step 5.2: Cross-Reference Links

Update README.md and INDEX.md to link to feature files:

```markdown
[Curriculum Status Enum & Active Lock](done/01-curriculum-status-enum.md)
```

Update each feature file to link back to README.md:

```markdown
For more context, see [README.md](../README.md)
```

### Step 5.3: Verify Quality

Checklist:
- [ ] All features from research documented
- [ ] Status (done/pending) correctly categorized
- [ ] File paths and line numbers verified in actual code
- [ ] Guard rules documented as tables
- [ ] Real-world scenarios included
- [ ] Implementation steps for pending features include code snippets
- [ ] README covers all core concepts
- [ ] INDEX quick-reference is accurate
- [ ] Cross-links work correctly
- [ ] Tone is consistent (professional, non-emotional, direct)

---

## Worked Example

Suppose research document has:

```
## Features

### ✅ Implemented: Curriculum Status Enum
Location: Enrollify.Core/Constants/CurriculumStatusEnum.cs
Guard: UpdateCurriculum blocked when Active (line 41)

### ❌ Missing: UpdateCourse Guards
The UpdateCourse command (line 30-52) has no guard for open sections.
Should prevent code changes when sections are Open/Locked/Active.
```

### Your Analysis:

1. **Extract features:** Curriculum Status Enum, UpdateCourse Guards
2. **Categorize:**
   - Curriculum Status Enum → ✅ IMPLEMENTED
   - UpdateCourse Guards → ❌ PENDING
3. **Verify in code:**
   - Check file `Enrollify.Application/Features/Courses/Commands/UpdateCourse.cs`
   - Line 30-52: No guards found
   - Conclusion: Status verified ✓
4. **Create files:**
   - `done/01-curriculum-status-enum.md` (150-200 words)
   - `pending/01-update-course-guards.md` (250-350 words with implementation code)
5. **Update README.md:**
   - Summarize both features in implementation status section
   - Link to feature files
6. **Update INDEX.md:**
   - Add both features to table
   - Mark percentages (1 done, 1 pending = 50%)

---

## Best Practices

### 1. Code References
Always cite specific file paths and line numbers from actual source:
```
❌ BAD:  "The UpdateCurriculum handler has a guard"
✅ GOOD: "UpdateCurriculum.cs (lines 41-44) blocks updates when StatusId == Active"
```

### 2. Guard Rules Documentation
Use tables for clarity:
```markdown
| Operation | Condition | Result |
|-----------|-----------|--------|
| UpdateCurriculum | StatusId == Active | ❌ Forbidden |
```

### 3. Real-World Scenarios
Include 2-3 practical examples:
```
### Scenario 1: Normal workflow
[Step by step what happens]

### Scenario 2: When guard is violated
[Why it fails, what error message]

### Scenario 3: Safe operation
[When the operation is allowed]
```

### 4. Implementation Code Snippets
For pending features, provide copy-paste ready code:
```csharp
// This shows exactly what needs to be added
public async Task<Result<CourseDto>> Handle(UpdateCourse command, ...)
{
    var activeSections = await _repo.ListAsync(...);
    if (activeSections.Any())
        return Result.Conflict(...);  // ← This is what's missing
}
```

### 5. Test Strategies
Show both unit and integration test approaches:
```
**Unit Test:** Guard blocks update when Open section exists
**Integration Test:** Verify API returns 403 Forbidden in database
```

### 6. Effort Estimates
Be realistic and specific:
```
❌ "A few hours"
✅ "2 hours: 30m spec + 1h handler + 30m tests"
```

---

## Common Pitfalls to Avoid

1. **Verifying without code:** Don't trust the research document alone — actually check the backend code for line numbers
2. **Missing edge cases:** Consider all statuses and state transitions when writing guard rules
3. **Incomplete implementation notes:** For pending features, think through all the pieces (domain methods, commands, handlers, endpoints, events, tests)
4. **Linking without context:** Always explain why features are related
5. **Ignoring priority:** Mark what's urgent vs. nice-to-have
6. **One-liner descriptions:** Provide enough context so someone unfamiliar with the domain can understand

---

## Workflow Summary

1. **Read** the research/specification document carefully
2. **Extract** all features and their claimed status
3. **Verify** each status by examining actual code files
4. **Categorize** into done/partial/pending folders
5. **Document** each feature with status, guards, scenarios, tests
6. **Create** README with core concepts and usage guidance
7. **Create** INDEX for quick reference and prioritization
8. **Cross-link** all documents
9. **Review** for accuracy, completeness, clarity

---

## Output Metrics

A well-executed analysis produces:

- **Number of feature files:** Typically 7-15 features
- **Lines of documentation:** 2,000-3,500 lines total
- **Coverage:** 90%+ of features from research document
- **Readability:** Clear enough that a developer unfamiliar with the domain can understand all features within 1 hour

---

## Tips for Success

✅ **Start with done/ features** — they're easier and build momentum  
✅ **Verify everything in code** — Don't assume; check line numbers  
✅ **Use consistent formatting** — Tables, code blocks, headers  
✅ **Tell stories with scenarios** — Not just dry rules  
✅ **Link everything together** — Cross-references help navigation  
✅ **Include implementation code** — For pending features, developers appreciate working examples  
✅ **Test your cross-links** — Make sure README.md → feature files → pending features all work  

---

## When to Stop

Your analysis is complete when:

- ✅ All features from research document are documented
- ✅ Each feature file is 150+ words with clear status
- ✅ README covers core concepts and usage patterns
- ✅ INDEX provides quick reference with percentages
- ✅ All file paths and line numbers verified in actual code
- ✅ Implementation steps for pending features include code snippets
- ✅ Cross-links are tested and working
- ✅ Someone new to the project can read README.md and understand the feature landscape in 30-45 minutes

---

## Related Documentation

- **AGENTS.md** — Project architecture overview
- **Research documents** — Source material for analysis
- **Backend code** — Source of truth for implementation status
- **Test files** — Verify guard enforcement and feature testing

---

**Skill Version:** 1.0  
**Last Updated:** 2026-06-08  
**Use Case:** Feature analysis, architecture documentation, specification breakdown  
**Domains:** Data-locking, API design, performance optimization, security architecture, any feature set analysis
