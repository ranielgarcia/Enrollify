# Enrollify Feature Analysis Skill — Usage Guide

## Quick Start

### When to Use This Skill

Use the **enrollify-feature-analysis** skill when you have:

✅ A research document describing features and their implementation status  
✅ A technical specification needing to be broken down  
✅ A markdown file listing what's implemented and what's missing  
✅ A meeting summary or design doc with feature statuses  

**Any source that describes:** "Feature X is implemented", "Feature Y is missing", "Feature Z is partial"

### Basic Usage Pattern

```
User: "Review this research document and break down 
       the features into done/pending folders with 
       detailed documentation."

[User provides file path to document]

Copilot: "I'll use the enrollify-feature-analysis skill to:
         1. Analyze the research document
         2. Verify implementation status in code
         3. Create done/ folder with X implemented features
         4. Create pending/ folder with Y unimplemented features
         5. Generate README.md with architecture guide
         6. Generate INDEX.md with quick reference"
```

---

## Step-by-Step Instructions

### 1. Provide the Research Document

Give the Copilot:
- **File path:** `docs/research/your-document.md`
- **Context:** What domain is this? (data-locking, security, performance, etc.)
- **Output location:** Where should docs go? (e.g., `docs/feature-analysis/` or `docs/your-domain-architecture/`)

### 2. Copilot Analyzes

The skill will:
1. Read your research document
2. Extract all mentioned features
3. Identify which are implemented/partial/pending
4. Verify against actual backend code
5. Create feature documentation

### 3. Review Output

After completion:
- Browse `README.md` for overview
- Check `INDEX.md` for quick reference
- Read individual feature files in `done/` and `pending/`
- Verify file paths and line numbers are correct

### 4. Request Adjustments

If needed:
- "Add more detail to feature X"
- "Fix the line numbers for UpdateCourse"
- "Add test strategies for pending features"

---

## Command Examples

### Example 1: Analyze Data-Locking Research

```
@docs/research/how-to-properly-locked-the-important-data-like-cur.md

Review this research document and create a comprehensive feature analysis.
Break down all features into:
- done/ folder for fully implemented features
- pending/ folder for missing/partial features
Create README.md and INDEX.md.

Output location: docs/data-locking-architecture/
```

### Example 2: Analyze Multiple Documents

```
Review these documents:
@docs/research/api-security-requirements.md
@docs/design/performance-optimization-plan.md

Create a combined feature analysis covering both documents.
Output to: docs/system-architecture/
```

### Example 3: Analyze Existing Markdown

```
@docs/existing-feature-list.md

This markdown lists features with their status (✅/⚠️/❌).
Break them down into organized documentation following the skill pattern.
Output to: docs/feature-analysis/
```

---

## What Each Section of Output Contains

### done/ Folder (Fully Implemented Features)

Each file in `done/` describes a **working, complete feature**:

```
File: 01-feature-name.md

Contents:
├── Overview (1-2 sentences)
├── Status: ✅ FULLY IMPLEMENTED
├── Current Implementation
│   ├── File: path/to/File.cs (lines X-Y)
│   ├── How it works (with code snippets)
│   └── Related components
├── Guard Rules (table format)
├── Real-World Scenarios (2-3 examples)
├── Testing (unit + integration approaches)
└── Notes (key design decisions)

Typical length: 150-300 words
Code snippets: Yes
Line numbers: Always verified in actual code
```

### pending/ Folder (Partial or Not Implemented)

Each file in `pending/` describes **missing or incomplete features**:

```
File: 01-feature-name.md

Contents:
├── Overview
├── Status: ⚠️ PARTIAL (or ❌ NOT IMPLEMENTED)
├── Current State (what exists, what's missing)
├── Missing/Gap Analysis
├── Proposed Implementation
│   ├── Files to create/modify
│   ├── Full code snippets (copy-paste ready)
│   ├── Step-by-step implementation (10-15 steps)
│   └── Testing approach
├── Priority (Low/Medium/High)
├── Effort Estimate (2h, 4h, 1 day, etc.)
└── Related Features (dependencies)

Typical length: 250-450 words
Code snippets: Yes, complete implementations
Line numbers: Verified where applicable
```

### README.md (Architecture Guide)

```
Contents:
├── Overview & Quick Status
├── Folder Structure (visual tree)
├── Core Concepts (the "why")
│   ├── Problem statement
│   ├── Solutions
│   └── Design patterns
├── Implementation Status by Feature (detailed breakdown)
├── Usage Guide
│   ├── For Developers
│   ├── For API Designers
│   ├── For QA/Testers
│   └── For System Admins
├── Key Files Reference (backend structure)
├── Related Documentation (links)
├── Common Q&A
├── Next Steps / Roadmap
└── Summary

Typical length: 600-1000 words
Audience: Architects, lead developers, project managers
Purpose: Understand the feature landscape and architecture
```

### INDEX.md (Quick Reference)

```
Contents:
├── Overview table with all features, status, metrics
├── File organization tree
├── Key concepts (3-4 summary paragraphs)
├── Feature checklist (visual progress)
├── Implementation completeness (percentage)
├── Priority and effort matrix
├── Next steps (prioritized list)
└── Pro tips for different roles

Typical length: 300-500 words
Audience: Everyone (developers, QA, admins, managers)
Purpose: Quick lookup and navigation
Time to read: 10-15 minutes
```

---

## Output Quality Checklist

After the skill completes, verify:

- [ ] All features from research document are documented
- [ ] Each feature file has clear status (✅/⚠️/❌)
- [ ] File paths are correct and verified in actual code
- [ ] Line numbers match actual code (spot-check 3-5 files)
- [ ] Guard rules are documented as tables
- [ ] Real-world scenarios make sense
- [ ] Code snippets in pending/ are copy-paste ready
- [ ] README.md covers core concepts clearly
- [ ] INDEX.md quick reference is accurate
- [ ] Cross-links work (README → features → pending)
- [ ] Tone is consistent and professional
- [ ] No unverified claims (all have code references)

---

## Customization

### Change Output Location

If you want documentation in a different folder:

```
"Create the same analysis but output to: docs/my-custom-folder/"
```

### Add Custom Sections

Request additional sections in README.md:

```
"Add a 'Migration Strategy' section to README.md"
"Include database schema examples for each feature"
```

### Focus on Specific Features

```
"Focus on only the pending features.
Create detailed implementation guides.
Skip the done/ folder."
```

### Change Documentation Style

```
"Use a more technical tone for the README.
Include ASCII diagrams for state machines.
Add performance implications for each feature."
```

---

## Common Adjustments

### If Line Numbers Are Wrong

```
"The file path for UpdateSubject is incorrect.
It's in Enrollify.Application/Features/Subjects/Commands/UpdateSubject.cs
Please verify and update the line numbers."
```

### If a Feature Was Missed

```
"You missed Feature X from the research document.
It's mentioned in section 'XXX' around line 500.
Please add documentation for it."
```

### If Priority Seems Off

```
"Update the pending features priority list.
Feature A (currently Low) should be Medium priority.
Feature B should be moved to Priority 1."
```

### If Code Snippets Need Improvement

```
"The proposed implementation for Feature X needs:
- More comments explaining the logic
- Example test case
- How to integrate with existing handlers"
```

---

## Tips for Best Results

### When Providing the Research Document

✅ **Good:**
```
Review @docs/research/how-to-properly-locked-the-important-data-like-cur.md

Analyze all features mentioned and categorize them.
Output to docs/data-locking-architecture/
```

❌ **Could be better:**
```
Break down the research document.
```

### When Specifying Output Location

✅ Clear and specific:
```
Output to: docs/feature-analysis/
```

❌ Vague:
```
Put the docs somewhere
```

### When Requesting Adjustments

✅ Specific about what changed:
```
"UpdateCourse line numbers are wrong.
Should be lines 30-52, not 40-60.
Also add the missing guard code snippet."
```

❌ Vague:
```
"Fix the documentation"
```

---

## Integration with Development Workflow

### Before Implementation Sprint

1. Use skill to analyze feature design doc
2. Review `pending/` folder for implementation details
3. Copy code snippets as starting points
4. Create tickets from `Next Steps` section
5. Estimate effort from documentation

### During Implementation

1. Reference the feature file's "implementation steps"
2. Use code snippets as templates
3. Update test strategies from the documentation
4. Link GitHub issues to feature file

### For Code Review

1. Check implementation against documented guard rules
2. Verify tests match documented test strategies
3. Ensure implementation steps were followed
4. Confirm code matches provided snippets

### For Documentation Maintenance

1. Keep README.md's roadmap in sync with actual progress
2. Move completed pending features to done/ folder
3. Update INDEX.md percentages as features complete
4. Archive old analyses in version control

---

## Skill Limitations

The skill does **NOT**:
- ❌ Write code implementations (only provides templates)
- ❌ Create tests (only provides testing strategies)
- ❌ Make architectural decisions (documents existing/proposed architecture)
- ❌ Automatically verify all code references (spot-checks, not exhaustive)
- ❌ Create database migrations
- ❌ Deploy changes

The skill **DOES**:
- ✅ Analyze and categorize features
- ✅ Create comprehensive documentation
- ✅ Provide implementation starting points
- ✅ Verify key code references
- ✅ Organize information for developers
- ✅ Produce actionable next steps

---

## FAQ

**Q: How long does analysis take?**  
A: Typically 10-15 minutes for a document with 8-12 features. Larger documents take proportionally longer.

**Q: Can I use this for non-Enrollify projects?**  
A: Yes! The skill is domain-agnostic. Replace file paths and code references with your project's structure.

**Q: What if my research document is incomplete?**  
A: The skill will analyze what's there and note any gaps. You can refine the research document and re-run the skill.

**Q: Can I re-run the skill on the same document?**  
A: Yes. The skill can:
- Update existing documentation with corrections
- Add new features you find
- Reorganize existing files
- Regenerate README.md/INDEX.md

**Q: How do I keep documentation in sync with code?**  
A: Treat feature files as living documentation. Update them when:
- Guard rules change
- New commands/handlers are added
- Status moves from pending → done
- Testing strategies evolve

---

## Getting Help

If the skill produces output that needs adjustment:

1. **Be specific:** "Line 64 should be 74", not "fix the numbers"
2. **Show examples:** Paste the exact text that's wrong
3. **Suggest alternatives:** "Should include a state machine diagram like this: [ASCII]"
4. **Reference sections:** "Update the 'Guard Rules' table for UpdateSubject"

---

## Success Criteria

You've successfully used the skill when:

✅ You can read `README.md` and understand the feature landscape in 30 minutes  
✅ A new team member can reference feature files to understand implementation  
✅ Developers implementing pending features have code snippets to start from  
✅ Code reviewers have a checklist of guard rules to verify  
✅ File paths and line numbers are accurate and verifiable  
✅ The `done/pending/` split matches actual implementation status  
✅ Future researchers can trace back from feature files to code  

---

**Skill Name:** enrollify-feature-analysis  
**Designed by:** Architecture Copilot  
**Updated:** 2026-06-08  
**For questions:** See SKILL.md or request clarification from the Copilot
