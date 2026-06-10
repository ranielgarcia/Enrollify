# Enrollify Feature Analysis Skill

## Overview

The **enrollify-feature-analysis** skill is a comprehensive, reusable workflow for analyzing technical research documents, specifications, and markdown files that describe software features and their implementation status.

**Use this skill when you have a document describing:**
- Features that are implemented, partially implemented, or not yet implemented
- Architecture decisions and design patterns
- Technical gaps or missing functionality
- Feature dependencies and relationships

## What This Skill Does

Transforms your research/specification document into organized, actionable feature documentation:

```
Input:  Research document (your-document.md)
   ↓
Analysis: Extract features, verify implementation status in code
   ↓
Output: Organized documentation with:
  ├── done/               (fully implemented features)
  ├── pending/            (partial/not implemented)
  ├── README.md          (architecture guide)
  └── INDEX.md           (quick reference)
```

### Output Quality

- **2,000-3,500 lines** of organized documentation
- **File paths & line numbers** verified against actual code
- **Guard rules** documented in table format
- **Real-world scenarios** with concrete examples
- **Implementation code snippets** (copy-paste ready for pending features)
- **Testing strategies** for both unit and integration tests

## Files in This Skill

| File | Purpose | Read First? |
|------|---------|------------|
| **SKILL.md** | Complete step-by-step guide (1,800+ lines) | ✅ Yes |
| **USAGE.md** | Quick usage examples and common patterns (600+ lines) | ✅ Yes |
| **TEMPLATE_DONE.md** | Template for fully implemented features | As needed |
| **TEMPLATE_PENDING.md** | Template for partial/not-implemented features | As needed |
| **README.md** | This file | You are here |

## Quick Start

### Minimal Command

```
Review @path/to/research-document.md and create feature analysis.
Output to: docs/feature-analysis/
```

### What Happens

1. Skill analyzes your research document
2. Extracts all mentioned features
3. Categorizes them (done/pending)
4. Verifies implementation status in backend code
5. Creates organized documentation with README + INDEX
6. Ready for developers to reference

### Expected Output

```
docs/feature-analysis/
├── done/
│   ├── 01-feature-name.md      (150-300 words)
│   ├── 02-feature-name.md
│   └── ...
├── pending/
│   ├── 01-feature-name.md      (250-450 words)
│   ├── 02-feature-name.md
│   └── ...
├── README.md                    (600-1000 words, architecture guide)
└── INDEX.md                     (300-500 words, quick reference)
```

## Real-World Example

See the **actual output** from using this skill:

```
docs/data-locking-architecture/        ← Real example created with this skill
├── done/                              (7 implemented features)
│   ├── 01-curriculum-status-enum.md
│   ├── 02-course-curriculum-assignment.md
│   ├── 03-class-section-status-machine.md
│   └── ...
├── pending/                           (2 incomplete features)
│   ├── 01-update-course-guards.md
│   └── 02-curriculum-phase-out-and-archive.md
├── README.md
└── INDEX.md
```

**Source document:** `docs/research/how-to-properly-locked-the-important-data-like-cur.md` (763 lines)

**Result:** 2,016 lines of organized documentation across 10 files.

## Use Cases

### Use Case 1: Architecture Documentation
You have a design document describing an architectural pattern (data-locking, API security, caching strategy, etc.). Use this skill to break it down into feature documentation.

### Use Case 2: Research Verification
You have research documenting what's implemented vs. missing. Use this skill to verify claims against actual code and create organized documentation.

### Use Case 3: Specification Breakdown
You have a technical specification or RFC. Use this skill to organize features by implementation status and prioritize missing work.

### Use Case 4: Knowledge Transfer
You need to document what's implemented so new team members understand the feature landscape. Use this skill to create comprehensive, organized guides.

### Use Case 5: Sprint Planning
You need to identify and prioritize remaining work. Use this skill to generate a roadmap with effort estimates.

## How to Use This Skill

### Step 1: Read the Main Guide

Start with **SKILL.md** — it contains:
- 5 detailed phases (pre-flight, categorization, documentation, README, assembly)
- Step-by-step instructions for each phase
- Best practices and common pitfalls
- Worked example

### Step 2: Understand Output Structure

Review **USAGE.md** for:
- Quick start patterns
- Command examples
- What each output section contains
- Quality checklist
- Common customizations

### Step 3: Use Templates

When creating feature files:
- **For implemented features:** Copy `TEMPLATE_DONE.md`
- **For pending features:** Copy `TEMPLATE_PENDING.md`
- Fill in sections for your feature
- Follow the template structure for consistency

### Step 4: Request Analysis

Tell the Copilot to use this skill:

```
Please use the enrollify-feature-analysis skill to analyze 
@docs/research/my-document.md

Create comprehensive feature documentation with:
- done/ folder for implemented features
- pending/ folder for unimplemented features
- README.md with architecture guide
- INDEX.md with quick reference

Output to: docs/my-feature-analysis/
```

## Key Features of This Skill

✅ **Reusable** — Works for any domain (data-locking, security, performance, etc.)  
✅ **Comprehensive** — Includes templates, guides, and best practices  
✅ **Verification-Focused** — All file paths and line numbers verified in actual code  
✅ **Developer-Friendly** — Includes code snippets and implementation templates  
✅ **Well-Organized** — Clear folder structure (done/pending) and cross-linked docs  
✅ **Actionable** — Pending features include step-by-step implementation plans  
✅ **Tested** — Already used successfully for data-locking architecture  

## What's Included

### SKILL.md (1,800+ lines)
The complete, authoritative guide covering:
- Pre-flight analysis of input documents
- Feature categorization methodology
- Detailed documentation templates for each phase
- README.md structure and content guidelines
- INDEX.md quick reference format
- Quality assurance checklist
- Best practices and pitfalls
- Worked example from start to finish

### USAGE.md (600+ lines)
Practical usage guide with:
- When to use this skill
- Command examples (minimal to complex)
- What to expect in each section of output
- Customization options
- Integration with development workflow
- Troubleshooting common issues
- FAQ section

### TEMPLATE_DONE.md (400+ lines)
Ready-to-use template for documenting fully implemented features:
- Overview section
- Current implementation details
- Guard rules (table format)
- Real-world scenarios (3+ examples)
- Implementation details explanation
- Related features
- Testing strategies (unit + integration)
- Architecture decisions
- Troubleshooting
- Summary

### TEMPLATE_PENDING.md (500+ lines)
Ready-to-use template for documenting incomplete/missing features:
- Status and gaps clearly stated
- What currently exists vs. what's missing
- Complete code snippets for missing pieces
- Step-by-step implementation plan with effort breakdown
- Guard rules and testing strategy
- Priority & effort assessment
- Dependencies tracking
- Success criteria

## Quick Reference

### When to Use This Skill

✅ **Good fit:**
- "Analyze this research document and create feature documentation"
- "Break down our API security spec into done/pending folders"
- "Document what's implemented vs. missing in our caching layer"
- "Create an architecture guide for our data-locking system"

❌ **Not the right tool:**
- Writing code implementations (use other skills for that)
- Creating tests from scratch (skill provides strategies, not test code)
- Making architectural decisions (documents existing/proposed decisions)

### Input Requirements

The skill works best with documents that describe:
- Feature names and descriptions
- Implementation status (implemented/partial/missing)
- Where features are implemented (file paths)
- What guards/constraints exist
- Related features and dependencies

### Output Guarantees

The skill will produce:
- ✅ Organized folder structure (done/pending)
- ✅ Verified file paths and line numbers (spot-checked in code)
- ✅ Guard rules in table format
- ✅ Real-world scenarios with concrete examples
- ✅ README with core concepts and usage guidance
- ✅ INDEX with quick reference and metrics
- ✅ Cross-linked documentation
- ✅ 2,000+ lines of total documentation

## Integration with Enrollify Project

This skill was designed for the Enrollify enrollment management system but works for any project:

**Enrollify Architecture Domains:**
- ✅ Data-Locking Architecture (`docs/data-locking-architecture/`)
- ✅ API Security (potential)
- ✅ Performance Optimization (potential)
- ✅ Testing Infrastructure (potential)

**Other Projects:**
- Microservices architecture documentation
- API feature documentation
- Infrastructure as Code specifications
- Database schema documentation
- Any feature set with implementation status

## Success Indicators

You've successfully used the skill when:

✅ README.md helps new team members understand the feature landscape in 30 minutes  
✅ Developers implementing pending features can use code snippets as starting points  
✅ Code reviewers have a checklist of guard rules to verify  
✅ File paths and line numbers are accurate and verifiable  
✅ done/pending categorization matches actual implementation status  
✅ Cross-links work and help navigation  
✅ Someone unfamiliar with the domain can understand the architecture  

## Version & Maintenance

| Aspect | Value |
|--------|-------|
| **Skill Name** | enrollify-feature-analysis |
| **Version** | 1.0 |
| **Created** | 2026-06-08 |
| **Status** | Stable & Tested |
| **Test Case** | `docs/data-locking-architecture/` |
| **Domains** | Data-locking, Architecture documentation, Feature analysis |

## Troubleshooting

**Q: Analysis takes too long**  
A: For documents with 20+ features, expect 15-20 minutes. For smaller documents (5-8 features), expect 5-10 minutes.

**Q: File paths are wrong**  
A: The skill verifies line numbers by reading actual code. If paths are wrong, provide corrections and the skill can update them.

**Q: I want to add more features**  
A: You can re-run the skill on an updated research document, or request the skill to add specific features.

**Q: Can I customize the output style?**  
A: Yes. Request changes like "use a more technical tone" or "include ASCII diagrams for state machines."

## Next Steps

1. **Read SKILL.md** — Understand the complete process (20 min read)
2. **Skim USAGE.md** — See practical examples (10 min read)
3. **Request analysis** — Tell Copilot to use the skill on your document
4. **Review output** — Check that features are correctly categorized
5. **Make adjustments** — Request corrections if needed
6. **Use in workflow** — Reference feature files in development/review

## Related Skills

- **enrollify-integration-tests** — Writing tests for backend features
- **enrollify-management-page** — Implementing CRUD pages in frontend
- **enrollify-ui-redesign** — Visual design improvements
- **grill-me** — Stress-testing plans and designs

## Questions or Feedback?

See the Enrollify project's main AGENTS.md for additional context and related skills.

---

**Skill Type:** Feature Analysis & Documentation  
**Complexity:** Medium (straightforward workflow, detailed templates)  
**Learning Curve:** Moderate (read SKILL.md + USAGE.md: ~30 min)  
**Reusability:** High (works for any domain with documented features)  
**Maintenance:** Update if skill-specific instructions change in `.github/skills/enrollify-feature-analysis/`  

**Last Updated:** 2026-06-08
