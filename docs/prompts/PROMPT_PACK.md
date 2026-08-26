# Majlis Prompt Pack

Use these prompts with Codex or another coding agent.

## 1. Repo Orientation Prompt

```text
You are working in the Majlis repository.

Before editing anything:
- Read AGENTS.md
- Read .specify/memory/constitution.md
- Read docs/ai-context/PROJECT.md
- Read docs/ai-context/ARCHITECTURE.md
- Read docs/ai-context/CONVENTIONS.md
- Read docs/ai-context/HANDOFF.md
- Read docs/product/full-app-scope.md
- Read docs/product/v1-product-decisions.md
- Read specs/003-production-app/spec.md, plan.md, and tasks.md
- Read docs/architecture/API_CONTRACTS.md and DATABASE_SCHEMA.md
- Read docs/quality/requirements-to-tests.md

Task:
Summarize the project goal, current repo structure, full production-app scope, and the next safest implementation step toward the complete release.

Scope:
- Read documentation only.
- Do not edit files.

Deliverables:
- Summary of what Majlis is.
- Key architecture constraints.
- Recommended next task.
- Any missing prerequisites.
```

## 2. Plan-First Prompt

```text
You are working in the Majlis repository.

Before doing anything:
- Read AGENTS.md
- Read .specify/memory/constitution.md
- Read docs/ai-context/PROJECT.md
- Read docs/ai-context/ARCHITECTURE.md
- Read docs/ai-context/CONVENTIONS.md
- Read docs/ai-context/HANDOFF.md
- Read docs/product/full-app-scope.md
- Read docs/product/v1-product-decisions.md
- Read specs/003-production-app/spec.md, plan.md, and tasks.md
- Read docs/architecture/API_CONTRACTS.md and DATABASE_SCHEMA.md
- Read docs/quality/requirements-to-tests.md
- Read the relevant specs/<feature>/spec.md, plan.md, and tasks.md
- Then inspect only the files needed for the task

Task:
<describe the feature or task>

Scope:
- Focus on: <target folders/files>
- Avoid touching: unrelated features, unrelated refactors, unplanned dependencies

Constraints:
- Do not edit immediately.
- Explain the current relevant structure.
- Propose the minimal implementation plan.
- Identify risks and tests.
- Wait for approval before editing.

Deliverables:
- Relevant files.
- Implementation plan.
- Test plan.
- Assumptions.
```

## 3. Feature Implementation Prompt

```text
You are working in the Majlis repository.

Before doing anything:
- Read AGENTS.md
- Read .specify/memory/constitution.md
- Read docs/ai-context/PROJECT.md
- Read docs/ai-context/ARCHITECTURE.md
- Read docs/ai-context/CONVENTIONS.md
- Read docs/ai-context/HANDOFF.md
- Read docs/product/full-app-scope.md
- Read docs/product/v1-product-decisions.md
- Read specs/003-production-app/spec.md, plan.md, and tasks.md
- Read docs/architecture/API_CONTRACTS.md and DATABASE_SCHEMA.md
- Read docs/quality/requirements-to-tests.md
- Set FEATURE_SPEC to the one approved `specs/<number>-<feature>/` folder for this task
- Read FEATURE_SPEC/spec.md, plan.md, and tasks.md
- Then inspect only the files needed for the selected task

Task:
Implement one approved unchecked task from FEATURE_SPEC/tasks.md.

Scope:
- Focus only on the selected task.
- Avoid unrelated refactors.
- Avoid implementing future features not required by the task.
- Treat the selected feature as one delivery slice; do not present it as completion of the full app.

Constraints:
- Preserve the clean architecture boundaries.
- Keep Flutter feature-first.
- Do not hardcode correct answers in the Flutter UI.
- Update tests for scoring/streak behavior when relevant.
- Do not add community features before moderation basics exist.
- Map the implemented requirement IDs to named tests/evidence in docs/quality/requirements-to-tests.md.
- Do not provision hosting, public domains, production Google/Apple credentials, App Links, or signing until the handoff records the `Game Ready` gate.

Deliverables:
- Implement the selected task.
- Mark the task complete if appropriate.
- Summarize files changed.
- Explain assumptions.

Validation:
- Run targeted tests/checks for changed areas.
- Report exact commands and results.

Handoff update:
- Update docs/ai-context/HANDOFF.md with what was completed, files changed, decisions made, blockers, and next recommended step.
```

## 4. Backend Test-First Prompt

```text
You are working in the Majlis .NET backend.

Read first:
- AGENTS.md
- docs/product/full-app-scope.md
- specs/003-production-app/spec.md, plan.md, and tasks.md
- docs/ai-context/ARCHITECTURE.md
- docs/architecture/DATABASE_SCHEMA.md
- docs/architecture/API_CONTRACTS.md
- Relevant specs folder
- docs/product/v1-product-decisions.md
- docs/quality/requirements-to-tests.md

Task:
Implement backend domain/application logic for <specific behavior> using test-first development.

Success criteria:
- Write a failing test first.
- Implement the smallest code needed to pass.
- Keep controllers thin.
- Do not access persistence directly from domain entities.
- Run targeted tests and report results.
- Update docs/ai-context/HANDOFF.md.
```

## 5. Flutter UI Prompt

```text
You are working in the Majlis Flutter Android app.

Read first:
- AGENTS.md
- docs/product/full-app-scope.md
- docs/product/v1-product-decisions.md
- specs/003-production-app/spec.md, plan.md, and tasks.md
- docs/architecture/API_CONTRACTS.md and DATABASE_SCHEMA.md
- docs/quality/requirements-to-tests.md
- docs/design/DESIGN.md
- docs/design/THEME.md
- docs/design/content-voice.md
- docs/ai-context/ARCHITECTURE.md
- Relevant spec.md, plan.md, and tasks.md

Task:
Implement <screen/component> for the Daily Majlis flow.

Constraints:
- Use feature-first structure.
- Use Riverpod for state.
- Use centralized theme tokens.
- Keep copy warm, short, and non-shaming.
- Do not expose correct answer before backend submission.

Deliverables:
- UI implementation.
- State handling.
- Basic widget tests if practical.
- Summary and handoff update.
```
