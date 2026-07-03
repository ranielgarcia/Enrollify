---
name: enrollify-to-user-stories
description: >
  Turn a PRD markdown file into individual user story markdown files under a
  specified output folder. Each story gets its own file with YAML frontmatter
  and structured sections. Use when the user asks to decompose a PRD into user
  story files or generate stories from a PRD document.
---

# Enrollify To User Stories Skill

Read a PRD `.md` file, decompose its user stories into individual properly-formatted story files under a specified folder. Follow every phase in order. Do not skip phases.

---

## Reference Files (Read These First)

- **PRD template (source format):** `.github/skills/to-prd/SKILL.md` — the PRD format this skill expects as input
- **Existing skill pattern:** `.github/skills/to-issues/SKILL.md` — sibling skill for similar decomposition workflows

---

## Phase 0 — Pre-flight: Read & Parse the PRD

Receive two inputs from the user:

| Input | Description | Example |
|-------|-------------|---------|
| `prdPath` | Path to the PRD markdown file | `docs/prds/course-scheduling.md` |
| `outputPath` | Target folder for generated story files (default: `docs/user-stories/{feature-slug}/`) | `docs/user-stories/course-scheduling/` |

Read the PRD file. Extract the following sections:

| PRD Section | Used For |
|-------------|----------|
| **User Stories** | Primary source — each numbered story produces one file |
| **Implementation Decisions** | Populates **Technical Notes** per story |
| **Testing Decisions** | Informs **Acceptance Criteria** + **Technical Notes** |
| **Out of Scope** | Populates **Out of Scope** per story |
| **Problem Statement / Solution** | Context for actors, priorities, and inferred details |

**Parse the User Stories list:**
- Each line matching `{N}. As {an actor}, I want {a feature}, so that {a benefit}` yields the `actor`, `feature`, and `benefit` fields
- Stories listed first or emphasized in _Implementation Decisions_ are candidates for `priority: high`
- Stories describing cross-cutting concerns get `labels` like `["infrastructure", "devops"]`

<prd-line-example>
1. As an Admin, I want to create course sections, so that I can set up the academic catalog for the semester.
</prd-line-example>

When a PRD story is vague (missing the triplet), infer from the Solution and Implementation Decisions sections. Note the inference in Technical Notes.

---

## Phase 1 — Decompose into Individual Stories

Build an in-memory story plan. For each story from the PRD:

### 1. Parse the Triplet

```
actor   = segment after "As {a/an}" and before ", I want"
feature = segment after ", I want" and before ", so that"
benefit = segment after ", so that"
```

Strip surrounding whitespace. Capitalize `actor` as a role name (e.g., `Admin`, `Instructor`, `Student`).

### 2. Assign a Unique ID

Format: `US-{NNN}` (zero-padded to 3 digits), incrementing from `US-001`.

### 3. Derive a Title

Use the `feature` segment, trimmed and capitalized sentence-style. Max ~60 chars.

```
"As an Admin, I want to create course sections, so that ..."
→ title: "Create course sections"
```

### 4. Set Priority

| Clue | Priority |
|------|----------|
| Story is #1 in the PRD's list | `high` |
| PRD emphasizes it in Implementation Decisions | `high` |
| It's a dependency for many other stories | `high` |
| It's a nice-to-have / stretch goal | `low` |
| Everything else | `medium` |

### 5. Estimate Story Points

| Complexity | Points | Indicators |
|------------|--------|------------|
| Trivial (single field, no new logic) | 1 | One acceptance criterion, no new endpoints |
| Medium (CRUD for one entity) | 3 | Default — standard create/update/list |
| Complex (multi-entity, business rules) | 5 | Workflow logic, validation rules, permissions |
| Very Complex (cross-cutting, new system) | 8 | New module, infrastructure changes, external integrations |

Default to **3** unless the PRD clearly indicates otherwise.

### 6. Assign Labels

Derive from the PRD's domain/module vocabulary and the story's content. Common labels:

- `frontend` — UI work (forms, tables, pages)
- `backend` — API / service layer work
- `database` — schema migrations, seed data
- `infrastructure` — deployment, CI/CD, containers
- `testing` — test infrastructure, test data
- Based on project's domain (e.g., `courses`, `scheduling`, `room-management`)

### 7. Identify Dependencies

Scan other stories in the plan:

- If story B references a feature that story A creates, add `US-A` to B's `dependencies`
- If the PRD's Implementation Decisions describe ordering, encode it here
- If ambiguous, leave `dependencies: []`

### 8. Write Acceptance Criteria

Derive from the PRD's **Solution**, **Implementation Decisions**, and **Testing Decisions** sections:

- Each criterion should be a specific, testable condition
- Include at least 1 positive case and 1 negative/edge case
- Pull specific assertions from the Testing Decisions section
- Format: `{N}. {condition}` starting with a strong verb (Should, Must, Displays, Returns, Redirects...)

### 9. Write Technical Notes

Pull from **Implementation Decisions**:

- Architecture decisions relevant to this story
- API contracts, endpoint paths, request/response shapes
- Schema changes, new properties, migration notes
- Module boundaries, new files, or interfaces to create
- Cross-reference IDs from the PRD (e.g., "See Implementation Decision #3")

### 10. Write Out of Scope

Inherit from the PRD's **Out of Scope** section. Only include items relevant to this story's domain. If the PRD has no Out of Scope section, omit this section from the story file.

### Verify the Plan

Before writing, confirm the plan covers every user story listed in the PRD. If the PRD has N stories, you should have N entries in the plan. If any story was too vague to decompose, stop and ask the user for clarification.

---

## Phase 2 — Write Story Files

### 2a. Create Output Directory

Create `{outputPath}/` (all parent directories).

### 2b. Write Individual Story Files

For each story in the plan, write `{outputPath}/{id}-{kebab-case-title}.md`:

```markdown
---
id: "{id}"
title: "{title}"
actor: "{actor}"
feature: "{feature}"
benefit: "{benefit}"
priority: {priority}
story_points: {storyPoints}
labels: [{labels}]
dependencies: [{dependencies}]
---

## Description

As {a/an} **{actor}**, I want **{feature}**, so that **{benefit}**.

## Acceptance Criteria

1. {criterion 1}
2. {criterion 2}
3. {criterion 3}

## Technical Notes

- {note 1}
- {note 2}

## Out of Scope

- {item 1}
- {item 2}
```

Kebab-case title rules:

- Lowercase
- Replace spaces/special chars with `-`
- Collapse consecutive `-`
- Strip leading/trailing `-`
- Max 60 chars (truncate if needed, preserving whole words)

### 2c. Write Index File

Write `{outputPath}/index.md`:

```markdown
# User Stories: {Feature Name}

> Generated from: `{prdPath}`

| ID | Title | Actor | Priority | Points | Dependencies |
|----|-------|-------|----------|--------|--------------|
| {id} | {title} | {actor} | {priority} | {points} | {deps} |
| ... | ... | ... | ... | ... | ... |
```

Derive `Feature Name` from the PRD filename (strip extension, title-case) or from the PRD's own title if present.

---

## Phase 3 — Verify

1. Confirm `{outputPath}/index.md` exists
2. Confirm each story in the plan has a corresponding `{id}-*.md` file
3. Validate all dependency references: if story A lists `US-002` as a dependency, verify that `US-002-*.md` exists
4. Print a summary:

```
Generated 5 user stories under docs/user-stories/course-scheduling/

  US-001  Create course sections          high    3  -
  US-002  Manage room assignments         medium  5  US-001
  US-003  View schedule conflicts         medium  3  US-001, US-002
  US-004  Export schedule to PDF          low     3  US-001
  US-005  Notify instructors of changes   medium  5  US-003
```

5. If any PRD user story does not have a corresponding file, report it as an error

---

## Common Pitfalls and Solutions

| Pitfall | Solution |
|---------|----------|
| PRD has vague user stories without clear actor/feature/benefit | Use the PRD's Solution section to infer; note the inference in **Technical Notes** |
| Stories reference cross-cutting concerns | Add both stories' IDs to each other's `dependencies`; add shared labels |
| Too many stories (20+) | Group related stories under an `epic:` label or create sub-directories |
| PRD title is unclear / file has a generic name | Ask the user for the feature name before generating |
| Story file already exists at output path | Warn and skip (do not overwrite); report which files were skipped |
| Dependency references a story ID that doesn't exist | Remove the reference and add a Technical Note about the gap |
| Output path contains special characters | Normalize the path; warn the user if normalization occurred |

---

## Checklist

- [ ] Read the PRD file and extracted User Stories, Implementation Decisions, Testing Decisions, Out of Scope
- [ ] Parsed each user story's actor, feature, and benefit triplet
- [ ] Assigned unique IDs (US-001, US-002, ...)
- [ ] Set priority, story points, labels, and dependencies for each story
- [ ] Wrote acceptance criteria from the PRD's Solution and Testing sections
- [ ] Wrote technical notes from the PRD's Implementation Decisions
- [ ] Created `{outputPath}/` directory
- [ ] Wrote one `{id}-{kebab-title}.md` file per story
- [ ] Wrote `{outputPath}/index.md` summary table
- [ ] Verified all PRD stories are covered
- [ ] Verified no broken dependency references
