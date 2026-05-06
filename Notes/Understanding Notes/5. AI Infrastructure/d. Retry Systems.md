# Retry Systems Understanding

# Files Involved

## Infrastructure Layer

- `Infrastructure/BackgroundServices/AIWorker.cs`
- `Infrastructure/Resilience/AIResiliencePolicy.cs`
- `Infrastructure/AI/AiService.cs`
- `Infrastructure/AI/Providers/OpenAIProvider.cs`

## Application Layer

- `Application/Interfaces/IAIQueue.cs`
- `Application/Interfaces/IAIJobStore.cs`

## Core Layer

- `Core/Entities/AIJob.cs`

## API Layer

- `API/Program.cs`

---

# Layer

```text
Infrastructure Resilience Layer
```

Retry systems belong to Infrastructure because:
- transient failure handling
- resilience
- fault tolerance
- external service recovery

are infrastructure concerns.

---

# Purpose

Retry systems allow applications to:

```text
Recover automatically from temporary failures.
```

Instead of failing immediately,
system retries operations intelligently.

---

# Why Retry Systems Are Important

Modern systems depend on:
- external APIs
- databases
- AI providers
- cloud services
- networks

Temporary failures are extremely common.

Examples:
- network timeout
- rate limiting
- temporary outage
- transient DB issue

Without retries:
small temporary issues become user-facing failures.

---

# Current Backend Retry Architecture

Current backend has TWO retry systems:

| Retry System     | Purpose                    |
|------------------|----------------------------|
| AIWorker retries | retry failed AI jobs       |
| Polly retries    | retry failed HTTP requests |

Very important distinction.

---

# Retry Architecture Overview

```text
AI Job
↓
AIWorker executes
↓
AI provider call fails
↓
Polly retries HTTP call
↓
If still fails
↓
AIWorker retries full job
```

Current backend already demonstrates layered resilience architecture.

---

# Retry System #1 — AIWorker Retries

# File

```text
Infrastructure/BackgroundServices/AIWorker.cs
```

Purpose:
- retry full AI jobs

---

# Retry Fields Inside AIJob

# File

```text
Core/Entities/AIJob.cs
```

```csharp
public int RetryCount { get; set; } = 0;

public int MaxRetries { get; set; } = 3;
```

Purpose:
- track retry attempts
- limit infinite retry loops

---

# AIWorker Failure Flow

Inside worker:

```csharp
catch (Exception ex)
{
    ...
}
```

Any AI processing failure enters retry flow.

---

# RetryCount Increment

```csharp
job.RetryCount++;
```

Tracks number of failed attempts.

---

# Retry Decision Logic

```csharp
if (job.RetryCount < job.MaxRetries)
```

If retries remain:
job requeued.

Else:
job permanently failed.

---

# Retrying Flow

If retries remain:

```csharp
job.Status = "Retrying";
```

Then:

```csharp
_queue.Enqueue(job);
```

Very important architecture understanding:

```text
Worker retries ENTIRE job lifecycle.
```

---

# Retry Lifecycle

```text
Job Processing
↓
Failure occurs
↓
RetryCount++
↓
Job requeued
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

Then:

```csharp
_jobStore.Update(job);
```

Purpose:
- preserve failure visibility
- avoid infinite loops
- expose errors to clients

---

# Why Retry Limits Matter

Without limits:

```text
Infinite retry loops possible.
```

Problems:
- CPU waste
- queue congestion
- cascading failures
- infrastructure overload

Retry limits prevent this.

---

# Retry System #2 — Polly HTTP Retries

# File

```text
Infrastructure/Resilience/AIResiliencePolicy.cs
```

Purpose:
- retry transient HTTP failures automatically

---

# HttpClient Retry Registration

# File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddHttpClient("AIClient")
    .AddPolicyHandler(
        AIResiliencePolicy.GetRetryPolicy()
    );
```

Meaning:
all AI HTTP calls automatically use retry policy.

---

# Polly Retry Policy Understanding

Inside:

```csharp
GetRetryPolicy()
```

```csharp
HttpPolicyExtensions
    .HandleTransientHttpError()
```

Retries:
- HTTP 5xx
- network failures
- timeouts

---

# Rate Limit Retry Handling

Additional retry condition:

```csharp
.OrResult(
    msg => msg.StatusCode ==
        HttpStatusCode.TooManyRequests
)
```

Handles:

```http
429 Too Many Requests
```

Very important for AI APIs.

---

# Exponential Backoff Understanding

Current retry strategy:

```csharp
retryAttempt =>
    TimeSpan.FromSeconds(
        Math.Pow(2, retryAttempt)
    )
```

Retry delays:

| Attempt | Delay |
|---------|-------|
| 1       | 2 sec |
| 2       | 4 sec |
| 3       | 8 sec |

---

# Why Exponential Backoff Matters

BAD retry behavior:

```text
Retry immediately repeatedly
```

Problems:
- API overload
- cascading failures
- aggressive traffic spikes

Exponential backoff:
- reduces pressure
- allows recovery time
- improves resilience

Very important distributed systems concept.

---

# Retry Layer Separation

Current backend has layered retry strategy:

| Layer     | Responsibility   |
|-----------|------------------|
| Polly     | retry HTTP calls |
| AIWorker  | retry full jobs  |

Excellent architecture separation.

---

# Example Failure Scenario

```text
AIWorker executes job
↓
OpenAIProvider makes HTTP request
↓
External API temporarily unavailable
↓
Polly retries HTTP request automatically
↓
Still fails
↓
Exception thrown
↓
AIWorker catches exception
↓
Job requeued
↓
Worker retries later
```

Very strong resilience architecture foundation.

---

# Why Multiple Retry Layers Matter

Different failures require different recovery strategies.

| Failure Type          | Best Retry Layer |
|-----------------------|------------------|
| temporary HTTP issue  | Polly            |
| full workflow failure | AIWorker         |
| DB reconnect          | DB retry layer   |

Enterprise systems often have multiple resilience layers.

---

# Retry vs Circuit Breaker

Important distinction:

| System          | Purpose                 |
|-----------------|-------------------------|
| Retry           | try again               |
| Circuit Breaker | temporarily stop trying |

Retries recover from temporary failures.

Circuit breakers prevent system overload.

---

# Logging Retry Failures

Worker logs retry attempts:

```csharp
_logger.LogWarning(...)
```

Purpose:
- observability
- debugging
- failure tracing

Very important production feature.

---

# Important Variables Understanding

| Variable       | Purpose                |
|----------------|------------------------|
| `RetryCount`   | current retry attempts |
| `MaxRetries`   | retry limit            |
| `job`          | AI processing task     |
| `retryAttempt` | Polly retry iteration  |
| `_queue`       | requeue failed jobs    |

---

# Production Relevance

Retry systems critical in:
- AI systems
- cloud-native systems
- distributed systems
- microservices
- external API integrations

because transient failures are unavoidable in real-world systems.

---

# Most Important Architectural Understanding

Retries create:

```text
Fault Tolerance
```

Current backend correctly separates:
- infrastructure retries
- workflow retries
- HTTP retries

This creates:
- resilient AI execution
- recoverable workflows
- production-grade reliability foundations