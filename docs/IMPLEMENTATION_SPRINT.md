# Smart AI Dashboard — Active Implementation Plan

**Purpose:** the working sequence for completing the existing application before cloud deployment. This plan supersedes the historical sprint note that began at commit `0c43eb3`; that commit remains historical context only.

## Locked-in execution order

1. **Finish source audit and documentation reconciliation**
   - Keep the architecture blueprint, workflow handbook, generated inventory and stabilization backlog aligned with `main`.
   - Separate source-traced facts from runtime-verified results.
   - Defer Mermaid rendering and GitHub Actions issues until the planned documentation/tooling pass, as already agreed.

2. **Establish a local baseline**
   - Inspect the current branch and working tree.
   - Run backend restore/build and frontend install/build/test commands in the existing environment.
   - Record exact commands, outcomes and existing failures before changing code.
   - Use a disposable/test database for migration checks; do not risk personal data or rewrite migrations already applied.

3. **Remediate P0 security and data-integrity issues**
   - Resolve email normalization/uniqueness and inspect existing data before a forward migration.
   - Review ownership foreign keys and the historical ownership migration/backfill.
   - Restrict diagnostic/error-detail endpoints and verify role and cross-user isolation.
   - Validate production configuration and secret injection.
   - Remove prompt/document contents and other sensitive values from logs/cache-key diagnostics.

4. **Stabilize P1 workflows and reliability**
   - Fix cancellation propagation, AI job retry/recovery and bounded pagination/input.
   - Verify upload, preview, extraction, download, summary rendering, saved summary retrieval and deletion consistency.
   - Fix frontend polling teardown/error behavior and stale-page document lookup.
   - Review storage-path boundaries and dependency health checks.

5. **Add meaningful automated tests and complete local verification**
   - Replace stale starter assertions and scaffold-only frontend tests with behavior tests.
   - Add a backend test project and targeted unit/integration tests.
   - Execute the consolidated workflow/security checklist in `docs/STABILIZATION_BACKLOG.md`.
   - Record observed results in `docs/WORKFLOW_UML_HANDBOOK.md`; a successful build alone does not mean a workflow is verified.

6. **Begin hands-on AWS deployment only after local gates pass**
   - Choose the target topology and trust boundaries.
   - Containerize the API/worker, move documents to S3, use a durable queue such as SQS where justified, deploy to an appropriate compute service, and configure IAM, networking, secrets, observability and CI/CD.
   - Learn each AWS component by implementing, verifying, safely exercising failures, troubleshooting and documenting it inside this project.

## Working rules

- Work directly on `main` as the sole developer; do not introduce a PR-only workflow.
- Do not change application features or database schema during source-audit/documentation-only sessions.
- Never rewrite a migration that may have been applied; use a reviewed forward migration after inspecting schema and data.
- Do not introduce Kubernetes, Kafka, microservices or other infrastructure without a concrete requirement.
- Treat `ai-engine/` as a separate research prototype until its integration and security boundary are deliberately designed.
- Keep the current application’s purpose intact: a unified dashboard demonstrating full-stack engineering, AI integration, reliable workflows and practical cloud engineering.
