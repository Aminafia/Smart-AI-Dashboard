# Smart AI Dashboard

Smart AI Dashboard is a full-stack learning and engineering project that brings AI capabilities into one application. The current implementation combines an Angular frontend, an ASP.NET Core API using Clean Architecture and MediatR, PostgreSQL persistence, Redis caching, background AI jobs, and local document storage.

The immediate goal is to stabilize and verify the local application end-to-end. AWS deployment follows after that baseline is understood and tested.

## Current capabilities

- User registration and JWT-based login
- Asynchronous text generation with persisted job status and frontend polling
- Synchronous plain-text summarization
- PDF upload, preview, text extraction, and document summarization
- User-scoped job history and document operations
- Admin user operations and health endpoints

## Architecture at a glance

- **Frontend:** Angular 21, standalone components, Angular Material, Tailwind CSS, RxJS
- **Backend:** ASP.NET Core / .NET 8, Clean Architecture, MediatR, CQRS-style handlers
- **Database:** PostgreSQL with EF Core migrations
- **Cache:** Redis
- **AI provider:** Google Gemini through the provider abstraction
- **Background processing:** hosted `AIWorker` with a process-local in-memory queue
- **Document storage:** local filesystem through the document storage abstraction

The current queue and local storage are development-stage implementation choices. They are not equivalent to a durable distributed queue or object storage service.

## Documentation

- [Architecture Blueprint](docs/ARCHITECTURE_BLUEPRINT.md)
- [Workflow UML Handbook](docs/WORKFLOW_UML_HANDBOOK.md)
- [Stabilization Backlog](docs/STABILIZATION_BACKLOG.md)
- [Diagram Maintenance Strategy](docs/DIAGRAM_MAINTENANCE.md)

Read the architecture blueprint first for the overall system map, then use the workflow handbook to follow each request through the frontend, API, application handlers, infrastructure and persistence.

## Prerequisites

Install and prepare:

- .NET SDK **8.0.419**, as pinned in `global.json`
- Node.js compatible with the Angular 21 toolchain and npm **11.5.2** (the package manager version recorded in `frontend/package.json`)
- PostgreSQL
- Redis
- A Google Gemini API key for AI provider requests

Create a local PostgreSQL database and ensure PostgreSQL and Redis are running before starting the API.

## Local configuration

The API reads configuration from standard ASP.NET Core configuration sources. Do not commit real credentials or production secrets.

From the repository root, initialize User Secrets for the API project:

```powershell
dotnet user-secrets init --project backend/API/API.csproj
```

Set your local values (replace the example connection string and secret with values for your machine):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SmartAIDashboardDb;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD" --project backend/API/API.csproj
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379" --project backend/API/API.csproj
dotnet user-secrets set "JwtSettings:Secret" "REPLACE_WITH_A_LONG_RANDOM_LOCAL_SECRET" --project backend/API/API.csproj
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY" --project backend/API/API.csproj
```

Use a sufficiently long, randomly generated JWT signing secret. User Secrets are for local development only; use an appropriate managed secret mechanism in deployment. The API startup checks that PostgreSQL and Redis connection-string settings are present.

The configured CORS origin for local Angular development is `http://localhost:4200`. If you use another frontend origin, update the local configuration accordingly.

## Apply database migrations

From the repository root:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update --project backend/Infrastructure/Infrastructure.csproj --startup-project backend/API/API.csproj
```

If `dotnet-ef` is already installed, skip the install command. Review the current migrations and target database before applying changes. Use a disposable database when validating migrations.

## Run the backend

From the repository root:

```powershell
dotnet restore backend/BackendSmartAIDashboard.sln
dotnet build backend/BackendSmartAIDashboard.sln
dotnet run --project backend/API/API.csproj
```

Swagger is configured for the Development environment. Use the actual URL printed by ASP.NET Core at startup rather than assuming a fixed port.

## Run the frontend

In a second terminal:

```powershell
cd frontend
npm ci
npm start
```

Open the local URL printed by Angular CLI (normally `http://localhost:4200`).

## Tests and verification

The frontend package defines these scripts:

```powershell
cd frontend
npm test
npm run build
```

The application has been manually tested multiple times during development. Intentional automated feature tests have not yet been written for either backend or frontend; the Angular spec files currently in the repository are generated scaffolding, not meaningful coverage. Automated tests are planned for a later phase. Use the workflow handbook and stabilization backlog to guide the eventual consolidated verification pass. A successful build alone does not establish runtime correctness.

## Current implementation limitations to keep in mind

- AI jobs are persisted in PostgreSQL, but dispatch currently uses an in-memory queue. Startup recovery mitigates some interruptions; it does not guarantee exactly-once processing.
- Uploaded documents are stored on the local filesystem, so deployment must account for writable persistent storage until the storage adapter is moved to object storage.
- PDF text extraction does not provide OCR for image-only/scanned pages.
- Runtime verification is still required for authentication, cross-user access boundaries, AI failure/retry behavior, document edge cases and UI update paths.

See [Stabilization Backlog](docs/STABILIZATION_BACKLOG.md) for the prioritized work list. Do not treat this project as production-ready until the security, reliability and end-to-end verification gates are completed.
