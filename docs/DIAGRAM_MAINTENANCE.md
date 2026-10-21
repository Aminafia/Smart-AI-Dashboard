# UML and Architecture Diagram Maintenance

## Goal

Keep the Smart AI Dashboard architecture documentation useful as the code changes, without pretending that a tool can infer every business workflow perfectly from source code.

The diagrams are part of the engineering workflow. They should explain both the current implementation and the intended architecture, and they must not silently drift from the code.

## What can be automated reliably?

| Artifact/check | Approach | Expected automation |
|---|---|---|
| Repository source-file inventory | Generate from the Git working tree | Fully generated |
| .NET project/layer dependency map | Read project references from `.csproj` files | Generated from project metadata |
| API route inventory | Extract controller route attributes or use an OpenAPI document generated from the running API | Generated, with runtime/OpenAPI caveats |
| Entity/database model overview | Combine EF Core model/migration metadata with a reviewed diagram | Partially generated; review relationships and semantics |
| Angular route inventory | Read route configuration and lazy-loaded feature references | Partially generated |
| Sequence diagrams for signup, generation, document workflows, retries and failure handling | Maintain Mermaid diagrams alongside the implementation; use source-file references and verification checklists | Human-reviewed, with automated drift checks |
| High-level system architecture and future AWS target | Maintain explicit current-state and target-state diagrams | Human-reviewed |

## Why sequence diagrams should not be blindly generated

A source scanner can find method calls, route attributes and dependencies, but it generally cannot produce a trustworthy business-level sequence diagram for asynchronous work, retries, ownership checks, cancellation, conditional error paths or future architecture. Those diagrams need to preserve intent and explain why a call happens, not just which method calls another method.

Therefore, the strategy is **hybrid**: generate facts that are mechanical, keep behavior diagrams reviewed by a developer, and automate checks that catch likely drift.

## Proposed repository layout

- `docs/ARCHITECTURE_BLUEPRINT.md` — master current-state architecture and future direction.
- `docs/WORKFLOW_UML_HANDBOOK.md` — reviewed end-to-end Mermaid workflows.
- `docs/STABILIZATION_BACKLOG.md` — prioritized engineering work and verification plan.
- `docs/DIAGRAM_MAINTENANCE.md` — this maintenance policy.
- `docs/generated/` — generated inventories/diagrams only, clearly labelled as generated.
- `scripts/` — deterministic generation and validation scripts.
- `.github/workflows/` — run documentation checks on pushes to `main` and on pull requests if/when used.

Do not mix generated output and hand-authored workflow explanations in a way that causes a generator to overwrite reviewed content.

## Safe rollout plan

1. **Baseline first:** preserve the current Mermaid diagrams and validate their source-file references.
2. **Automate inventory:** generate a fresh source-file index from the repository tree, including the `ai-engine/` prototype and current docs.
3. **Add deterministic checks:** verify Mermaid fences are balanced, referenced repository paths exist, and generated inventory is up to date.
4. **Generate structural views:** add a .NET project dependency diagram from `.csproj` references and a route inventory from the current API/frontend route declarations.
5. **Add CI checks:** run the generator in check mode on pushes to `main`. A mismatch should fail with an actionable message telling the developer which command refreshes the generated files.
6. **Maintain behavioral diagrams:** whenever a route, handler, provider, persistence relationship, status transition, middleware order or error branch changes, update the affected Mermaid diagram and checklist in the same change.
7. **Review after stabilization:** add stronger Mermaid rendering and OpenAPI-based checks only after the initial deterministic checks are reliable.

## Rules

- Generated diagrams must say which source metadata they were generated from and when appropriate identify the commit.
- Hand-authored sequence diagrams remain the reviewed explanation of actual behavior.
- Do not update the architecture diagram solely to match a code change if the code change is itself incorrect; resolve intended behavior first.
- Keep current-state diagrams separate from future AWS diagrams.
- Never label a workflow verified because a diagram renders or a build passes.
- Run the final manual end-to-end checklist after stabilization; automated diagram checks do not replace runtime verification.

## Current status

This document records the agreed direction. The first implementation step now exists: `scripts/check_architecture_docs.py` can regenerate the tracked source inventory and check Markdown fence balance and inline source-path references. `.github/workflows/architecture-docs.yml` runs the checker on pushes to `main` and on pull requests. The first workflow runs are still pending/in progress; do not call the checks passed until a completed run confirms that result. The current automation does not generate semantic sequence diagrams or infer runtime behavior.
