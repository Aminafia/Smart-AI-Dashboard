# Background Workers Understanding

# Files Involved

## Infrastructure Layer

- `Infrastructure/BackgroundServices/AIWorker.cs`
- `Infrastructure/DependencyInjection.cs`

## Application Layer

- `Application/Interfaces/IAIQueue.cs`
- `Application/Interfaces/IAIJobStore.cs`
- `Application/Interfaces/IAIService.cs`

## Infrastructure Services

- `Infrastructure/Services/AIQueue.cs`
- `Infrastructure/Services/AIJobStore.cs`
- `Infrastructure/AI/AiService.cs`

## Core Layer

- `Core/Entities/AIJob.cs`

---

# Layer

```text
Infrastructure Layer
```

Background workers belong to Infrastructure because:
- background execution
- async processing
- queue consumers
- long-running services

are infrastructure concerns.

---

# Purpose

Background workers allow tasks to execute:

```text
Outside HTTP request lifecycle
```

They continuously run in the background while application is alive.

---

# Why Background Workers Are Important

Without background workers:

```text
Heavy operations would block API requests.
```

Problems:
- request timeouts
- slow APIs
- blocked threads
- poor scalability

Background workers solve this using:
- async processing
- queue consumption
- independent execution loops

---

# Current Worker Architecture

Current backend uses:

```text
AIWorker
```

to process AI jobs asynchronously.

Architecture flow:

```text
HTTP Request
↓
Job queued
↓
Background Worker picks job later
↓
AI processing executes independently
```

---

# BackgroundService Understanding

# File

```text
Infrastructure/BackgroundServices/AIWorker.cs
```

```csharp
public class AIWorker : BackgroundService
```

`BackgroundService`
is ASP.NET Core abstraction for:
- hosted services
- continuously running background tasks

---

# Hosted Service Registration

# File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddHostedService<AIWorker>();
```

Purpose:
- register worker into ASP.NET Core host lifecycle

When application starts:
worker automatically starts.

When application stops:
worker gracefully shuts down.

---

# Worker Lifecycle Understanding

```text
Application Starts
↓
AIWorker starts automatically
↓
ExecuteAsync() begins
↓
Continuous processing loop
↓
Application Shutdown
↓
Worker cancellation triggered
```

---

# ExecuteAsync Understanding

Core worker logic:

```csharp
protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
```

This method runs continuously while application alive.

---

# CancellationToken Understanding

Variable:

```csharp
stoppingToken
```

Purpose:
- graceful shutdown support

When application shutting down:
token becomes cancelled.

Worker exits safely.

Very important production concept.

---

# Continuous Worker Loop

Inside worker:

```csharp
while (!stoppingToken.IsCancellationRequested)
```

Meaning:

```text
Keep processing forever
until application shutdown.
```

This is core background worker pattern.

---

# Queue Consumption Flow

Worker checks queue:

```csharp
var job = _queue.Dequeue();
```

Current implementation uses:

```text
ConcurrentQueue<AIJob>
```

Worker acts as:

```text
Queue Consumer
```

---

# Empty Queue Handling

If no jobs available:

```csharp
if (job is null)
{
    await Task.Delay(500, stoppingToken);
    continue;
}
```

Purpose:
- avoid CPU overuse
- avoid infinite tight polling loops

Very important optimization concept.

---

# Job Processing Lifecycle

When job found:

```csharp
job.Status = "Processing";
```

Then:

```csharp
_jobStore.Update(job);
```

Purpose:
- expose current execution state

---

# IServiceScopeFactory Understanding

Very important DI lifetime concept.

Injected dependency:

```csharp
private readonly IServiceScopeFactory _scopeFactory;
```

---

# Why IServiceScopeFactory Is Needed

`AIWorker`
is effectively:

```text
Singleton
```

because hosted services live entire application lifetime.

BUT:

```csharp
IAIService
```

registered as:

```text
Scoped
```

Problem:

```text
Singleton cannot directly depend on Scoped service.
```

---

# Scoped Resolution Flow

Solution:

```csharp
using var scope =
    _scopeFactory.CreateScope();
```

Then:

```csharp
var aiService =
    scope.ServiceProvider
        .GetRequiredService<IAIService>();
```

Purpose:
- create temporary scoped service safely
- isolate dependencies properly

This is extremely important enterprise DI understanding.

---

# AI Request Creation

Worker creates:

```csharp
var request = new AIRequest
{
    Prompt = job.Prompt
};
```

Transforms queued job into AI workflow request.

---

# AI Execution Flow

Worker calls:

```csharp
await aiService.GenerateAsync(request);
```

Execution moves into:

```text
Infrastructure/AI/AiService.cs
```

---

# Successful Job Completion

If AI execution succeeds:

```csharp
job.Result = response.Output;

job.Status = "Completed";

job.CompletedAt = DateTime.UtcNow;
```

Then:

```csharp
_jobStore.Update(job);
```

---

# Exception Handling In Worker

Worker wraps processing inside:

```csharp
try
{
    ...
}
catch (Exception ex)
{
    ...
}
```

Purpose:
- prevent worker crash
- isolate job failures
- support retries

Very important production concept.

---

# Retry System Understanding

On failure:

```csharp
job.RetryCount++;
```

Worker checks:

```csharp
if (job.RetryCount < job.MaxRetries)
```

If retries remain:
- job requeued

Else:
- marked failed

---

# Retry Flow

```text
Job fails
↓
RetryCount++
↓
Status = Retrying
↓
Queue again
↓
Worker retries later
```

---

# Permanent Failure Flow

If retries exhausted:

```csharp
job.Status = "Failed";

job.Error = "...";
```

Purpose:
- preserve failure information
- allow client visibility
- improve observability

---

# Logging Understanding

Worker logs:
- retries
- failures
- processing events

Example:

```csharp
_logger.LogWarning(...)
```

and:

```csharp
_logger.LogError(...)
```

Very important for production debugging.

---

# Worker vs HTTP Request Lifecycle

Very important architecture understanding.

HTTP requests are:

```text
Short-lived
```

Workers are:

```text
Long-running
```

Workers continue processing independently even after HTTP response already returned.

---

# Current Backend Architecture Advantage

This architecture allows:

```text
Client receives response immediately
```

while:
- AI processing
- retries
- caching
- provider calls

continue independently in background.

---

# Important Variables Understanding

| Variable        | Purpose                    |
|-----------------|----------------------------|
| `stoppingToken` | graceful shutdown          |
| `_queue`        | queued jobs                |
| `_jobStore`     | job tracking               |
| `_scopeFactory` | scoped dependency creation |
| `job`           | AI processing task         |
| `RetryCount`    | retry tracking             |

---

# Production Relevance

Background workers heavily used in:
- AI systems
- email systems
- notification systems
- event-driven systems
- distributed systems
- cloud-native platforms

because expensive workloads should execute asynchronously.

---

# Most Important Architectural Understanding

Background workers create:

```text
Independent execution infrastructure
```

This separates:
- user-facing request handling
FROM
- heavy background processing

Current backend already demonstrates:
- queue-consumer architecture
- async execution
- retry handling
- scoped dependency resolution
- resilient worker processing

which are strong enterprise backend concepts.