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

### STAB-002 — Harden uploaded-file validation and handling
**Source evidence (corrected after tracing the MediatR validation pipeline):** the controller rejects null/empty files. `UploadDocumentCommandValidator` enforces a 10 MB maximum, allows `.pdf`, `.doc`, `.docx`, and `.txt`, and checks that declared content type matches the extension. The validator is registered through the Application assembly and runs via `ValidationBehavior<,>`. This is more than the controller alone shows, but it does not inspect file signatures or prove the content is safe/parseable.

**Plan**
1. Preserve the existing server-side limits and test boundary behavior.
2. Verify actual bytes/file signatures and parser behavior; safely reject malformed or mismatched files.
3. Confirm effective hosting/request-body limits are consistent with the 10 MB command limit.
4. Keep generated storage names and harden local storage path boundaries.
5. Keep document ownership checks on preview, download, extraction, summary and delete paths.

**Verification:** empty, boundary/oversized, wrong-extension, mismatched declared content type, spoofed signature, malformed PDF/DOCX, valid supported file, and two-user ownership cases.

### STAB-003 — Remove raw prompt/document text from cache keys and cache logs
**Source evidence:** the inspected AI service constructs a cache key using operation type plus full input text and emits cache-related logs containing that key. This may expose prompts or extracted document content in logs and creates large keys.

**Plan**
1. Create a stable digest of normalized operation input for the cache key.
2. Log operation, digest prefix, and job/document/user IDs where appropriate—not raw content.
3. Review request/exception logging to ensure full request bodies, tokens and credentials are not recorded.
4. Keep cache errors best-effort without hiding the primary AI result.

**Verification:** inspect logs for prompts, document text, tokens and raw cache keys; verify cache hit/miss still works.

## P1 — Functional and reliability gates

### STAB-004 — Complete and align server-side AI input validation
**Source evidence (corrected after tracing the validator assembly):** `GenerateTextCommandValidator` exists in `backend/Application/Features/AI/AIRequestValidator.cs` and enforces a non-empty prompt with a 5–1000 character range. `SummarizeCommandValidator` exists and checks non-empty/minimum length, but has no explicit maximum. No `SummarizeDocumentCommandValidator` was found in the inspected source. The validators are registered from the Application assembly and invoked by the MediatR validation behavior. Earlier wording implying there was no GenerateText validator was incorrect because the file name differs from the feature folder's expected convention.

**Plan:** preserve and test the existing generation validator; add explicit maximum size/token limits for plain-text and document summarization; ensure whitespace handling is intentional; align Angular form feedback with server-side rules.

**Verification:** exercise empty, whitespace-only, Unicode, normal, boundary-length and oversized inputs for each endpoint through the API; confirm invalid requests do not call the provider or create unintended jobs.

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
**Current testing position (confirmed by the developer):** the application has been manually tested multiple times during implementation, but no intentional automated feature test cases have been written for the backend or frontend yet. Existing Angular `.spec.ts` files are generated/scaffold-level examples and must not be counted as meaningful feature coverage. One generated app spec expects `Hello, frontend`, so it may be stale; do not spend time fixing generated specs now unless needed to get a later test run working.

**Current CI position:** a GitHub Actions workflow now checks architecture-documentation drift, but it does not build or test the application. No dedicated backend test project was found in the repository tree inspected so far.

**Plan:** defer writing automated test cases until the developer chooses to start that phase. For now, preserve the existing manual-test history as context, finish source/documentation audit, and prepare a clear manual verification checklist. Later, add meaningful coverage for registration/login, user ownership, AI job status transitions, document ownership, and API error contracts, plus repeatable backend/frontend build checks.

**Verification:** when the testing phase begins, run tests and builds from a clean checkout and record the actual commands and outcomes in the handbook tracker.

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

## Additional source-trace notes — document and data lifecycle

These are source-level observations from the current repository and are not yet runtime-confirmed defects.

- **Document ownership boundary:** `DocumentRepository.GetByIdAsync` filters by both document ID and current user ID, and the delete handler uses that method before deleting. Keep this ownership check at every document-specific API/handler boundary.
- **Extracted-content repository:** `DocumentContentRepository.GetByDocumentIdAsync` filters only by document ID. The extraction workflow must first prove that the document belongs to the authenticated user; avoid exposing this repository method directly through an endpoint without that ownership gate.
- **Document/content relationship:** `AppDbContext` configures a one-to-one relationship from `DocumentContent` to `Document` with cascade delete. Confirm the generated migration and live database constraint match this model before relying on cascade behavior.
- **AI job relationship:** `AIJob` carries `UserId`, nullable `DocumentId`, and legacy non-nullable `ProjectId`. The reviewed `AppDbContext` does not explicitly configure an AIJob-to-Document relationship. Inspect all migrations, model snapshots, and the actual database schema before changing these fields or assuming document deletion will remove associated AI jobs.
- **File/database consistency:** upload writes the file before inserting metadata and attempts file cleanup if metadata persistence fails. Delete removes the physical file before deleting metadata. Both directions have crash/failure windows because filesystem operations and database transactions are not atomic together. A later hardening decision should favor recoverable cleanup (for example, staged deletion or reconciliation) rather than claiming atomicity.
- **Storage-path safety:** the current local storage implementation combines a fixed storage root with a stored filename/path. Current uploads generate a GUID-based stored filename, but callers and persisted metadata should not be able to supply arbitrary paths. Preserve the storage abstraction when moving to S3 and validate object keys at the boundary.
- **Retention and privacy:** deleting a document currently does not, by itself, establish a documented retention policy for linked AI job prompts/results or logs. Decide whether summaries/results should be deleted, retained, or anonymized, and document the policy before production exposure.
- **No schema changes made:** these observations are documentation only. Do not remove `ProjectId`, add/drop foreign keys, or rewrite historical migrations until the current schema and all references have been audited and a forward migration has been planned.

## Additional source-trace notes — API boundary and operations

These observations are based on the current controller/host source and still require environment-aware verification.

- **Test controller exposure:** `TestController` includes `GET /api/test/error` without an authorization attribute, deliberately throws an exception, and exposes separate authenticated/admin diagnostic routes. Restrict the controller to Development or remove it from production routing before release. This is already tracked by STAB-022.
- **Health endpoint distinction:** `/health/live` runs only the in-process Self check; `/health/ready` checks PostgreSQL. Neither currently proves Redis or the configured AI provider is available. `/health/details` returns check names and exception messages, so keep it private or restrict it before production exposure.
- **Rate limiting:** `Program.cs` applies the named concurrency limiter to mapped controllers; the fixed-window limiter is registered but must be checked for actual endpoint policy attachment. Do not assume the fixed-window limit is active just because it is registered.
- **Request validation:** API behavior suppresses the automatic model-state invalid filter, so validators and exception mapping must be traced for each request model. For document upload, the controller rejects null/empty files and the MediatR command validator enforces a 10 MB maximum, an extension allowlist (`.pdf`, `.doc`, `.docx`, `.txt`), and matching declared content type. These metadata checks do not validate the actual file bytes or guarantee the file is safe/parseable; test malformed files and effective hosting request limits.
- **Logging discipline:** the admin user controller logs operation milestones and a newly created user ID, not the password. Continue reviewing handlers and AI/cache logging for raw prompt/document text and avoid logging tokens, credentials, or extracted document content.
- **Controller access boundary:** `DocumentsController` has class-level `[Authorize]`; actual per-document ownership still depends on each query/command handler, so verify all file/content/extraction/delete paths individually rather than treating authentication alone as sufficient.
- **No code or deployment changes:** this pass records source-level concerns only; no application code was changed and no endpoint has been runtime-tested during this audit.

## Additional source-trace notes — AI request and cache path

These findings are from source inspection; runtime behavior and the full worker implementation still need a separate verification pass.

- **Cache keys include raw input:** `AiService` constructs `ai:{JobType}:{Input}` and logs the key on cache get/set failures, hits and misses. This makes prompt or extracted document text part of the cache key and potentially log output. Replace this with a stable digest of normalized operation/input plus any model/configuration dimensions that affect results; log only the opaque key or operation metadata. This supports STAB-003.
- **Cancellation support is uneven:** `AiService.ProcessAsync` accepts and passes a cancellation token to the provider, but the plain-text summary and document-summary handlers must be checked for whether they actually pass their token to `ProcessAsync`. The generate controller/handler path shown here does not explicitly forward request cancellation to MediatR/queue persistence. Verify the complete path before changing signatures.
- **Generate request validation:** `AIController.Generate` accepts `GenerateTextRequest` and copies `Prompt` into a command. Validation is handled downstream by `GenerateTextCommandValidator` in `AIRequestValidator.cs`, registered via the Application assembly and MediatR validation behavior; the controller itself does not perform those checks. Test the full API path to ensure invalid input is rejected before enqueue/provider execution.
- **Rate limit coverage:** `POST /api/AI/generate` explicitly uses the fixed-window policy. Other AI actions do not have that attribute in this controller, although the host applies a concurrency limiter to mapped controllers. Confirm whether that overall concurrency cap is intended and whether costly synchronous summary routes need a distinct policy.
- **Job authorization:** the status, history and statistics actions require authentication; verify their query handlers and store queries always scope data to the current user, not just the supplied job ID.
- **Cache semantics:** successful provider outputs are cached for ten minutes. Confirm that cache keys account for model/provider/config changes when necessary and that no cross-user data or sensitive document content can be recovered through a shared-key design.
- **No AI code changed:** this is an audit record only. The AI worker source path was not resolved in this pass, so do not treat queue recovery/retry behavior as fully revalidated here.

## Additional source-trace notes — frontend contracts and job queries

These are source-review observations. The relevant Angular component path and infrastructure job-store implementation were not located under the initially guessed paths, so those parts remain unverified.

- **API response envelopes:** Angular `AiService` declares `ApiResponse<T>` wrappers for generation, status, history, summaries and stats, matching the controller's `ApiResponse<T>.SuccessResponse(...)` pattern in the inspected actions. During final verification, capture the actual network payload and component assignment to diagnose the previously reported summary-rendering-after-refresh issue; do not assume the wrapper or Markdown renderer is the cause without observing the value at each boundary.
- **Job status is user-scoped at the handler boundary:** `GetAIStatusQueryHandler` passes both the requested job ID and `ICurrentUserService.UserId` to `IAIJobStore.GetJobAsync`. Confirm the implementation applies both filters.
- **Job history and stats are also requested for the current user:** the handlers pass the current user ID into the store. Verify store queries preserve that filter and ensure paging values are bounded at the API/validation/store boundary.
- **Job history exposes prompts:** `GetAIJobsQueryHandler` returns each job's prompt but omits its result/error. Review whether the history UI needs the complete prompt or whether a preview/redaction policy is more appropriate for long or sensitive document-derived input.
- **Cancellation is not consistently propagated:** the handlers receive a MediatR cancellation token but the calls to `IAIJobStore` shown here do not pass it. This can allow database work to continue after the request is cancelled; confirm interface signatures and add cancellation where supported during the implementation phase.
- **Frontend service skips the global loading indicator for polling and document-summary operations:** `AiService` sets `SKIP_LOADING` for status polling, document summarization, and saved-summary retrieval. This appears intentional to avoid overlay flicker, but should be confirmed against the intended UX during manual verification.
- **No frontend behavior changes made:** the actual GenerateComponent source path was not resolved in this pass. Polling cleanup, error handling, immediate rendering, and component lifecycle remain open verification items rather than confirmed defects.

## Additional source-trace notes — background worker and job persistence

The worker, in-memory queue, and job-store implementations have now been inspected directly.

- **The queue is process-local memory:** `AIQueue` uses `ConcurrentQueue<AIJob>` with no persistence or backpressure. A process crash can discard queued objects, so database-based recovery is essential. Before cloud deployment, replace this with the agreed durable queue when moving to a separate worker or multiple API instances.
- **Startup recovery is database-driven:** `AIWorker` queries Pending, Processing, and Retrying jobs at startup. Processing jobs are reset to Pending; jobs at or above their retry allowance are marked Failed. This is useful crash recovery, but not a guarantee of exactly-once execution.
- **Duplicate provider work remains possible:** the worker calls the provider before saving Completed. If the provider succeeds but the process dies before the result is persisted, recovery can call the provider again. Decide whether this cost/duplication is acceptable and consider idempotency/provider request tracking before introducing multiple workers.
- **Retry count semantics:** the worker increments RetryCount after a failure and re-enqueues only while RetryCount is less than MaxRetries. With the default MaxRetries of 3, this means up to three processing attempts total, not three retries after an initial attempt. Confirm that the UI/docs use this meaning consistently.
- **Shutdown states rely on next-start recovery:** cancellation during active processing exits the loop without persisting a new state; cancellation during retry delay leaves the job as Retrying. On the next startup these states are selected for recovery. Test graceful shutdown and abrupt process termination separately.
- **A retry delay blocks this worker's single processing loop:** while waiting between retries, the same worker cannot process another queued job. This is a throughput limitation to document; avoid adding concurrency until job-store scoping, provider limits and duplicate-work behavior are understood.
- **Store calls lack visible cancellation parameters:** `AIJobStore` calls EF Core operations without forwarding request/worker cancellation tokens. Consider adding tokens consistently to persistence interfaces and implementations.
- **History paging is unbounded in the store:** `GetJobsAsync` directly uses `Skip((page - 1) * pageSize).Take(pageSize)` and does not clamp or reject invalid values. Add explicit validation/bounds at the request boundary and tests for zero, negative, and very large page sizes.
- **Job-store ownership is implemented for user-facing reads:** `GetJobAsync`, `GetJobsAsync`, `GetStatsAsync`, and latest-summary lookup filter by user ID where relevant. `GetRecoverableJobsAsync` intentionally returns recoverable jobs across all users for the background worker.
- **No worker/queue code changed:** these are source-trace findings only. Worker failure, restart, retry, and shutdown scenarios remain for the final verification pass.

## Additional source-trace notes — document upload, retrieval, and deletion

The current document controller, handlers, repository, and local storage implementation were inspected directly.

- **Authentication is present at the controller boundary:** `DocumentsController` has class-level `[Authorize]`. Upload, content retrieval, extraction, download and delete still depend on their handlers/queries enforcing per-document ownership. Extraction and deletion explicitly use the user-scoped repository lookup; verify the `GetDocument` and `GetDocumentContent` query handlers similarly.
- **Upload validation is present in the command pipeline, not fully visible in the controller:** the controller rejects missing/empty files; `UploadDocumentCommandValidator` enforces a 10 MB limit, `.pdf`/`.doc`/`.docx`/`.txt` extensions, and a declared content-type/extension match. Original filename and content type are metadata, not proof of actual bytes. Remaining work is to test limits, validate file signatures/parser behavior and harden storage-path boundaries.
- **Storage paths are generated but not independently constrained on read/delete:** `LocalDocumentStorage` combines its storage root with the supplied path. The upload handler currently stores the generated filename as `StoragePath`, which reduces immediate exposure, but storage operations should still reject rooted paths, separators, and traversal segments so a corrupt or manipulated database value cannot target files outside the storage directory. S3 migration should use validated object keys, not filesystem paths.
- **Upload and delete are not atomic across filesystem and PostgreSQL:** upload writes the file before saving metadata and attempts cleanup on metadata failure. Delete removes the file before deleting metadata. A crash or second-operation failure can leave orphaned files or metadata pointing to a missing file. Plan a reconciliation/cleanup strategy; avoid a larger storage abstraction rewrite until the recovery behavior is specified.
- **Pagination needs bounds here too:** `DocumentRepository.GetPagedAsync` directly applies `Skip((page - 1) * pageSize).Take(pageSize)`. Validate page >= 1 and cap page size at the query boundary.
- **Cancellation is propagated on document paths:** the controller passes its token to MediatR, and the upload/extract/delete handlers pass it into storage and repository operations where shown. Preserve this as other service paths are standardized.
- **Content extraction verifies ownership before document-content lookup:** the extraction handler first retrieves the document by both ID and current user ID, then looks up extracted content by document ID. This is the correct ownership-check ordering; retain it if repository interfaces are refactored.
- **Deletion semantics still need a data-retention decision:** the document row's deletion may cascade to its content according to EF model configuration, but linked AI jobs/prompts/results and log records need an explicit retention policy. Confirm actual migration/live schema and FK behavior before making schema changes.
- **No document implementation changed:** these are source-trace findings. File-size limits, invalid MIME/extension cases, traversal defense, failed-write cleanup, cross-user access, and delete failure windows remain verification items.

## Additional source-trace notes — document reads, authentication, and diagnostics

- **Document download and extracted-content retrieval are user-scoped:** both `GetDocumentQueryHandler` and `GetDocumentContentQueryHandler` call `IDocumentRepository.GetByIdAsync(id, currentUserId, cancellationToken)` before opening the stored file or reading extracted text. This closes the ownership question for these two paths at source level. Still verify with two-user tests.
- **Login/register cancellation is not forwarded from the controller:** `AuthController.Login` and `Register` call `_mediator.Send(command)` without the request cancellation token, despite MediatR handlers supporting it. Pass the controller token through as a consistency improvement.
- **Authentication logging is currently low-detail but noisy:** login logs endpoint hit, success, and handler milestones. It does not visibly log the submitted password, which is good. Keep that invariant and consider reducing routine per-step logs while retaining structured security-relevant events and correlation IDs.
- **Email matching is exact in the repository:** duplicate detection and login lookup use `u.Email == email` without an explicit normalization/case policy. Define canonicalization (e.g., trim and case-normalize consistently) and enforce uniqueness at the database level; app-level pre-checks alone can race. Confirm provider/database collation behavior rather than assuming case sensitivity.
- **The test controller is routed without a class-level authorization requirement:** `GET /api/test/error` intentionally throws a test exception and has no `[Authorize]`; other test routes require authenticated/admin claims. Restrict this controller to Development or remove it from production routing.
- **Health detail endpoint exposes exception messages:** `/health/details` returns health-check names, statuses, and exception message text. It currently checks registered checks only, including PostgreSQL and Self; do not publish detailed diagnostics publicly. `/health/live` is process liveness and `/health/ready` is PostgreSQL readiness, not proof that Redis or the AI provider is available.
- **No code changed:** document-read ownership is confirmed by source inspection, while request cancellation, email normalization/unique constraint, diagnostic endpoint exposure, and environment-aware behavior remain backlog work and verification items.

## Execution gates — recommended implementation order and exit criteria

This order resolves dependencies between backlog items. Do not start the next gate until the current gate's exit criteria are met and recorded. These are planned acceptance criteria, not claims that the work has already passed.

### Gate A — Secure the public API and data boundary (P0)

Implement STAB-001, STAB-002, STAB-003, STAB-011, STAB-022, plus the Python-engine decision in STAB-023 if that service is intended to run outside a trusted local environment.

**Exit criteria:** email normalization is consistent across registration/login and a database unique constraint prevents duplicate accounts under concurrent requests; invalid/oversized uploads are rejected server-side; storage paths cannot escape the configured root; cache keys/logs contain no raw prompts or extracted text; detailed health output and test-only routes are not publicly exposed; any externally reachable AI engine has an explicit authentication/network boundary and restrictive CORS.

### Gate B — Make core workflows predictable (P1)

Implement STAB-004 through STAB-010 and STAB-015 where needed to close request validation, polling, cancellation, document navigation, data deletion, job recovery and paging gaps.

**Exit criteria:** invalid prompts and paging values return defined client errors; polling stops on completion, failure, navigation and component destruction; user-scoped job/document reads are verified; document upload/delete failure windows have a documented recovery behavior; retries and restarts produce known job states; generated summaries appear without requiring a page refresh.

### Gate C — Remove prototype leftovers and confirm contracts (P2)

Implement STAB-012 through STAB-016 and STAB-021. Before removing Project/ProjectId, search the entire repository, inspect every historical migration and the current model snapshot, and inspect the actual disposable/test database schema. Never rewrite applied historical migrations; use a new migration if schema changes are justified.

**Exit criteria:** each remaining domain field has a clear purpose; document navigation and file-size display match the intended UX; API envelopes and DTOs are documented and confirmed against real responses; safe logging is reviewed; automated backend/frontend tests and build checks run reproducibly in CI.

### Gate D — Final local verification and evidence

Run the consolidated verification sequence below on a clean working tree and disposable/test database. Record exact commands, environment prerequisites, pass/fail results and any remaining limitations in the handbook. A successful build is necessary but not sufficient; include authorization, restart/failure and document lifecycle cases.

**Exit criteria:** all P0 items are closed; P1 workflow tests pass or remaining exceptions are explicitly accepted with rationale; critical workflows have repeatable automated coverage; the handbook accurately reflects tested behavior; diagram/documentation checks pass on the final commit.

### Gate E — AWS deployment (only after Gates A–D)

Then execute STAB-017 through STAB-020 in small increments: S3 behind the storage abstraction, SQS for durable work dispatch, API/worker containerization and externalized secrets/configuration, then managed database/cache and observability choices. Apply least-privilege IAM, private networking where appropriate, health/readiness checks, logging/metrics and cost controls as part of each step.

**Exit criteria:** deployed workflows pass the same acceptance tests as local; storage and queue failure behavior is understood; no secrets are embedded in source/images; monitoring can distinguish API liveness, database readiness and provider/queue failures; deployment and rollback steps are documented.

## Additional source-trace notes — EF model and host pipeline reconciliation

- **The current EF model snapshot confirms no unique index on `Users.Email`:** `Users` has only its primary key in the snapshot. The `DocumentContent.DocumentId` unique index and cascade-delete FK are represented. Do not assume a live database exactly matches the snapshot until migrations and the target schema are inspected.
- **AIJob relationship integrity is not configured in the current model:** `AIJob` has `UserId`, nullable `DocumentId`, and required legacy `ProjectId`, but the inspected `AppDbContext` only configures job enum conversions and the `(UserId, CreatedAt)` index. The snapshot likewise has no explicit FK from AIJobs to Users or Documents. Decide whether these are intentional scalar references or should be real foreign keys; validate existing orphaned rows before any migration.
- **Document ownership is application-enforced, not database-enforced:** the `Documents.UserId` index exists, but no explicit FK to Users is configured in the inspected model. If adding constraints, plan the migration and orphan cleanup rather than assuming application filters alone protect referential integrity.
- **Rate limiting is layered:** `Program.cs` applies the named `concurrency` limiter (permit limit 2, queue 0) to all mapped controllers. The named fixed-window limiter (5 requests per 10 seconds) is registered but only protects actions that explicitly attach that policy. Confirm this matches the intended throughput and response behavior under load.
- **Middleware order is deliberate and should be verified in integration tests:** exception handling, correlation ID, and request logging are installed before CORS, authentication, authorization, rate limiting, and controller mapping. Ensure rejected requests and exceptions still return the intended envelope and correlation ID, and that logs don't include sensitive request bodies.
- **CORS origins are configuration-driven:** if `Cors:AllowedOrigins` is absent, the policy receives an empty origin list. This fails closed for browser origins, but deployment configuration must supply the actual frontend origin; do not use wildcard origins as a quick fix.
- **Redis is required at startup but not part of the readiness checks:** the host throws if the Redis connection string is missing, while readiness checks only PostgreSQL. A present connection string does not guarantee Redis is reachable, so decide whether Redis should be included in readiness or exposed as a separately protected dependency check.
- **No model or host code changed:** these are source-level model/pipeline findings. Before any schema change, inspect all migrations and the actual database schema, and run migration tests on a disposable database.

## Additional source-trace notes — migration history and ownership backfill

- **The ownership migration contains a hard-coded account assumption:** `20260828212727_AddOwnership.Up` assigns every existing document and AI job with a null `UserId` to the first user whose normalized email equals `admin@smartAIdashboard.com`, then changes `UserId` to non-nullable. If that account is absent, the backfill can leave null values and the migration can fail when altering the column; if it exists, all legacy records are attributed to that one account regardless of their actual historical owner. This is a data-provenance risk, not merely a schema concern. Before reusing this migration on any database, inspect its applied status and existing rows; never edit an already-applied migration. If an un-applied migration needs correction, create a safe forward plan and choose explicit ownership handling (manual mapping, quarantine, or intentional assignment) rather than silently assuming one admin owns all records.
- **The same migration adds `AIJobs.DocumentId` and `UserId` but no corresponding foreign-key constraints:** it creates indexes for `Documents.UserId` and `(AIJobs.UserId, AIJobs.CreatedAt)` only. Current `AppDbContext`/snapshot also do not model AIJob-to-User, AIJob-to-Document, or Document-to-User foreign keys. Decide whether to add constraints after identifying orphaned rows and intended deletion/retention behavior.
- **Historical schema evolution is incremental:** initial migrations create `Users`, then `AIJobs`, then `Documents`; a later migration adds `DocumentContents` with a unique `DocumentId` FK to `Documents` and cascade deletion. The checked-in snapshot contains `UserId` and `DocumentId` columns for current entities but no Users.Email unique index. This is source-history evidence only; applied migration state and live schema remain unverified in this pass.
- **Migration maintenance notes are ambiguous/stale:** `backend/Infrastructure/Migrations/README.md` only says “Temporary migration note.” and adjacent notes instruct regenerating an ownership migration. Clarify which migration is authoritative and how to safely validate it. The source inventory references `20260828210000_AddOwnership.Designer.cs`, but the corresponding `.cs` path was not found in this fetch; reconcile the inventory and migration directory before relying on that filename.
- **No migrations or data were changed:** the next safe step is read-only inspection of the actual migration history and row counts in a disposable/local database, followed by a documented migration correction plan. Do not drop `ProjectId` or add constraints until data implications and migration status are known.

## Audit correction — upload validation is present

A follow-up source search found `backend/Application/Features/Documents/Commands/UploadDocument/UploadDocumentCommandValidator.cs`, which is registered through `AddValidatorsFromAssembly(...)` and executed by `ValidationBehavior<,>`. The earlier note that upload validation was not visible must therefore be narrowed: the controller itself only checks for a missing/empty file, **but the command pipeline also validates** a 10 MB maximum, a filename extension allowlist (`pdf`, `doc`, `docx`, `txt`), and a matching declared content type.

Remaining upload risks are narrower: extension and `IFormFile.ContentType` are client-controlled metadata and do not prove file contents match the claimed type; no magic-byte/content parser validation was found in this validator. Verify request-size limits at the hosting boundary and test malformed/mismatched files. Do not treat the earlier broad statement as proof that no size/type validation exists.

## Additional source-trace notes — cache and worker reliability

- `AiService` constructs Redis cache keys as `ai:{JobType}:{Input}` and logs the full key on GET/SET failures, hits and misses. Because `Input` may contain prompts or extracted document text, logs can disclose user content. Replace raw input in keys/logs with a stable digest and define whether cache sharing across users is acceptable.
- Successful provider outputs are cached for ten minutes. The key includes job type and input but not user, provider, or model version. Review privacy/isolation and invalidation semantics before production.
- `GeminiProvider` uses named `AIClient` with retry/circuit-breaker/60-second timeout policies, logs status codes, and rethrows failures. It does not implement automatic OpenAI fallback despite the presence of `OpenAIProvider`; don't describe fallback as active.
- `AIQueue` is process-local memory; `AIWorker` recovers Pending/Processing/Retrying records at startup and processes one job at a time. A crash after provider success but before result persistence can repeat paid provider work. Retry delay blocks the single worker loop; `MaxRetries = 3` is used as a total-attempt ceiling by the worker.
- `AIJobStore` filters user-facing get/list/stats/latest-summary queries by `UserId`; recovery intentionally scans recoverable jobs across users. Paging currently accepts page/pageSize without repository-level bounds, and store methods do not accept cancellation tokens.
- `DocumentRepository.GetPagedAsync` scopes records to the current user and orders newest first, but page/pageSize are not bounded in the repository. Document file retrieval and extracted-content retrieval first verify ownership through `GetByIdAsync(id, userId)`.
- `TestController` exposes an unauthenticated deliberate error endpoint, plus authenticated/admin diagnostic endpoints. Remove or restrict test routes before production deployment.
- These are source-only findings; no runtime behavior, database state, or current CI status was verified in this pass.

## Additional source-trace notes — AI validation and frontend result lifecycle

- **Generate validation exists:** `backend/Application/Features/AI/AIRequestValidator.cs` defines `GenerateTextCommandValidator` and enforces a prompt length of 5–1000 characters. The filename is less conventional than a feature-local validator but assembly scanning registers it. The Angular Generate form currently creates a plain `FormControl` without visible required/min/max validators; align UI feedback with server rules while keeping server validation authoritative.
- **Plain-text summarization validation is partial:** `SummarizeCommandValidator` rejects empty text and enforces a minimum of five characters, but no maximum length is defined. The Angular summarize form likewise has no visible validators. Define a safe input limit that matches provider/request constraints and test it end-to-end.
- **Document summarization has no validator found in the inspected feature path:** it takes extracted text from storage, so the original upload size limit indirectly bounds input, but extracted content can still be large. Define explicit AI input/token/character limits and expected behavior for scanned/empty/oversized documents.
- **Request cancellation isn't propagated into synchronous summaries:** both `SummarizeCommandHandler` and `SummarizeDocumentCommandHandler` receive a cancellation token but call `IAIService.ProcessAsync` without it. Their AIJobStore calls also omit cancellation tokens because the interface currently lacks them. Forward cancellation consistently through provider and persistence operations.
- **Generate polling has lifecycle gaps:** `GenerateComponent` starts polling using `interval(2000)`, so the first status check waits two seconds. Its subscription isn't retained or cancelled on component destruction; there is no `error` callback for polling failures, and the job ID is read from mutable component state. Use an immediate first poll, explicit terminal/error handling, and teardown-aware RxJS (`takeUntilDestroyed` or equivalent), with a stable job ID captured per request.
- **Document summary immediate rendering is implemented in source:** `DocumentDetailsComponent.generateSummary` reads `response.data.output`, awaits Markdown parsing, sanitizes the result, assigns `summaryHtml`, and triggers change detection; it also preserves plain generated text if rendering fails. This source path looks intentional, but the previously reported display-until-refresh issue is not considered resolved until reproduced and verified in the running app.
- **Document-details lookup uses a capped list rather than a direct document-by-ID query:** it fetches the first 100 documents and searches for the route ID. Documents beyond that page may incorrectly appear missing. Prefer a user-scoped document-by-ID metadata endpoint/query.
- **Result rendering sanitizes HTML:** the shared `ResultViewerComponent` uses `[innerHTML]`; inspected Generate/Summarize components render through `MarkdownService`, and the document-details path uses DOMPurify. Verify sanitizer configuration and security tests rather than relying on visual inspection alone.
- **No runtime tests were performed for these observations.** These notes are source-trace findings and do not establish that the reported UI issue is fixed.

## Additional source-trace notes — API contracts and authentication configuration

- **API response envelope is structurally aligned:** backend `ApiResponse<T>` serializes `Success`, `Message`, `Data`, and `Errors`; Angular's `ApiResponse<T>` model and error interceptor account for both camelCase and PascalCase names. This supports the intended contract but does not replace checking actual serialized payloads and HTTP status codes.
- **JWT settings have no startup validation visible in the inspected code:** `AddAuth` binds `JwtSettings` and dereferences `jwtSettings!.Issuer` / `Secret`; `JwtTokenService` uses the secret directly and expiry directly. Add fail-fast validation for non-empty issuer/audience, sufficient high-entropy secret, and sensible positive expiry. `appsettings.json` contains blank secret/connection strings/API key, which avoids committing credentials; `appsettings.Development.json` contains a clearly development-only signing secret and a `CHANGE_ME` database password placeholder. Never reuse the development secret in production; inject real values through local user secrets/environment and later managed cloud secrets.
- **JWT bearer option `RequireHttpsMetadata = false` is set explicitly.** This setting primarily affects metadata retrieval for authority-based configurations and this code uses local symmetric signing-key validation, so it should not be treated alone as proof that API requests can use HTTP. Nevertheless, production deployment must enforce HTTPS at the host/proxy and validate deployment configuration.
- **Request cancellation is inconsistent at controller and handler boundaries:** `AIController.Generate`, `GetStatus`, `GetJobs`, `Summarize`, and `GetStats` call MediatR without request cancellation; the relevant command/query handlers and job-store calls also omit tokens. Document-summary endpoints pass cancellation, but synchronous summarize handlers do not forward the token into `IAIService.ProcessAsync`. Include a consistent end-to-end cancellation pass.
- **Potential exception mapping mismatch:** `CurrentUserService.UserId` throws `UnauthorizedAccessException` if the authenticated principal lacks a usable user ID. The global exception middleware maps only the application's custom `UnauthorizedException` to 401; other exceptions become 500. Consider a deliberate, tested mapping for this exceptional principal state rather than exposing it as an internal error.
- **Success and error envelopes look consistent by source inspection:** the middleware emits `ApiResponse<T>` failures and the Angular interceptor reads both casing variants for message/errors. Runtime checks must still confirm correlation IDs, status codes, validation errors and no sensitive detail leakage.
- **No application code or runtime configuration was changed in this pass.** These findings are source-review observations; live secrets, deployed TLS, and API responses were not tested.

## Additional source-trace notes — test readiness, logging and dependency boundaries

- **Angular specs are mostly scaffolding, not feature tests:** `ai.service.spec.ts` only checks service construction; `generate.component.spec.ts` only checks component construction; `app.component.spec.ts` still asserts the starter text `Hello, frontend`. `package.json` exposes `npm test` and `npm run build`, with Vitest in dev dependencies. The starter assertion is stale against the current product UI and may fail. Do not count spec-file presence as feature coverage; first get the current test runner/build baseline, then add focused behavior tests (HTTP contracts, auth guards/interceptors, polling terminal/error states, document summary rendering).
- **No backend test project found in the inspected repository inventory:** add a dedicated test project later, with unit tests for validators/handlers and integration tests for auth, user/document/job isolation, exception envelopes, migrations and provider/queue failure paths. The repository docs already state this gap; the finding is reconfirmed by source search, not a claim that every possible branch was exhaustively checked.
- **Correlation IDs are enriched into Serilog events, but request log messages do not explicitly carry the ID as a template property:** `CorrelationIdMiddleware` pushes `CorrelationId` into `LogContext`, and `Program.cs` configures `Enrich.FromLogContext()` plus a console output template that displays it. This means the earlier concern that the console pipeline necessarily loses correlation IDs was too broad. Verify that both console and rolling JSON sinks retain the ID and that exception logs can be correlated. The incoming `X-Correlation-ID` is accepted as-is; consider length/format limits and response-header safety for caller-supplied IDs.
- **Sensitive data review remains necessary:** request logging currently logs method/path but not bodies in the inspected middleware, which is preferable. However, exception logs include exception objects and AI cache/provider code has separate prompt/key logging risks already captured above. Inspect all log templates and HTTP diagnostics before production; never log tokens, API keys, passwords or raw document text.
- **The standalone Python AI engine is not deployment-ready:** `ai-engine/app.py` permits wildcard origins with credentials, exposes `/summarize` without authentication, and accepts caller-provided `max_length` / `min_length` without visible explicit bounds in the route. Keep it local/research-only until auth/network restrictions, CORS policy, request and parameter limits, resource controls and failure behavior are designed. This is separate from the current .NET Gemini path.
- **Dependency target/version alignment needs a deliberate compatibility review:** the API targets .NET 8 while its project references include `AspNetCore.HealthChecks.NpgSql` 9.0.0, `Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.5 and `Serilog.AspNetCore` 10.0.0 alongside several 8.0.4 packages. This may or may not be supported by the full transitive graph; don't infer failure from version numbers alone. Run restore/build, inspect NuGet compatibility warnings and lock down a coherent supported set before deployment.
- **No Dockerfile or compose manifest surfaced in the repository search.** This is not a blocker for the current local stabilization phase; containerize only after local build/test/runtime baseline is reliable and the deployment architecture is chosen. No application code or tests were changed in this audit pass.

## Additional source-trace notes — frontend authentication and implementation plan

- **Frontend route guards check token presence, not token validity or expiry:** `TokenService.isLoggedIn()` returns whether `localStorage` contains a token. Both `authGuard` and `guestGuard` rely on that check and navigate imperatively before returning `false`. An expired/malformed token can therefore appear logged-in to the UI until the API rejects a request. Consider expiry-aware state and a centralized 401/session-expiry flow; backend authorization remains the security boundary.
- **JWT is stored in localStorage and attached as a bearer token by the HTTP interceptor.** This is a common SPA pattern but means successful script injection can expose the token. Before production, review XSS prevention, strict HTML/Markdown sanitization, CSP and token/session lifetime. Do not migrate to cookies without deliberately designing CSRF and cookie settings.
- **AuthService assumes successful login response contains `response.data.token`.** Verify casing and response-envelope behavior against the actual serializer and test success/error responses. Registration does not automatically save a token in the inspected service path; confirm the UI's intended post-registration behavior.
- **Login handler avoids logging submitted credentials:** it logs general progress and whether a user was found or password verification failed, without logging the email/password value in the inspected templates. Keep this property during observability improvements.
- **Implementation sprint document is stale/underspecified:** `docs/IMPLEMENTATION_SPRINT.md` references a historical starting commit and broad cloud tasks, but it does not reflect the current agreed gate of source audit → stabilization/security fixes → meaningful tests → full local verification → AWS. Reconcile it with the prioritized backlog before treating it as the active plan.
- **The handbook's authentication narrative should distinguish UI route gating from backend security:** guards are navigation UX, not an authorization mechanism. API endpoint policies and ownership checks must be tested independently.
- **No app code was changed and no runtime auth test was performed.** These findings are from source inspection only.

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
