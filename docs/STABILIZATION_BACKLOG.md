# Smart AI Dashboard — Stabilization Backlog

**Purpose:** consolidate source-tracing findings into a practical order of work before the final end-to-end verification pass and AWS deployment.

**Scope note:** this backlog is based on source inspection of the current `main` branch and the workflow handbook. Items are not runtime-confirmed unless explicitly marked otherwise. It is a prioritized plan, not a claim that every item is a reproducible defect.

## Priority definitions

- **P0 — security/data-integrity gate:** resolve before treating the app as safe to deploy publicly.
- **P1 — functional/reliability gate:** resolve before calling the local application stable.
- **P2 — product polish/maintainability:** address before or alongside the final verification pass when the fix is low-risk.
- **P3 — cloud evolution:** do after local stabilization, as part of the hands-on AWS implementation.

## P0 — Security and data integrity

### STAB-001 — Enforce unique, normalized user email
**Source evidence:** the inspected EF Core model snapshot does not define a unique index on `Users.Email`; repository comparison does not make the email normalization/case policy explicit.

**Plan**
1. Decide the canonical policy (trim whitespace; compare case-insensitively; preserve a canonical display form if desired).
2. Inspect existing user rows for duplicates/collisions under that policy.
3. Normalize existing data safely.
4. Add a unique database index through a new EF Core migration.
5. Handle the database unique-constraint violation as a controlled conflict response, because application-level existence checks alone race under concurrent requests.

**Verification:** duplicate registrations with exact-case and different-case email; concurrent registration attempts; confirm only one user row is created.

### STAB-002 — Validate uploaded files on the server
**Source evidence:** the inspected upload path checks missing/empty files, but an explicit extension/content-type allowlist and maximum upload size were not found in the reviewed path.

**Plan**
1. Define allowed formats and maximum file size.
2. Validate extension and media type as hints, not as the only proof of file type.
3. Enforce request/body limits and reject unsupported files before writing to disk.
4. Generate safe storage names; do not trust client file paths.
5. Keep document ownership checks on preview, download, extraction, summary and delete paths.

**Verification:** empty, oversized, wrong-extension, wrong-content-type, malformed PDF and valid PDF cases.

### STAB-003 — Remove raw prompt/document text from cache keys and cache logs
**Source evidence:** the inspected AI service constructs a cache key using operation type plus full input text and emits cache-related logs containing that key. This may expose prompts or extracted document content in logs and creates large keys.

**Plan**
1. Create a stable digest of normalized operation input for the cache key.
2. Log operation, digest prefix, and job/document/user IDs where appropriate—not raw content.
3. Review request/exception logging to ensure full request bodies, tokens and credentials are not recorded.
4. Keep cache errors best-effort without hiding the primary AI result.

**Verification:** inspect logs for prompts, document text, tokens and raw cache keys; verify cache hit/miss still works.

## P1 — Functional and reliability gates

### STAB-004 — Add explicit server-side prompt validation
**Source evidence:** no validator was found at the expected GenerateText feature path, and the inspected frontend form has no clear required/minimum-length policy.

**Plan:** define empty/whitespace rules and practical input limits for text generation, plain-text summarization and document summarization; implement server-side validators and align the UI.

**Verification:** empty, whitespace-only, Unicode, normal, and oversized input for each relevant endpoint.

### STAB-005 — Make polling lifecycle-safe
**Source evidence:** the generation component uses a two-second interval, does not retain/cancel the polling subscription on destruction, and has no explicit polling error handler.

**Plan:** poll immediately after job creation, retain the subscription or use a lifecycle-aware RxJS pattern, stop on Completed/Failed, handle HTTP errors visibly, and prevent overlapping requests if response latency exceeds the interval.

**Verification:** navigate away during polling; force status endpoint failure; verify terminal statuses stop polling and results/errors render without refresh.

### STAB-006 — Propagate cancellation through synchronous AI workflows
**Source evidence:** the plain-text and document-summary handlers accept a CancellationToken but the inspected calls do not pass it to `IAIService.ProcessAsync`.

**Plan:** pass the request token through the Application interface, Infrastructure service and provider HTTP request where supported.

**Verification:** cancel a long-running local request and confirm provider work is cancelled or its remaining limitations are documented.

### STAB-007 — Replace first-100 document metadata lookup
**Source evidence:** the document-details page obtains a page of up to 100 documents and searches that collection for the selected ID.

**Plan:** expose/use an ID-specific, user-scoped endpoint or query returning only the requested document metadata; retain the ownership gate.

**Verification:** open a document whose record would not be in the first page; verify missing/foreign IDs return controlled responses.

### STAB-008 — Review document deletion consistency and relationships
**Source evidence:** deletion removes the physical file before database metadata; a later DB failure can leave metadata without a file. Related DocumentContent and AIJob cleanup/cascade behavior requires deliberate schema review.

**Plan:** document the desired retention policy; inspect all foreign keys/cascades; select an explicit compensation or orphan-reconciliation strategy; do not silently delete AI history if it should be retained.

**Verification:** controlled file and DB failures, plus expected related-row behavior.

### STAB-009 — Harden background worker recovery semantics
**Source evidence:** the worker uses a singleton in-memory queue and attempts startup recovery of Pending/Processing/Retrying jobs. Recovery mitigates crashes but does not guarantee exactly-once AI calls.

**Plan:** keep the current approach for local stabilization; document state transitions and crash windows; ensure cancellation/shutdown does not leave confusing state; consider idempotency or a durable queue only during the planned cloud stage.

**Verification:** restart with jobs in each non-terminal state; safely simulate provider failure; observe retry count, backoff, terminal failure, and duplicate-call possibility.

### STAB-010 — Bound pagination and propagate cancellation
**Source evidence:** the inspected job-history endpoint/query does not visibly bound pageSize, and some query interfaces do not accept CancellationToken.

**Plan:** define safe page/pageSize limits and propagate cancellation through handlers and EF queries.

**Verification:** invalid/oversized pagination inputs and cancelled requests.

### STAB-011 — Restrict detailed health diagnostics
**Source evidence:** the detailed health endpoint returns check statuses and exception messages. Current readiness registration checks PostgreSQL, not every dependency.

**Plan:** restrict detailed diagnostics to authorized/internal access or safe environment-specific exposure; decide whether Redis checks are needed; keep liveness distinct from readiness.

**Verification:** dependency-down scenarios and unauthenticated access to detailed diagnostics.

## P2 — Consistency, maintainability and UI polish

### STAB-012 — Remove legacy Project/ProjectId prototype concepts
**Source evidence:** `AIJob.ProjectId` remains in the entity, and the generation, plain-text-summary, and document-summary handlers assign random GUIDs. Historical EF migrations/model snapshots contain the field. `backend/Core/Entities/Project.cs` exists, but the directly inspected `AppDbContext` has no `DbSet<Project>` and no explicit Project mapping. This strongly suggests the entity is unused in the current model, but the full source tree and database schema must still be searched before removal.

**Plan**
1. Search all current source, tests, notes, migrations and database mappings for `Project`, `ProjectId`, and relationships.
2. Confirm no product workflow requires projects.
3. Remove live entity/property/assignments and any DbContext mapping.
4. Add a new migration that safely drops the obsolete column/table only after confirming existing schema and data.
5. Do not rewrite historical migrations that may already have been applied.
6. Update diagrams and this handbook after the change.

**Verification:** clean database migration from zero and upgrade from the existing migration state; generation, both summary workflows, history and document workflows still work.

### STAB-013 — Make document navigation state intentional
**Source evidence:** the list component constructs navigation state separately from the router.navigate call; that state object is not passed to navigate.

**Plan:** either pass the intended state correctly or remove the unused expression; make document details load by document ID regardless of navigation state.

### STAB-014 — Align file-size formatting
**Source evidence:** the list formatter uses two decimal places and includes a space between number and unit, while the desired UI style is compact (for example `300KB`, `1.2 MB`, `1GB`).

**Plan:** define one consistent format and test bytes/KB/MB/GB boundaries.

### STAB-015 — Confirm API contracts and immediate rendering
**Source evidence:** the source path for plain-text summary assigns response output to component state and uses the Markdown sanitization path. Earlier observations reported a summary sometimes appearing only after refresh; source inspection alone cannot establish the runtime cause.

**Plan:** during final testing capture HTTP status, actual response envelope, service mapping and component state before changing implementation.

### STAB-016 — Verify user-facing DTOs and logs
**Source evidence:** the inspected `UserDto` exposes only ID, email, full name and role; the `GetUsersQueryHandler` maps those fields explicitly and does not return PasswordHash. The Development settings file contains a development-only JWT secret and a placeholder database password; these must never be reused as production values or treated as secure credentials.

**Plan:** retain explicit DTO mapping; ensure logs avoid passwords, tokens, raw document text and sensitive request bodies; ensure production uses a non-Development environment and supplies strong JWT/database/Redis/provider settings through environment-specific secret injection.

### STAB-021 — Establish a repeatable automated test/build gate
**Current testing position (confirmed by the developer):** the application has been manually tested multiple times during implementation, but the developer has not written intentional automated test cases for either backend or frontend yet. Existing Angular `.spec.ts` files are generated/scaffold-level examples and must not be counted as meaningful feature coverage. One generated app spec expects `Hello, frontend`, so it may be stale; do not spend time fixing generated specs now unless needed to get a later automated test run working. GitHub code searches did not locate a dedicated backend test project or CI workflow; this remains a repository-tree verification item.

**Plan:** defer writing automated test cases until the developer chooses to start that phase. For now, preserve the manual test history as context, finish source/documentation audit, and prepare a clear manual verification checklist. Later, add a small but meaningful test foundation for registration/login, user ownership, AI job status transitions, document ownership, and API error contracts, plus repeatable build checks. The root README has now been expanded with local setup instructions.

**Plan:** inspect the full repository tree and existing frontend specs before creating anything. Identify whether backend tests and GitHub Actions workflows exist. If absent, add a minimal test foundation for registration/login, user ownership, AI job status transitions, document ownership, and API error contracts, plus repeatable backend/frontend build checks. Add a concise root README with prerequisites, local configuration, database migration, Redis, backend/frontend startup, and safe test commands. Keep tests aligned with the real architecture; do not create a large test framework before the first useful tests.

**Verification:** run tests and builds from a clean checkout; capture the actual commands and outcomes in the handbook tracker.

### STAB-022 — Review and restrict the test-only API controller
**Source evidence:** `backend/API/Controllers/TestController.cs` exposes `GET /api/test/error` without an authorization attribute and intentionally throws an exception. It also exposes authenticated and AdminOnly test routes. The controller appears intended for middleware/authentication testing, not as a product feature.

**Plan:** before any production deployment, remove the test-only controller or place it behind an explicit Development-only registration/route condition. Do not rely on obscurity. Preserve the middleware/authentication test scenarios in the later test plan instead of leaving a deliberate error endpoint in the production surface.

**Priority:** P1 for production readiness; no need to disrupt the current local development flow before the final verification phase.

### STAB-023 — Decide the role of the standalone Python AI engine
**Source evidence:** the repository contains a top-level `ai-engine/` directory with a FastAPI endpoint, a Pegasus XSum Transformers model loaded at process startup, and pinned Python dependencies. The inspected .NET API/provider path calls Gemini through the shared AI provider abstraction; no integration from the .NET API to this FastAPI service was found in the inspected controller/provider paths. The FastAPI app also configures wildcard CORS with credentials enabled and exposes its summarize route without authentication. Treat it as a separate prototype/research component unless further source tracing proves otherwise.

**Plan:** keep it in the repository for now because the project's long-term purpose includes learning and integrating multiple AI approaches. Decide later whether it should become a supported model-provider service or remain a research sandbox. Before exposing it beyond a trusted local environment, define its API contract, authentication/network boundary, restrictive CORS, request size/length limits, model resource requirements, health checks, logging and failure handling. Do not introduce another runtime into the current deployment merely because the directory exists.

## P3 — AWS transition (after local stabilization)

### STAB-017 — Replace local file storage with S3 behind the existing abstraction
Define private bucket access, object naming, metadata consistency, size/type limits, lifecycle/retention, and safe download/preview behavior.

### STAB-018 — Replace in-memory queue with SQS
Design visibility timeout, retries/dead-letter queue, idempotency, poison-message handling, and worker scaling. Do not claim exactly-once processing; design for at-least-once delivery.

### STAB-019 — Containerize API and worker; externalize configuration
Create repeatable Docker builds, inject secrets through an appropriate managed mechanism, and keep local development straightforward.

### STAB-020 — Select managed data/cache and observability services
Choose PostgreSQL hosting, Redis strategy, CloudWatch logging/metrics/alarms, least-privilege IAM, networking and deployment/rollback. Make decisions based on cost and actual workload rather than adding infrastructure by default.

## Consolidated final verification sequence

1. Restore/build the backend and build the frontend from a clean working tree.
2. Apply EF migrations to a disposable/test database; verify both clean-install and upgrade paths.
3. Test registration/login, JWT, normal-user access, admin access and cross-user isolation.
4. Test async generation, all job states, polling, retries and restart recovery.
5. Test plain-text summarization and immediate UI rendering.
6. Test upload, preview/download, extraction, saved summary retrieval, listing and deletion.
7. Test health endpoints, rate limits, correlation IDs, logging, cache behavior and controlled dependency failures.
8. Re-run the source-level security checklist and confirm no secrets or sensitive content are logged.
9. Record actual outcomes in the handbook tracker; do not mark any workflow verified from a successful build alone.

## Explicit non-goals for this stabilization pass

- Do not split the application into microservices just to make the diagram look more complex.
- Do not introduce Kubernetes, Kafka, or distributed infrastructure before the cloud design calls for it.
- Do not replace the current AI provider strategy until fallback requirements are defined.
- Do not refactor working paths merely for stylistic uniformity.
- Do not start AWS deployment before the local app has passed the agreed verification pass.
