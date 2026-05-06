# Logging + Serilog Understanding

# Files Involved

## API Layer

- `API/Program.cs`
- `API/Middlewares/RequestLoggingMiddleware.cs`
- `API/Middlewares/CorrelationIdMiddleware.cs`
- `API/Controllers/AuthController.cs`

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Features/AI/Commands/GenerateAI/GenerateAICommandHandler.cs`

## Infrastructure Layer

- `Infrastructure/AI/Providers/OpenAIProvider.cs`
- `Infrastructure/BackgroundServices/AIWorker.cs`

## Logs

- `API/logs/log-*.txt`

## Project Configuration

- `API/API.csproj`
- `appsettings.json`

---

# Layer

```text
Cross-cutting Observability Infrastructure
```

Logging spans the entire backend because:
- requests
- workflows
- AI jobs
- middleware
- exceptions
- infrastructure operations

all need traceability.

---

# Purpose

Logging provides:

```text
Runtime visibility into system behavior.
```

Without logging:
- debugging becomes extremely difficult
- production monitoring becomes impossible
- failures become invisible

---

# What Is Observability?

Observability means:

```text
Ability to understand internal system state
using external outputs.
```

Main observability pillars:
- logs
- metrics
- traces

Current backend already implements:
- structured logs
- request tracing
- correlation IDs

---

# Why Logging Is Important

Production systems need logging for:
- debugging
- monitoring
- auditing
- tracing
- performance analysis
- incident investigation

---

# Serilog Understanding

Current backend uses:

```text
Serilog
```

from:

```text
API/API.csproj
```

Packages:

```xml
Serilog.AspNetCore

Serilog.Sinks.Console

Serilog.Sinks.File
```

---

# Why Serilog Instead Of Console.WriteLine

BAD:

```csharp
Console.WriteLine(...)
```

Problems:
- unstructured
- difficult to search
- difficult to filter
- poor production support

Serilog provides:
- structured logging
- sinks
- enrichment
- formatting
- centralized logging support

---

# Serilog Configuration Flow

# File

```text
API/Program.cs
```

---

# Step 1 — Logger Configuration

```csharp
Log.Logger = new LoggerConfiguration()
```

Creates global Serilog logger.

---

# Step 2 — Minimum Log Level

```csharp
.MinimumLevel.Information()
```

Meaning:
only logs at:
- Information
- Warning
- Error
- Fatal

will be stored.

---

# Step 3 — Log Context Enrichment

```csharp
.Enrich.FromLogContext()
```

Allows additional metadata:
- correlation IDs
- request metadata
- contextual information

to flow automatically through logs.

---

# Step 4 — Console Sink

```csharp
.WriteTo.Console()
```

Logs printed to terminal during development.

Useful for:
- local debugging
- development tracing

---

# Step 5 — File Sink

```csharp
.WriteTo.File(
    "logs/log-.txt",
    rollingInterval: RollingInterval.Day
)
```

Purpose:
- persist logs into files
- create daily rolling log files

Generated example:

```text
logs/log-20260505.txt
```

---

# Step 6 — Register Serilog

```csharp
builder.Host.UseSerilog();
```

ASP.NET Core now uses Serilog as global logging provider.

---

# Logging Flow In Current Backend

```text
Request arrives
↓
Middleware logs request
↓
Handler logs workflow steps
↓
Repository/AI services log operations
↓
Exceptions logged
↓
Response logged
↓
Logs written to console + file
```

---

# Request Logging Middleware

# File

```text
API/Middlewares/RequestLoggingMiddleware.cs
```

Purpose:
- log incoming requests
- log outgoing responses
- measure duration
- log failures

---

# Incoming Request Logging

Inside middleware:

```csharp
Log.Information(
    "[Middleware] Incoming Request..."
)
```

Logs:
- HTTP method
- request path
- correlation ID

Example:

```text
POST /api/auth/login
```

---

# Stopwatch Understanding

```csharp
var stopwatch = Stopwatch.StartNew();
```

Measures request duration.

Very important for:
- performance monitoring
- latency analysis
- bottleneck detection

---

# Response Logging

After request completes:

```csharp
Log.Information(
    "[Middleware] Response..."
)
```

Logs:
- status code
- execution time
- correlation ID

---

# Exception Logging

Inside:

```csharp
catch (Exception ex)
```

Middleware logs:

```csharp
Log.Error(ex, ...)
```

Important understanding:

Structured exception logging stores:
- stack traces
- exception messages
- contextual metadata

---

# Controller Logging

# File

```text
API/Controllers/AuthController.cs
```

Example:

```csharp
Log.Information(
    "[Controller] Login endpoint hit"
);
```

Purpose:
- track controller execution
- trace API usage

---

# Handler Logging

# File

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

Example:

```csharp
_logger.LogInformation(
    "[Handler] Fetching user from DB"
);
```

Purpose:
- trace business workflow execution
- debug workflow steps

---

# AI Job Logging

# File

```text
Infrastructure/BackgroundServices/AIWorker.cs
```

Example:

```csharp
_logger.LogWarning(
    ex,
    "Job {JobId} failed..."
);
```

Purpose:
- trace async background processing
- monitor retries
- debug AI failures

---

# Structured Logging Understanding

Example:

```csharp
_logger.LogInformation(
    "Cache HIT for key {Key}",
    cacheKey
);
```

Important:

```text
{Key}
```

is structured property,
NOT string interpolation.

Benefits:
- searchable logs
- filterable logs
- machine-readable metadata

Very important enterprise logging concept.

---

# Logging Levels Understanding

| Level       | Purpose             |
|-------------|---------------------|
| Information | normal operations   | 
| Warning     | recoverable issues  |
| Error       | failures/exceptions |
| Fatal       | application crash   |

---

# Correlation ID Integration

Current backend combines:
- logging
- correlation IDs

Example:

```text
CorrelationId: abc-123
```

Purpose:
- trace single request across entire system

Extremely important in:
- microservices
- distributed systems
- async systems

---

# AI Logging Flow Example

```text
GenerateAICommandHandler
↓
AIWorker
↓
AiService
↓
OpenAIProvider
↓
CacheService
```

All layers can log using same correlation/tracing model.

---

# Important Variables Understanding

| Variable       | Purpose                 |
|----------------|-------------------------|
| `Log.Logger`   | global Serilog logger   |
| `_logger`      | injected typed logger   |
| `stopwatch`    | request timing          |
| `correlationId`| request tracing         |
| `cacheKey`     | structured log property |

---

# appsettings.json Logging Configuration

## File

```text
appsettings.json
```

```json
"Logging": {
  "LogLevel": {
    "Default": "Information"
  }
}
```

Controls ASP.NET Core logging verbosity.

---

# Production Relevance

Logging is absolutely critical in:
- enterprise APIs
- cloud-native systems
- AI platforms
- distributed systems
- microservices

because production debugging depends heavily on logs.

---

# Most Important Architectural Understanding

Logging is a:

```text
Cross-cutting concern
```

It should be:
- centralized
- structured
- consistent
- traceable

Current backend correctly places logging across:
- middleware
- handlers
- infrastructure
- background workers

creating strong observability foundations.