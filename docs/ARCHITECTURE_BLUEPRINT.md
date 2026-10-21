# Smart AI Dashboard — Architecture Blueprint

> **Living source of truth.** Start here before changing architecture, implementing a feature, or beginning cloud deployment. Update this document whenever implementation changes. Diagrams describe the repository at the baseline commit listed below; re-check source before treating them as current after code changes.

- Repository: [Aminafia/Smart-AI-Dashboard](https://github.com/Aminafia/Smart-AI-Dashboard)
- Baseline branch: `main`
- Baseline commit: `1ecdb51be7c74f9a78f664d61110bb45479a1f5e`
- Tracked files at baseline: **361**
- **Baseline metadata note:** the commit and file count above are a historical snapshot, not a live `main` status. For current structural inventory, use [`docs/generated/SOURCE_INVENTORY.md`](generated/SOURCE_INVENTORY.md) and [`docs/generated/DOTNET_PROJECT_DEPENDENCIES.md`](generated/DOTNET_PROJECT_DEPENDENCIES.md); regenerate/check them with `python scripts/check_architecture_docs.py --write` and `python scripts/check_architecture_docs.py --check`. The reviewed workflow diagrams below remain human-maintained and must be checked against source after implementation changes.
- Purpose: map system boundaries, execution paths, dependencies, persistence, and code-file locations.
- Important limit: diagrams summarize and connect source files; they do not replace source code. The inventory at the end makes tracked files discoverable. Individual implementation details must be verified against each linked source file.
- End-to-end workflow handbook: [docs/WORKFLOW_UML_HANDBOOK.md](WORKFLOW_UML_HANDBOOK.md) — master walkthrough, shared middleware/security foundation, sequence diagrams, participating-file registers, and verification checklists.

## 1. Locked-in product and architecture decisions

**Execution tracking:** the workflow-level source trace is in [`WORKFLOW_UML_HANDBOOK.md`](WORKFLOW_UML_HANDBOOK.md); prioritized fixes and the final verification order are in [`STABILIZATION_BACKLOG.md`](STABILIZATION_BACKLOG.md).

1. This is **one Smart AI Dashboard application** with a shared frontend and backend, not a collection of separately managed AI projects.
2. `ProjectId` in `AIJob` and the `Project` entity are legacy prototype concepts. Do not add project-management features to justify them. Trace references, then remove obsolete fields/entities with a safe EF Core migration after confirming no functional dependencies.
3. Text generation is asynchronous because it can take longer: persist job, enqueue, process in `AIWorker`, persist status/result, poll status.
4. Text and document summarization are currently synchronous: call the AI service in the request handler and return the result. This is intentional for current workloads.
5. Local queue/storage are acceptable for local development. Cloud evolution is planned, not a reason for premature redesign: local files → S3; in-memory queue → SQS; containerized API/worker; managed database/cache as selected during deployment design.
6. Finish code consistency and essential end-to-end verification before deployment. Keep the implementation simple; no Kubernetes, Kafka, or microservice split unless a demonstrated requirement justifies it.

## 2. Level 0 — Master system map

```mermaid
flowchart TB
  USER["User / Browser"]
  subgraph FE["Angular 21 frontend"]
    ROUTES["Routes + auth/guest guards"]
    LAYOUT["MainLayoutComponent"]
    PAGES["Dashboard · Generate · Summarize · Job History"]
    DOCUI["Document List · Document Details"]
    SERVICES["Core services: Auth · Token · AI · Documents"]
    HTTP["HTTP interceptors: auth · loading · errors"]
    SHARED["Shared UI: loading overlay · cards · result viewer · dialogs · snackbar"]
    ROUTES --> LAYOUT
    LAYOUT --> PAGES
    LAYOUT --> DOCUI
    PAGES --> SERVICES
    DOCUI --> SERVICES
    PAGES --> SHARED
    DOCUI --> SHARED
    SERVICES --> HTTP
  end
  subgraph API["ASP.NET Core API"]
    PIPE["Program.cs: DI · middleware · CORS · JWT · rate limits · health checks"]
    CTRLS["Controllers: Auth · Users · AI · Documents"]
    MED["MediatR"]
    PIPE --> CTRLS --> MED
  end
  subgraph APP["Application"]
    USECASES["Commands / queries / handlers"]
    PORTS["Interfaces · DTOs · validators · exceptions · response models"]
    MED --> USECASES
    USECASES --> PORTS
  end
  subgraph DOMAIN["Core domain"]
    ENT["User · AIJob · Document · DocumentContent · AIResult · Project (verify legacy usage)"]
    ENUMS["AIJobType · AIJobStatus · Roles"]
    ENT --> ENUMS
  end
  subgraph INFRA["Infrastructure"]
    JOBS["AIJobStore · AIQueue · AIWorker"]
    AISVC["AiService · GeminiProvider / OpenAIProvider"]
    RES["Polly retry · circuit breaker · timeout"]
    CACHE["CacheService"]
    DOCS["LocalDocumentStorage · PdfTextExtractor · repositories"]
    AUTH["JwtTokenService · CurrentUserService"]
  end
  subgraph RESOURCES["Local / external resources"]
    PG[("PostgreSQL via EF Core")]
    REDIS[("Redis")]
    FS[("Local document files")]
    GEMINI["Gemini API"]
  end
  USER --> ROUTES
  HTTP --> PIPE
  USECASES --> ENT
  USECASES --> JOBS
  USECASES --> AISVC
  USECASES --> DOCS
  USECASES --> AUTH
  JOBS --> PG
  JOBS --> AISVC
  AISVC --> CACHE --> REDIS
  AISVC --> RES --> GEMINI
  DOCS --> PG
  DOCS --> FS
  AUTH --> PG
```

### Layer rules

- **Frontend:** route and view state; calls REST endpoints through typed services.
- **API:** HTTP boundary, middleware, auth configuration, routing, status codes and request/response serialization.
- **Application:** use-case orchestration through MediatR; depends on abstractions, not concrete storage/provider implementations.
- **Core:** domain entities and enums.
- **Infrastructure:** implementations of interfaces, EF Core, file storage, AI provider, Redis, resilience and worker.
- **External resources:** PostgreSQL, Redis and Gemini are separate runtime dependencies. Local files and in-memory queue are current local implementations.

## 3. Level 1 — Frontend route and service map

```mermaid
flowchart TD
  APP["main.ts → app.config.ts → app.routes.ts"]
  GUEST["guestGuard"]
  AUTH["authGuard + authGuard on child routes"]
  LOGIN["LoginComponent"]
  REGISTER["RegisterComponent"]
  MAIN["MainLayoutComponent"]
  DASH["DashboardComponent"]
  GEN["GenerateComponent"]
  SUM["SummarizeComponent"]
  HISTORY["JobHistoryComponent + JobDetailsDialog"]
  LIST["DocumentListComponent"]
  DETAIL["DocumentDetailsComponent"]
  AUTH_SERVICE["AuthService · TokenService · AuthStateService · CurrentUserService"]
  AI_SERVICE["AiService"]
  DOC_SERVICE["DocumentService"]
  INTERCEPTORS["authInterceptor · loadingInterceptor · errorInterceptor"]
  SHARED["Shared components/services: header, card, workspace, result viewer, dialogs, loading, snackbar, markdown"]
  APP --> GUEST --> LOGIN
  GUEST --> REGISTER
  APP --> AUTH --> MAIN
  MAIN --> DASH
  MAIN --> GEN
  MAIN --> SUM
  MAIN --> HISTORY
  MAIN --> LIST --> DETAIL
  LOGIN --> AUTH_SERVICE
  REGISTER --> AUTH_SERVICE
  DASH --> AI_SERVICE
  GEN --> AI_SERVICE
  SUM --> AI_SERVICE
  HISTORY --> AI_SERVICE
  LIST --> DOC_SERVICE
  DETAIL --> DOC_SERVICE
  DETAIL --> AI_SERVICE
  AUTH_SERVICE --> INTERCEPTORS
  AI_SERVICE --> INTERCEPTORS
  DOC_SERVICE --> INTERCEPTORS
  MAIN --> SHARED
  GEN --> SHARED
  DETAIL --> SHARED
```

Routes defined in `frontend/src/app/app.routes.ts`:
- `/login`, `/register` — guest guarded.
- `/dashboard`, `/ai/generate`, `/summarize`, `/ai/jobs`, `/documents`, `/documents/:id` — authenticated layout.
- Unknown routes redirect to dashboard.

Key frontend source areas:
- App bootstrap/config: `frontend/src/main.ts`, `frontend/src/app/app.config.ts`, `frontend/src/app/app.routes.ts`.
- Core infrastructure: `frontend/src/app/core/guards/`, `core/interceptors/`, `core/services/`, `core/models/`.
- Features: `frontend/src/app/features/auth/`, `features/ai/`, `features/dashboard/`, `features/documents/`.
- Shared presentation: `frontend/src/app/shared/components/`, `frontend/src/app/shared/services/`.
- Environment/build: `frontend/src/environments/`, `frontend/angular.json`, `frontend/package.json`, `frontend/tsconfig.json`, `frontend/tsconfig.app.json`, `frontend/tsconfig.spec.json`, `frontend/src/styles.css`, `frontend/src/material-theme.scss`.

## 4. Level 1 — Backend dependency and request map

```mermaid
flowchart LR
  CLIENT["Angular HttpClient"]
  MIDDLE["Program.cs middleware<br/>exception · correlation ID · request logging · CORS · auth · rate limit"]
  CTRL["API Controllers"]
  MED["MediatR dispatch"]
  VAL["ValidationBehavior + FluentValidation"]
  HANDLER["Command/query handler"]
  INTERFACE["Application interface"]
  IMPL["Infrastructure implementation"]
  DOMAIN["Core entity / enum"]
  STORE[("PostgreSQL / Redis / local file / AI API")]
  CLIENT --> MIDDLE --> CTRL --> MED --> VAL --> HANDLER
  HANDLER --> INTERFACE
  HANDLER --> DOMAIN
  INTERFACE -. implemented by .-> IMPL
  IMPL --> STORE
```

Registration and bootstrap:
- `backend/API/Program.cs`: web host, logging, controllers, JSON enum conversion, Swagger, application/infrastructure/auth/health-check registrations, CORS, Redis cache registration, rate limiting, middleware pipeline, controller mapping.
- `backend/Application/DependencyInjection.cs`: MediatR handlers, FluentValidation validators, validation pipeline behavior.
- `backend/Infrastructure/DependencyInjection.cs`: EF Core/Npgsql, repositories, current-user service, local document storage, PDF extractor, AI service/provider, JWT service, cache service, HttpClient resilience policies, singleton in-memory queue and hosted AI worker.
- `backend/Infrastructure/Data/AppDbContext.cs`: entity sets, enum-to-string conversions, indexes and one-to-one document-content relation.

### Controller endpoint map (verify contracts against current source)

| Controller | Route | Purpose |
|---|---|---|
| AuthController | `POST /api/auth/login` | Validate credentials and issue JWT |
| AuthController | `POST /api/auth/register` | Register account |
| UsersController | `GET /api/users` | Admin-only user listing |
| UsersController | `POST /api/users` | Admin-only user creation |
| AIController | `POST /api/AI/generate` | Create queued generation job |
| AIController | `GET /api/AI/status/{jobId}` | Retrieve current user's job status/result |
| AIController | `GET /api/AI/jobs` | Paginated current user's jobs |
| AIController | `POST /api/AI/summarize` | Synchronous plain-text summary |
| AIController | `POST /api/AI/documents/{documentId}/summarize` | Synchronous document summary |
| AIController | `GET /api/AI/stats` | Current user's job statistics |
| AIController | `GET /api/AI/documents/{documentId}/summary` | Retrieve saved completed document summary |
| DocumentsController | `POST /api/Documents` | Upload document |
| DocumentsController | `GET /api/Documents` | Paginated document list |
| DocumentsController | `GET /api/Documents/{id}` | Download/preview original file |
| DocumentsController | `DELETE /api/Documents/{id}` | Delete document |
| DocumentsController | `POST /api/Documents/{id}/extract` | Extract and persist text |
| DocumentsController | `GET /api/Documents/{id}/content` | Retrieve extracted text |

## 5. Sequence — asynchronous text generation

```mermaid
sequenceDiagram
  actor U as User
  participant UI as GenerateComponent
  participant S as Angular AiService
  participant C as AIController
  participant M as MediatR
  participant H as GenerateTextCommandHandler
  participant DB as AIJobStore / PostgreSQL
  participant Q as AIQueue (memory)
  participant W as AIWorker
  participant A as Infrastructure AiService
  participant R as Redis
  participant P as GeminiProvider
  U->>UI: Submit prompt
  UI->>S: generate(request)
  S->>C: POST /api/AI/generate + JWT
  C->>M: GenerateTextCommand
  M->>H: Handle
  H->>DB: Persist Pending AIJob
  H->>Q: Enqueue job
  H-->>UI: Return job ID and status
  loop Until terminal status
    UI->>S: getStatus(jobId)
    S->>C: GET /api/AI/status/{jobId}
    C->>DB: GetJobAsync(jobId, currentUserId)
    DB-->>UI: Current status/result
  end
  W->>Q: Dequeue job
  W->>DB: Update Processing
  W->>A: ProcessAsync(AIRequest)
  A->>R: Get cache entry
  alt Cache hit
    R-->>A: Cached output
  else Cache miss
    A->>P: GenerateAsync(prompt)
    P-->>A: Output
    A->>R: Cache successful output (10 minutes)
  end
  A-->>W: AIProviderResponse
  W->>DB: Persist Completed result
```

Failure branch: worker increments retry count, records the job state, waits with exponential delay and re-enqueues while retries remain; otherwise it persists Failed status/error. On startup, the worker queries recoverable jobs from PostgreSQL. Verify exact transitions in `backend/Infrastructure/BackgroundServices/AIWorker.cs`.

**Important:** the database is durable; the current `ConcurrentQueue<AIJob>` is not. PostgreSQL recovery is a mitigation, not a distributed durable queue or an exactly-once guarantee.

## 6. Sequence — synchronous summarization

```mermaid
sequenceDiagram
  actor U as User
  participant UI as Summarize UI / DocumentDetailsComponent
  participant S as Angular AiService
  participant C as AIController
  participant M as MediatR
  participant H as Summarize handler
  participant D as Document repositories
  participant A as Infrastructure AiService
  participant R as Redis
  participant P as GeminiProvider
  participant DB as AIJobStore / PostgreSQL

  U->>UI: Request summary
  UI->>S: summarize(...) or summarizeDocument(id)
  S->>C: POST summary endpoint + JWT
  C->>M: SummarizeCommand / SummarizeDocumentCommand
  M->>H: Handle
  opt Document summary
    H->>D: Verify document belongs to current user
    H->>D: Load extracted text
  end
  H->>DB: Create Processing AIJob
  H->>A: ProcessAsync(input, Summarize)
  A->>R: Check cache
  alt Cache miss
    A->>P: Generate summary
    P-->>A: Summary
    A->>R: Cache output
  else Cache hit
    R-->>A: Cached summary
  end
  A-->>H: Output
  H->>DB: Persist Completed result
  H-->>UI: Summary response
```

This is intentionally synchronous for the current workload. The handler persists Completed or Failed state and returns the output directly. The Angular document-details component renders Markdown and separately reloads saved summaries on later visits.

## 7. Sequence — document upload, extraction and preview

```mermaid
sequenceDiagram
  actor U as User
  participant UI as DocumentList / DocumentDetails
  participant S as DocumentService
  participant C as DocumentsController
  participant M as MediatR
  participant H as Upload/Extract handler
  participant FS as IDocumentStorage
  participant EX as IDocumentTextExtractor
  participant DR as DocumentRepository
  participant CR as DocumentContentRepository
  participant DB as PostgreSQL
  U->>UI: Upload PDF
  UI->>S: upload(file) multipart/form-data
  S->>C: POST /api/Documents + JWT
  C->>M: UploadDocumentCommand
  M->>H: Handle
  H->>FS: SaveAsync
  H->>DR: Add document metadata with UserId
  DR->>DB: SaveChanges
  H-->>UI: Document metadata / ID
  UI->>S: download(id) for PDF preview
  S->>C: GET /api/Documents/{id}
  C->>DR: Find document by ID and current user
  C-->>UI: File response
  U->>UI: Extract text
  UI->>S: extract(id)
  S->>C: POST /api/Documents/{id}/extract
  C->>M: ExtractDocumentCommand
  M->>H: Handle
  H->>DR: Verify ownership
  H->>FS: OpenReadAsync
  H->>EX: ExtractTextAsync
  H->>CR: Add or update DocumentContent
  CR->>DB: Persist extracted text
  UI->>S: getContent(id)
  S->>C: GET /api/Documents/{id}/content
  C-->>UI: Extracted text
```

Ownership invariant: handlers must verify document ownership before using document IDs to retrieve content, delete files, extract text or summarize content. Keep this check when adding endpoints.

Current implementation note: Document Details finds the metadata by loading page 1 of up to 100 documents. This is a known improvement, not a reason to block architecture documentation.

## 8. Class / dependency diagram — AI worker and provider boundary

```mermaid
classDiagram
  class IAIQueue {
    <<interface>>
    +Enqueue(AIJob)
    +Dequeue() AIJob
  }
  class AIQueue {
    -ConcurrentQueue~AIJob~ queue
    +Enqueue(AIJob)
    +Dequeue() AIJob
  }
  class AIWorker {
    -IAIQueue queue
    -IServiceScopeFactory scopeFactory
    +ExecuteAsync(token)
  }
  class IAIJobStore {
    <<interface>>
    +AddJobAsync(job)
    +GetJobAsync(id,userId)
    +GetJobsAsync(page,size,userId)
    +UpdateJobAsync(job)
    +GetRecoverableJobsAsync()
  }
  class AIJobStore {
    -AppDbContext dbContext
  }
  class IAIService {
    <<interface>>
    +ProcessAsync(request,token)
  }
  class AiService {
    -IAIProvider provider
    -ICacheService cache
  }
  class IAIProvider {
    <<interface>>
    +GenerateAsync(prompt,token)
  }
  class GeminiProvider
  class OpenAIProvider
  class ICacheService {
    <<interface>>
    +GetAsync(key)
    +SetAsync(key,value,ttl)
  }
  class CacheService
  class AppDbContext
  IAIQueue <|.. AIQueue
  IAIJobStore <|.. AIJobStore
  IAIService <|.. AiService
  IAIProvider <|.. GeminiProvider
  IAIProvider <|.. OpenAIProvider
  ICacheService <|.. CacheService
  AIWorker --> IAIQueue
  AIWorker --> IAIJobStore
  AIWorker --> IAIService
  AiService --> IAIProvider
  AiService --> ICacheService
  AIJobStore --> AppDbContext
  AIQueue ..> AIJob
```

The currently registered provider is `GeminiProvider`; `OpenAIProvider` exists but is not currently registered as automatic fallback in the inspected infrastructure registration. Do not assume failover occurs just because both implementations exist.

## 9. Domain and data ownership map

```mermaid
erDiagram
  USER ||--o{ AI_JOB : owns
  USER ||--o{ DOCUMENT : owns
  DOCUMENT ||--o| DOCUMENT_CONTENT : has_extracted_text
  DOCUMENT ||--o{ AI_JOB : summarized_by
  USER {
    uuid Id PK
    string Email
    string PasswordHash
    string Role
  }
  AI_JOB {
    uuid Id PK
    uuid UserId FK
    uuid DocumentId FK
    uuid ProjectId "legacy prototype field"
    string JobType
    string Status
    string Prompt
    string Result
    string Error
    int RetryCount
    int MaxRetries
  }
  DOCUMENT {
    uuid Id PK
    uuid UserId FK
    string FileName
    string StoredFileName
    string StoragePath
    string ContentType
    bigint FileSize
  }
  DOCUMENT_CONTENT {
    uuid Id PK
    uuid DocumentId FK
    string ExtractedText
    datetime ExtractedAt
  }
```

Treat the diagram as a logical ownership map. Confirm exact database foreign-key constraints and deletion behavior against every migration and the current model snapshot before making schema changes. `ProjectId` is not an active product requirement.

## 10. Reliability, cache and configuration

- **AI retry and circuit breaker:** `backend/Infrastructure/Resilience/AIResiliencePolicy.cs`, attached to named `AIClient` with an HTTP timeout in `Infrastructure/DependencyInjection.cs`. Inspect actual provider HTTP usage and policy behavior when validating retry semantics.
- **Cache:** `AiService` derives a cache key from job type and input, tolerates cache GET/SET exceptions, and caches successful outputs for ten minutes. Cache-key privacy/length and provider/model versioning are later hardening considerations, not a reason to add architecture now.
- **Logging:** Serilog console and rolling JSON file sinks in `backend/API/Program.cs`; correlation ID and request logging middleware are registered.
- **Health checks:** custom health-check registration and mapping in `backend/API/Extensions/HealthCheckExtensions.cs`.
- **Secrets/config:** inspect `backend/API/appsettings.json`, `appsettings.Development.json`, environment variables and JWT/provider settings. Never commit real credentials. Replace local configuration with managed secret injection in cloud.
- **Rate limiting:** concurrency and fixed-window policies configured in `Program.cs`; the AI generation endpoint has an explicit fixed-window attribute.
- **Database:** EF Core/Npgsql and migration history under `backend/Infrastructure/Migrations/`.
- **Current storage boundary:** `IDocumentStorage` / `LocalDocumentStorage` is the replacement point for future S3 implementation.
- **Current queue boundary:** `IAIQueue` / `AIQueue` is the replacement point for future SQS integration.

## 11. Stabilization and cloud sequence

1. Complete file-by-file dependency and endpoint mapping; confirm no unaccounted runtime path.
2. Remove obsolete project prototype fields/entities only after reference and migration checks.
3. Verify end-to-end login, generation/recovery, summarization, upload, extraction, preview, delete, history and ownership isolation.
4. Fix only confirmed correctness/security blockers and record non-blocking improvements.
5. Containerize the API and frontend/serving setup; separate API and worker runtime roles where appropriate.
6. Deploy the first working version to AWS with appropriate IAM, networking, secret management and logging.
7. Replace local document storage with S3 and in-memory queue with SQS at the planned integration stage; test retries, duplicates and recovery.
8. Add cloud monitoring, CI/CD and cost controls; expand into RAG/agents/multimodal capabilities only after the base system is stable.

## Diagram maintenance

See [Diagram Maintenance Strategy](DIAGRAM_MAINTENANCE.md) for the planned hybrid approach: generate mechanical inventories/structural maps, retain human-reviewed workflow sequence diagrams, and add automated drift checks. These checks are planned; do not assume they are already running.

## Current status of the standalone Python AI engine

The repository also contains a top-level `ai-engine/` FastAPI application with a Pegasus XSum Transformers summarizer. In the inspected current API/provider path, the ASP.NET Core application calls Gemini through its provider abstraction; no call from the .NET API to this FastAPI application was identified in the reviewed routes and provider files.

For now, treat `ai-engine/` as a separate research/prototype component—not as a required dependency of the current Smart AI Dashboard runtime. Keep it because multi-model AI experimentation is part of the broader learning goal, but make an explicit product/architecture decision before integrating or deploying it. If it becomes an active service, define its API contract, authentication/network boundary, model download/startup behavior, resource requirements, health checks, logging and failure handling.

The API also currently contains `backend/API/Controllers/TestController.cs`, including a deliberately throwing `GET /api/test/error` endpoint without an authorization attribute. This is a development diagnostic, not a product feature; remove it or restrict it to Development before production deployment.

## 12. Current tracked-file inventory

The original inventory snapshot has been replaced by a generated, current-branch index so new files do not leave this blueprint stale:

- [Generated Repository Source Inventory](generated/SOURCE_INVENTORY.md)
- [Generated .NET Project Dependency Map](generated/DOTNET_PROJECT_DEPENDENCIES.md)
- Generator and validator: `scripts/check_architecture_docs.py`
- GitHub Actions check: `.github/workflows/architecture-docs.yml`

The generated inventory lists tracked paths from the checked-out Git tree. It helps with discovery but does not replace source review or runtime verification.

## 13. Change-control rule for this blueprint

For every implementation change:
1. Identify the affected files in the inventory.
2. Update the relevant sequence/class/dependency diagram and endpoint/data contracts.
3. Record decisions, migration implications and tests.
4. Re-check the baseline SHA and update the document's baseline after the corresponding code change lands.
5. Do not mark a subsystem fully verified until its relevant source paths and tests have been inspected and executed.

---
*Generated for the current repository baseline. This is a living architecture document, not a claim that every feature has already passed runtime tests.*
